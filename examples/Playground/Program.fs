open Argu
open System
open System.Numerics
open Womb
open Womb.Graphics
open Womb.Types

let DEFAULT_WIDTH = 800u
let DEFAULT_HEIGHT = 600u

type CliArguments =
  | [<Unique>][<EqualsAssignmentOrSpaced>] Width of width:uint
  | [<Unique>][<EqualsAssignmentOrSpaced>] Height of height:uint

  interface IArgParserTemplate with
    member s.Usage =
      match s with
      | Width _ -> $"set the initial display width (default: %d{DEFAULT_WIDTH})"
      | Height _ -> $"set the initial display height (default: %d{DEFAULT_HEIGHT})"

type GameState =
  { Triangles: Primitives.ShadedObject; }
  
    static member Default = {
      Triangles = Primitives.ShadedObject.Default }

let private initHandler (config:Config<GameState>) =
  let state = config.State
  let triangles =
    Primitives.ShadedObject.From
      { state.Triangles with
          FragmentShaderPaths = ["Resources/Shaders/fragment.glsl"]
          VertexShaderPaths = ["Resources/Shaders/vertex.glsl"]
      }
      [|
        0.0f; -0.5f; 0.0f;  // shared vertex
        // first triangle
        -0.9f; -0.5f; 0.0f; // left vertex
        -0.45f; 0.5f; 0.0f; // top vertex
        // second triangle
        0.9f; -0.5f; 0.0f;  // right vertex
        0.45f; 0.5f; 0.0f;  // top vertex
      |]
      [|
        0u; 1u; 2u; // first triangle vertex order as array indices
        0u; 3u; 4u; // second triangle vertex order as array indices
      |]
  match Display.compileShader triangles.VertexShaderPaths triangles.FragmentShaderPaths with
  | Some(shader) -> 
      { config with
          State =
            { GameState.Default with
                Triangles =
                  { triangles with
                      Shader = shader
                      Context = Primitives.ShadedObjectContext.From triangles.Context.Vertices triangles.Context.Indices } } }
  | None ->
      Logging.fail "Failed to compile shader"
      config

let private calculateMatrices cameraPosition cameraTarget =
  let viewMatrix = Matrix4x4.CreateLookAt(
    cameraPosition,
    cameraTarget,
    Vector3.UnitY
  )
  let projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(-1f, 1f, -1f, 1f, 0f, 1f)
  (viewMatrix, projectionMatrix)

let private drawHandler (config:Config<GameState>) =
  let cameraPosition = new Vector3(0f, 0f, 1f)
  let cameraTarget = new Vector3(0f, 0f, 0f)
  let (viewMatrix, projectionMatrix) = calculateMatrices cameraPosition cameraTarget

  let state = config.State
  let scale = Vector3.One * 1.0f
  let rotation = Vector3.UnitZ * 0.0f
  let displayConfig = Display.clear config.DisplayConfig
  Primitives.drawShadedObject
    config
    viewMatrix
    projectionMatrix
    state.Triangles
    scale
    rotation
    Vector3.Zero
    []
  { config with
      DisplayConfig = Display.swap displayConfig }

[<EntryPoint>]
let main argv =
  let errorHandler =
    ProcessExiter(
      colorizer=function
        | ErrorCode.HelpText -> None
        | _ -> Some ConsoleColor.Red )
  let parsedArgs =
    ArgumentParser.Create<CliArguments>(programName="Playground", errorHandler=errorHandler).Parse argv

  let width = parsedArgs.GetResult(Width, DEFAULT_WIDTH)
  let height = parsedArgs.GetResult(Height, DEFAULT_HEIGHT)

  ( Game.play
      "Womb Playground"
      width
      height
      GameState.Default
      (Some initHandler)
      None
      (Some drawHandler) ).ExitCode
