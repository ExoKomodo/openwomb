open Argu
open System
open System.Numerics
open Womb
open Womb.Graphics
open Womb.Lib.Types
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
  let fragmentPaths = ["Resources/Shaders/fragment.glsl"]
  let vertexPaths = ["Resources/Shaders/vertex.glsl"]
  let vertices = [|
    0.0f; -0.5f; 0.0f;  // shared vertex
    // first triangle
    -0.9f; -0.5f; 0.0f; // left vertex
    -0.45f; 0.5f; 0.0f; // top vertex
    // second triangle
    0.9f; -0.5f; 0.0f;  // right vertex
    0.45f; 0.5f; 0.0f;  // top vertex
  |]
  let indices = [|
    0u; 1u; 2u; // first triangle vertex order as array indices
    0u; 3u; 4u; // second triangle vertex order as array indices
  |]
  let transform =
    { Transform.Default() with
        Scale = (1.0f, 1.0f, 1.0f)
        Rotation = (0.0f, 0.0f, 0.0f) }
  match Primitives.ShadedObject.CreateQuad vertexPaths fragmentPaths transform 1.8f 1.0f with
  | Some primitive ->
    { config with
        State =
          { GameState.Default with
              Triangles = primitive }}
  | None ->
    Logging.fail "Failed to create initial game state"
    config

let private calculateMatrices (cameraPosition:Numerics.Vector3) (cameraTarget:Numerics.Vector3) =
  let viewMatrix = Matrix4x4.CreateLookAt(
    cameraPosition,
    cameraTarget,
    Vector3.UnitY
  )
  let projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(-1f, 1f, -1f, 1f, 0f, 1f)
  (viewMatrix, projectionMatrix)

let private loopHandler (config:Config<GameState>) =
  let cameraPosition = new Numerics.Vector3(0f, 0f, 1f)
  let cameraTarget = new Numerics.Vector3(0f, 0f, 0f)
  let (viewMatrix, projectionMatrix) = calculateMatrices cameraPosition cameraTarget

  let state = config.State
  let displayConfig = Display.clear config.DisplayConfig
  Primitives.ShadedObject.Draw
    config
    viewMatrix
    projectionMatrix
    state.Triangles
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
      (Some loopHandler) ).ExitCode
