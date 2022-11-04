module Womb.Graphics.Primitives

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
    FragmentShaderPath: string;
    VertexShaderPath: string; }

    static member Default =
      { VertexData = VertexObjectData.Default
        Shader = 0u
        Vertices = Array.empty
        Indices = Array.empty
        FragmentShaderPath = ""
        VertexShaderPath = "" }

    static member From (primitive: ShadedObject) (vertices) (indices) : ShadedObject =
      { primitive with
          Vertices = vertices
          Indices = indices
          VertexData = VertexObjectData.From vertices indices }

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
