module Womb.Graphics.Primitives

#nowarn "9" // Unverifiable IL due to fixed expression and NativePtr library usage

open System.Numerics
open Womb.Logging
open Womb.Backends.OpenGL.Api
open Womb.Backends.OpenGL.Api.Constants
open Womb.Graphics.Types
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
  | Quad of Context:ShadedObjectContext * Shader:ShaderProgram

  static member Default =
    Quad(ShadedObjectContext.Default, ShaderProgram.Default)

  static member CreateQuad vertexPaths fragmentPaths vertices indices =
    match (
      Display.compileShader
        vertexPaths
        fragmentPaths
    ) with
    | Some(shader) -> 
        Some(
          Quad(
            ShadedObjectContext.From vertices indices,
            shader
          )
        )
    | None ->
        fail $"Failed to compile quad shaders:\n{vertexPaths}\n{fragmentPaths}"
        None

  static member UpdateIndices (indices) (primitive: ShadedObject) : ShadedObject =
    match primitive with
    | Quad(context, shader) -> Quad(
        ShadedObjectContext.UpdateIndices indices context,
        shader
      )

  static member UpdateVertices (vertices) (primitive: ShadedObject) : ShadedObject =
    match primitive with
    | Quad(context, shader) -> Quad(
        ShadedObjectContext.UpdateVertices vertices context,
        shader
      )

  static member Update (vertices) (indices) (primitive: ShadedObject) : ShadedObject =
    match primitive with
    | Quad(context, shader) -> Quad(
        ShadedObjectContext.Update vertices indices context,
        shader
      )
  
  static member private UseMvpShader<'T> (config:Config<'T>) (shader:ShaderProgram) (viewMatrix:Matrix4x4) (projectionMatrix:Matrix4x4) (scale:Vector3) (rotation:Vector3) (translation:Vector3) uniforms =
    glUseProgram shader.Id
    let mvpUniform = glGetUniformLocation shader.Id "mvp"

    let scaleMatrix = Matrix4x4.CreateScale(scale)
    let rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
    let translationMatrix = Matrix4x4.CreateTranslation(translation)
    let modelMatrix = scaleMatrix * rotationMatrix * translationMatrix

    let mvp = modelMatrix * viewMatrix * projectionMatrix
    glUniformMatrix4fv mvpUniform 1 mvp
    
    List.map
      (
        fun uniformData ->
          match uniformData with
          | Matrix4x4Uniform(name, data) -> 
              let location = glGetUniformLocation shader.Id name
              glUniformMatrix4fv location 1 data
          | Vector2Uniform(name, data) -> 
              let location = glGetUniformLocation shader.Id name
              glUniform2f location data.X data.Y)
      uniforms |> ignore
  
  static member Draw<'T> (config:Config<'T>) (viewMatrix:Matrix4x4) (projectionMatrix:Matrix4x4) (primitive:ShadedObject) (scale:Vector3) (rotation:Vector3) (translation:Vector3) (uniforms) =
    match primitive with
    | Quad(context, shader) ->
      ShadedObject.UseMvpShader config shader viewMatrix projectionMatrix scale rotation translation (
        Vector2Uniform("mouse", config.Mouse.Position)::uniforms)
      
      glBindVertexArray context.VAO
      glBindBuffer
        GL_ELEMENT_ARRAY_BUFFER
        context.EBO
      glDrawElements
        GL_TRIANGLES
        context.Indices.Length
        GL_UNSIGNED_INT
        GL_NULL

