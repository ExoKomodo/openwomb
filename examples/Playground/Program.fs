open Argu
open Womb
open Womb.Graphics
open System

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

let private initHandler configState =
  let (config, state) = configState
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
      (
        config,
        { GameState.Default with
            Triangles = { triangles with
              Shader = shader
              VertexData = Primitives.VertexObjectData.From triangles.Vertices triangles.Indices } }
      )
  | None ->
      Logging.fail "Failed to compile shader"
      configState

let private drawHandler configState =
  let (config, state) = configState
  let config = Display.clear config
  Primitives.drawShadedObject state.Triangles
  (Display.swap config, state)

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

  let (config, _) = (
    Game.play
      "Womb Playground"
      width
      height
      GameState.Default
      (Some initHandler)
      (Some drawHandler) )
  config.ExitCode
