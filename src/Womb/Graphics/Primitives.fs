module Womb.Graphics.Primitives

#nowarn "9" // Unverifiable IL due to fixed expression and NativePtr library usage

open System.Numerics
open System.Text
open Womb.Backends.OpenGL.Api
open Womb.Backends.OpenGL.Api.Constants

type VertexObjectData =
  { VAO: uint;
    VBO: uint;
    EBO: uint; }

    static member Default =
      { VAO = 0u
        VBO = 0u
        EBO = 0u }

    static member From (vertices) (indices) : VertexObjectData =
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
        GL_STATIC_DRAW
      
      glBindBuffer
        GL_ELEMENT_ARRAY_BUFFER
        ebo
      glBufferData
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
        EBO = ebo }

type ShadedObject =
  { VertexData: VertexObjectData;
    Vertices: array<single>;
    Indices: array<uint>;
    Shader: uint;
    FragmentShaderPaths: list<string>;
    VertexShaderPaths: list<string>; }

    static member Default =
      { VertexData = VertexObjectData.Default
        Shader = 0u
        Vertices = Array.empty
        Indices = Array.empty
        FragmentShaderPaths = List.Empty
        VertexShaderPaths = List.Empty }

    static member From (primitive: ShadedObject) (vertices) (indices) : ShadedObject =
      { primitive with
          Vertices = vertices
          Indices = indices
          VertexData = VertexObjectData.From vertices indices }

let private _useMvpShader shader (viewMatrix:Matrix4x4) (projectionMatrix:Matrix4x4) (scale:Vector3) (rotation:Vector3) (translation:Vector3) =
  glUseProgram shader
  let mvpUniform = glGetUniformLocationEasy shader "mvp"

  let scaleMatrix = Matrix4x4.CreateScale(scale)
  let rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
  let translationMatrix = Matrix4x4.CreateTranslation(translation)
  let modelMatrix = scaleMatrix * rotationMatrix * translationMatrix

  let mvp = modelMatrix * viewMatrix * projectionMatrix
  glUniformMatrix4fvEasy mvpUniform 1 mvp

let drawShadedLine (primitive:ShadedObject) =
  glUseProgram primitive.Shader
  glBindVertexArray primitive.VertexData.VAO
  glBindBuffer
    GL_ELEMENT_ARRAY_BUFFER
    primitive.VertexData.EBO
  glDrawArrays
    GL_LINES
    0
    primitive.Indices.Length

let drawShadedLineWithMvp (viewMatrix:Matrix4x4) (projectionMatrix:Matrix4x4) (primitive:ShadedObject) (scale:Vector3) (rotation:Vector3) (translation:Vector3) =
  let shader = primitive.Shader
  _useMvpShader shader viewMatrix projectionMatrix scale rotation translation

  glBindVertexArray primitive.VertexData.VAO
  glBindBuffer
    GL_ELEMENT_ARRAY_BUFFER
    primitive.VertexData.EBO
  glDrawArrays
    GL_LINES
    0
    primitive.Indices.Length

let drawShadedObject (primitive:ShadedObject) =
  glUseProgram primitive.Shader
  glBindVertexArray primitive.VertexData.VAO
  glBindBuffer
    GL_ELEMENT_ARRAY_BUFFER
    primitive.VertexData.EBO
  glDrawElements
    GL_TRIANGLES
    primitive.Indices.Length
    GL_UNSIGNED_INT
    GL_NULL

let drawShadedObjectWithMvp (viewMatrix:Matrix4x4) (projectionMatrix:Matrix4x4) (primitive:ShadedObject) (scale:Vector3) (rotation:Vector3) (translation:Vector3) =
  let shader = primitive.Shader
  _useMvpShader shader viewMatrix projectionMatrix scale rotation translation
  
  glBindVertexArray primitive.VertexData.VAO
  glBindBuffer
    GL_ELEMENT_ARRAY_BUFFER
    primitive.VertexData.EBO
  glDrawElements
    GL_TRIANGLES
    primitive.Indices.Length
    GL_UNSIGNED_INT
    GL_NULL
