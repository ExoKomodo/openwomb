module Womb.Graphics.Primitives

#nowarn "9" // Unverifiable IL due to fixed expression and NativePtr library usage

open Womb.Backends.OpenGL.Api
open Womb.Backends.OpenGL.Api.Constants
open Womb.Graphics.Types
open Womb.Lib.Types
open Womb.Logging
open Womb.Types

type ShadedObjectContext =
  { VAO: uint;
    VBO: uint;
    EBO: uint;
    Vertices: array<single>;
    Indices: array<uint>; }

    static member Default () =
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
  | Quad of Context:ShadedObjectContext * Shader:ShaderProgram * Transform:Womb.Lib.Types.Transform * Width:single * Height:single

  static member DefaultQuad =
    Quad(ShadedObjectContext.Default(), ShaderProgram.Default(), Transform.Default(), 0f, 0f)

  static member Default = ShadedObject.DefaultQuad

  static member Contains primitive point =
    match primitive with
    | Quad(context, shader, transform, width, height) ->
        let (x, y, _) = transform.Translation
        let rect = new System.Drawing.RectangleF(x - (width / 2f), y - (height / 2f), width, height)
        let (x, y) = point
        if rect.Contains(x, y) then
          Some point
        else
          None

  static member CreateQuad vertexPaths fragmentPaths transform width height =
    match (
      Display.compileShader
        vertexPaths
        fragmentPaths
    ) with
    | Some shader ->
        let vertices = [|
          // bottom left
          -width / 2.0f; -height / 2.0f; 0.0f;
          // shared top left
          -width / 2.0f; height / 2.0f; 0.0f;
          // shared bottom right
          width / 2.0f; -height / 2.0f; 0.0f;
          // top right
          width / 2.0f; height / 2.0f; 0.0f;
        |]
        let indices = [|
          0u; 1u; 2u; // first triangle vertex order as array indices
          1u; 2u; 3u; // second triangle vertex order as array indices
        |]
        Quad(
          ShadedObjectContext.From vertices indices,
          shader,
          transform,
          width,
          height
        ) |> Some
    | None ->
        fail $"Failed to compile quad shaders:\n{vertexPaths}\n{fragmentPaths}"
        None

  static member UpdateIndices (indices) (primitive: ShadedObject) : ShadedObject =
    match primitive with
    | Quad(context, shader, transform, width, height) -> Quad(
        ShadedObjectContext.UpdateIndices indices context,
        shader,
        transform,
        width,
        height
      )

  static member UpdateVertices (vertices) (primitive: ShadedObject) : ShadedObject =
    match primitive with
    | Quad(context, shader, transform, width, height) -> Quad(
        ShadedObjectContext.UpdateVertices vertices context,
        shader,
        transform,
        width,
        height
      )

  static member Update (vertices) (indices) (primitive: ShadedObject) : ShadedObject =
    match primitive with
    | Quad(context, shader, transform, width, height) -> Quad(
        ShadedObjectContext.Update vertices indices context,
        shader,
        transform,
        width,
        height
      )
  
  static member private UseMvpShader<'T> (config:Config<'T>) (shader:ShaderProgram) (viewMatrix:System.Numerics.Matrix4x4) (projectionMatrix:System.Numerics.Matrix4x4) (transform:Transform) uniforms =
    glUseProgram shader.Id
    let mvpUniform = glGetUniformLocation shader.Id "in_mvp"

    let (x, y, z) = transform.Scale
    let scaleMatrix = System.Numerics.Matrix4x4.CreateScale(x, y, z)
    let (x, y, z) = transform.Rotation
    let rotationMatrix = System.Numerics.Matrix4x4.CreateFromYawPitchRoll(y, x, z)
    let (x, y, z) = transform.Translation
    let translationMatrix = System.Numerics.Matrix4x4.CreateTranslation(x, y, z)
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
          | Vector1Uniform(name, data) ->
              let location = glGetUniformLocation shader.Id name
              glUniform1f location data
          | Vector2Uniform(name, data) ->
              let location = glGetUniformLocation shader.Id name
              let (x, y) = data
              glUniform2f location x y
          | Vector3Uniform(name, data) ->
              let location = glGetUniformLocation shader.Id name
              let (x, y, z) = data
              glUniform3f location x y z
          | Vector4Uniform(name, data) ->
              let location = glGetUniformLocation shader.Id name
              let (x, y, z, w) = data
              glUniform4f location x y z w
          | VectorUniform(name, data) ->
              let location = glGetUniformLocation shader.Id name
              match data.Length with
              | 1 -> glUniform1f location data[0]
              | 2 -> glUniform2f location data[0] data[1]
              | 3 -> glUniform3f location data[0] data[1] data[2]
              | 4 -> glUniform4f location data[0] data[1] data[2] data[3]
              | len -> fail $"Unsupported IVectorUniform length {len} when trying to use shader"
          | IVector1Uniform(name, x) ->
              let location = glGetUniformLocation shader.Id name
              glUniform1i location x
          | IVector2Uniform(name, data) ->
              let location = glGetUniformLocation shader.Id name
              let (x, y) = data
              glUniform2i location x y
          | IVector3Uniform(name, data) ->
              let location = glGetUniformLocation shader.Id name
              let (x, y, z) = data
              glUniform3i location x y z
          | IVector4Uniform(name, data) ->
              let location = glGetUniformLocation shader.Id name
              let (x, y, z, w) = data
              glUniform4i location x y z w
          | IVectorUniform(name, data) ->
              let location = glGetUniformLocation shader.Id name
              match data.Length with
              | 1 -> glUniform1i location data[0]
              | 2 -> glUniform2i location data[0] data[1]
              | 3 -> glUniform3i location data[0] data[1] data[2]
              | 4 -> glUniform4i location data[0] data[1] data[2] data[3]
              | len -> fail $"Unsupported IVectorUniform length {len} when trying to use shader"
          | UVector1Uniform(name, x) ->
              let location = glGetUniformLocation shader.Id name
              glUniform1ui location x
          | UVector2Uniform(name, data) ->
              let location = glGetUniformLocation shader.Id name
              let (x, y) = data
              glUniform2ui location x y
          | UVector3Uniform(name, data) ->
              let location = glGetUniformLocation shader.Id name
              let (x, y, z) = data
              glUniform3ui location x y z
          | UVector4Uniform(name, data) ->
              let location = glGetUniformLocation shader.Id name
              let (x, y, z, w) = data
              glUniform4ui location x y z w
          | UVectorUniform(name, data) ->
              let location = glGetUniformLocation shader.Id name
              match data.Length with
              | 1 -> glUniform1ui location data[0]
              | 2 -> glUniform2ui location data[0] data[1]
              | 3 -> glUniform3ui location data[0] data[1] data[2]
              | 4 -> glUniform4ui location data[0] data[1] data[2] data[3]
              | len -> fail $"Unsupported UVectorUniform length {len} when trying to use shader"
      )
      uniforms |> ignore
  
  static member Draw<'T> (config:Config<'T>) (viewMatrix:System.Numerics.Matrix4x4) (projectionMatrix:System.Numerics.Matrix4x4) (primitive:ShadedObject) (uniforms) =
    match primitive with
    | Quad(context, shader, transform, width, height) ->
      ShadedObject.UseMvpShader
        config
        shader
        viewMatrix
        projectionMatrix
        transform
        (
          [
            Vector2Uniform(
              "in_mouse",
              config.Mouse.Position
            );
            Vector2Uniform(
              "in_viewport",
              (
                config.DisplayConfig.Width |> single,
                config.DisplayConfig.Height |> single
              )
            );
          ] @ uniforms )
      
      glBindVertexArray context.VAO
      glBindBuffer
        GL_ELEMENT_ARRAY_BUFFER
        context.EBO
      glDrawElements
        GL_TRIANGLES
        context.Indices.Length
        GL_UNSIGNED_INT
        GL_NULL
