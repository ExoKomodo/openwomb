module Womb.Graphics.Display

open Womb.Backends.OpenGL.Api
open Womb.Backends.OpenGL.Api.Constants
open Womb.Logging

///////////
// Types //
///////////

open SDL2Bindings

type Config =
  { Width: uint;
    Height: uint;
    Title: string;
    IsFullscreen: bool;
    WindowFlags: SDL.SDL_WindowFlags;
    Window: nativeint;
    Context: nativeint; }

    static member Default =
      { Width = 800u
        Height = 600u
        Title = "Womb"
        IsFullscreen = false
        WindowFlags = SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN
        Window = 0n
        Context = 0n }

////////////C
// Module //
////////////

open System.IO

let private shaderTypeToString shaderType =
  match shaderType with
  | GL_VERTEX_SHADER -> "vertex"
  | GL_FRAGMENT_SHADER -> "fragment"
  | _ -> "unknown"

let private buildShader
  shaderType
  shaderSource =
    let shaderTypeString = shaderTypeToString shaderType
    let shader = glCreateShader shaderType
    glShaderSource
      shader
      shaderSource
    glCompileShader shader
    let success =
      glGetShaderiv
        shader
        GL_COMPILE_STATUS
    if success = 0 then
      fail $"Failed to compile %s{shaderTypeString} shader with error:"
      fail (glGetShaderInfoLog shader)
      None
    else
      debug $"Successfully compiled %s{shaderTypeString} shader"
      Some(shader)

let private linkShaderProgram vertexShader fragmentShader =
  let shaderProgram = glCreateProgram()
  glAttachShader
    shaderProgram
    vertexShader
  glAttachShader
    shaderProgram
    fragmentShader
  glLinkProgram shaderProgram
  glDeleteShader vertexShader
  glDeleteShader fragmentShader

  debug
    $"Successfully linked shader program with vertex shader (ID:%d{vertexShader}) fragment shader (ID:%d{fragmentShader})"
  
  let success =
    glGetProgramiv
      shaderProgram
      GL_LINK_STATUS
  if success = 0 then
    fail "Failed to link shader program with error:"
    fail (glGetProgramInfoLog shaderProgram)
    None
  else
    Some(shaderProgram)

let compileShader vertexShaderPath fragmentShaderPath =
  let vertexShaderSource = File.ReadAllText(vertexShaderPath)
  let fragmentShaderSource = File.ReadAllText(fragmentShaderPath)
  let vertexShaderOpt =
    buildShader GL_VERTEX_SHADER vertexShaderSource
  let fragmentShaderOpt =
    buildShader GL_FRAGMENT_SHADER fragmentShaderSource

  match (vertexShaderOpt, fragmentShaderOpt) with
  | Some(vertexShader), Some(fragmentShader) ->
    match linkShaderProgram vertexShader fragmentShader with
    | Some(shaderProgram) ->
        Some(shaderProgram)
    | None -> None 
  | _ -> None

let private initializeGraphicsContext config =
  debug "BEGIN graphics context initialization"
  let context = SDL.SDL_GL_CreateContext(config.Window)
  SDL.SDL_GL_MakeCurrent(config.Window, context)
    |> ignore
  Womb.Backends.OpenGL.Module.setup SDL.SDL_GL_GetProcAddress
  debug "END graphics context initialization"
  
  let vendor =
    match glGetString GL_VENDOR with
    | Some(vendor) -> vendor
    | None ->
        fail "Something Examples/Playground/went wrong getting vendor string"
        "<invalid:vendor>"
  let renderer =
    match glGetString GL_RENDERER with
    | Some(renderer) -> renderer
    | None ->
        fail "Something went wrong getting renderer string"
        "<invalid:renderer>"
  let version =
    match glGetString GL_VERSION with
    | Some(version) -> version
    | None ->
        fail "Something went wrong getting version string"
        "<invalid:version>"
  info
    $"Found %s{renderer} from %s{vendor} with the following OpenGL driver: %s{version}"
  info
    $"Using %s{renderer} from %s{vendor} with the following OpenGL driver: %s{version}"

  glViewport
    0
    0
    (int config.Width)
    (int config.Height)

  { config with Context = context }

let private setGLAttributes config =
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION,
    3 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION,
    3 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK,
    SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_CONTEXT_FLAGS,
    (int)SDL.SDL_GLcontext.SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_RED_SIZE,
    8 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_GREEN_SIZE,
    8 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_BLUE_SIZE,
    8 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_ALPHA_SIZE,
    8 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_BUFFER_SIZE,
    0 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_DOUBLEBUFFER,
    1 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE,
    16 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_STENCIL_SIZE,
    0 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_ACCUM_RED_SIZE,
    0 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_ACCUM_GREEN_SIZE,
    0 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_ACCUM_BLUE_SIZE,
    0 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_ACCUM_ALPHA_SIZE,
    0 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_STEREO,
    0 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_MULTISAMPLEBUFFERS,
    0 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_MULTISAMPLESAMPLES,
    0 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_ACCELERATED_VISUAL,
    1 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_SHARE_WITH_CURRENT_CONTEXT,
    0 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_FRAMEBUFFER_SRGB_CAPABLE,
    0 ) |> ignore
  SDL.SDL_GL_SetAttribute(
    SDL.SDL_GLattr.SDL_GL_CONTEXT_RELEASE_BEHAVIOR,
    1 ) |> ignore

let private determineWindowFlags config =
  SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL
  ||| SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE
  ||| SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN

let private initializeWindow config =
  debug "Initializing window"
  setGLAttributes config
  let windowFlags = determineWindowFlags config
  { config with
      WindowFlags = windowFlags
      Window =
        SDL.SDL_CreateWindow(
          config.Title,
          SDL.SDL_WINDOWPOS_CENTERED,
          SDL.SDL_WINDOWPOS_CENTERED,
          (int config.Width),
          (int config.Height),
          windowFlags ) }

let clear config =
  glClearColor
    1f
    0f
    1f
    1f
  glClear
    GL_COLOR_BUFFER_BIT
  config

let initialize config =
  debug
    $"BEGIN graphics initialization for %s{config.Title} with %d{config.Width} by %d{config.Height}"

  if SDL.SDL_Init(SDL.SDL_INIT_VIDEO) <> 0 then 
    debug "FAIL graphics initialization"
    None
  else
    debug "END graphics initialization"
    Some(
      initializeWindow config 
      |> initializeGraphicsContext )

let shutdown config =
  config

let swap config =
  SDL.SDL_GL_SwapWindow(config.Window)
  config

let toggleFullscreen config =
  { config with
      IsFullscreen = not config.IsFullscreen }
