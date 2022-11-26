module Womb.Graphics.Primitives

#nowarn "9" // Unverifiable IL due to fixed expression and NativePtr library usage

open System.Numerics
open Womb.Backends.OpenGL.Api
open Womb.Backends.OpenGL.Api.Constants
open Womb.Types

type ShadedObjectContext =
  { VAO: uint;
    VBO: uint;
    EBO: uint;
    Vertices: array<single>;
    Indices: array<uint>; }

    static member Default =
      { VAO = 0u
        VBO = 0u
        EBO = 0u
        Vertices = Array.empty
        Indices = Array.empty }

    static member From (vertices) (indices) : ShadedObjectContext =
      let vao = glGenVertexArray()
      let vbo = glGenBuffer()
      let ebo = glGenBuffer()

      glBindVertexArray vao

      glBindBuffer
        GL_ARRAY_BUFFER
        vbo
      glBufferData<single>
        GL_ARRAY_BUFFER
        vertices
        GL_DYNAMIC_DRAW

      glBindBuffer
        GL_ELEMENT_ARRAY_BUFFER
        ebo
      glBufferData<uint>
        GL_ELEMENT_ARRAY_BUFFER
        indices
        GL_STATIC_DRAW

      glVertexAttribPointer
        0u
        3u
        GL_FLOAT
        false
        3
        0
      glEnableVertexAttribArray 0u

      { VAO = vao
        VBO = vbo
        EBO = ebo
        Vertices = vertices
        Indices = indices }

    static member UpdateIndices (indices) (context: ShadedObjectContext) : ShadedObjectContext =
      glBindBuffer
        GL_ELEMENT_ARRAY_BUFFER
        context.EBO
      glBufferSubData<uint>
        GL_ELEMENT_ARRAY_BUFFER
        0
        indices

      { context with
          Indices = indices }

    static member UpdateVertices (vertices) (context: ShadedObjectContext) : ShadedObjectContext =
      glBindBuffer
        GL_ARRAY_BUFFER
        context.VBO
      glBufferSubData<single>
        GL_ARRAY_BUFFER
        0
        vertices

      { context with
          Vertices = vertices }
    
    static member Update (vertices) (indices) (context: ShadedObjectContext) : ShadedObjectContext =
      ShadedObjectContext.UpdateVertices vertices context
        |> ShadedObjectContext.UpdateIndices indices

type ShadedObject =
  { Context: ShadedObjectContext;
    Shader: uint;
    FragmentShaderPaths: list<string>;
    VertexShaderPaths: list<string>; }

    static member Default =
      { Context = ShadedObjectContext.Default
        Shader = 0u
        FragmentShaderPaths = List.Empty
        VertexShaderPaths = List.Empty }

    static member From (primitive: ShadedObject) (vertices) (indices) : ShadedObject =
      { primitive with
          Context = ShadedObjectContext.From vertices indices }

    static member UpdateIndices (indices) (primitive: ShadedObject) : ShadedObject =
      { primitive with
          Context = ShadedObjectContext.UpdateIndices indices primitive.Context }

    static member UpdateVertices (vertices) (primitive: ShadedObject) : ShadedObject =
      { primitive with
          Context = ShadedObjectContext.UpdateVertices vertices primitive.Context }

    static member Update (vertices) (indices) (primitive: ShadedObject) : ShadedObject =
      { primitive with
          Context = ShadedObjectContext.Update vertices indices primitive.Context }

type UniformData =
  | Matrix4x4Uniform of Name:string * Data:Matrix4x4
  | Vector2Uniform of Name:string * Data:Vector2

let private _useMvpShader<'T> (config:Config<'T>) shader (viewMatrix:Matrix4x4) (projectionMatrix:Matrix4x4) (scale:Vector3) (rotation:Vector3) (translation:Vector3) uniforms =
  glUseProgram shader
  let mvpUniform = glGetUniformLocationEasy shader "mvp"

  let scaleMatrix = Matrix4x4.CreateScale(scale)
  let rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
  let translationMatrix = Matrix4x4.CreateTranslation(translation)
  let modelMatrix = scaleMatrix * rotationMatrix * translationMatrix

  let mvp = modelMatrix * viewMatrix * projectionMatrix
  glUniformMatrix4fvEasy mvpUniform 1 mvp
  
  List.map
    (
      fun uniformData ->
        match uniformData with
        | Matrix4x4Uniform(name, data) -> 
            let location = glGetUniformLocationEasy shader name
            glUniformMatrix4fvEasy location 1 data
        | Vector2Uniform(name, data) -> 
            let location = glGetUniformLocationEasy shader name
            glUniform2f location data.X data.Y)
    uniforms |> ignore

let drawShadedLine<'T> (config:Config<'T>) (primitive:ShadedObject) =
  glUseProgram primitive.Shader
  glBindVertexArray primitive.Context.VAO
  glBindBuffer
    GL_ARRAY_BUFFER
    primitive.Context.VBO
  glBindBuffer
    GL_ELEMENT_ARRAY_BUFFER
    primitive.Context.EBO
  glDrawArrays
    GL_LINES
    0
    primitive.Context.Indices.Length

let drawShadedLineWithMvp<'T> (config:Config<'T>) (viewMatrix:Matrix4x4) (projectionMatrix:Matrix4x4) (primitive:ShadedObject) (scale:Vector3) (rotation:Vector3) (translation:Vector3) =
  let shader = primitive.Shader
  _useMvpShader config shader viewMatrix projectionMatrix scale rotation translation []

  glBindVertexArray primitive.Context.VAO
  glBindBuffer
    GL_ELEMENT_ARRAY_BUFFER
    primitive.Context.EBO
  glDrawArrays
    GL_LINES
    0
    primitive.Context.Indices.Length

let drawShadedObject<'T> (config:Config<'T>) (primitive:ShadedObject) =
  glUseProgram primitive.Shader
  glBindVertexArray primitive.Context.VAO
  glBindBuffer
    GL_ELEMENT_ARRAY_BUFFER
    primitive.Context.EBO
  glDrawElements
    GL_TRIANGLES
    primitive.Context.Indices.Length
    GL_UNSIGNED_INT
    GL_NULL

let drawShadedObjectWithMvp<'T> (config:Config<'T>) (viewMatrix:Matrix4x4) (projectionMatrix:Matrix4x4) (primitive:ShadedObject) (scale:Vector3) (rotation:Vector3) (translation:Vector3) (uniforms) =
  let shader = primitive.Shader
  _useMvpShader config shader viewMatrix projectionMatrix scale rotation translation (
    Vector2Uniform("mouse", config.Mouse.Position)::uniforms)
  
  glBindVertexArray primitive.Context.VAO
  glBindBuffer
    GL_ELEMENT_ARRAY_BUFFER
    primitive.Context.EBO
  glDrawElements
    GL_TRIANGLES
    primitive.Context.Indices.Length
    GL_UNSIGNED_INT
    GL_NULL
