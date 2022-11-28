module Womb.Graphics.Display

open SDL2Bindings
open System.IO
open Womb.Backends.OpenGL.Api
open Womb.Backends.OpenGL.Api.Constants
open Womb.Graphics.Types
open Womb.Logging

////////////C
// Module //
////////////

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

let private linkShaderPrograms shaders =
  let shaderProgram = glCreateProgram()
  List.map
    (glAttachShader shaderProgram)
    shaders |> ignore

  glLinkProgram shaderProgram
  List.map
    glDeleteShader
    shaders |> ignore

  List.map
    (
      fun shaderId -> (debug
        $"Successfully linked shader program with shader (ID:%d{shaderId})"
      )
    )
    shaders |> ignore
  
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

let private _compileShader shaderPath shaderType =
  let shaderSource = File.ReadAllText(shaderPath)
  buildShader shaderType shaderSource

let compileShader vertexShaderPaths fragmentShaderPaths =
  let shaderInfo = (
    List.append
      (List.map
        (fun shaderPath -> shaderPath, GL_VERTEX_SHADER)
        vertexShaderPaths)
      (List.map
        (fun shaderPath -> shaderPath, GL_FRAGMENT_SHADER)
        fragmentShaderPaths)
  )
  let shaders: list<uint> = (List.choose  id
    (
      List.map
        (fun (shaderPath, shaderType) ->
          _compileShader shaderPath shaderType
        )
        shaderInfo
    )
  )

  match linkShaderPrograms shaders with
  | Some(programId) ->
      Some(
        { Id = programId
          FragmentPaths = fragmentShaderPaths
          VertexPaths = vertexShaderPaths } )
  | None -> None

let private initializeGraphicsContext (config:DisplayConfig) =
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
        fail "Something went wrong getting vendor string"
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

let private setGLAttributes (config:DisplayConfig) =
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

let private determineWindowFlags (config:DisplayConfig) =
  SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL
  ||| SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE
  ||| SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN

let private initializeWindow (config:DisplayConfig) =
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

let clear (config:DisplayConfig) =
  glClearColor
    1f
    0f
    1f
    1f
  glClear
    GL_COLOR_BUFFER_BIT
  config

let initialize (config:DisplayConfig) =
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

let shutdown (config:DisplayConfig) =
  config

let swap (config:DisplayConfig) =
  SDL.SDL_GL_SwapWindow(config.Window)
  config

let toggleFullscreen (config:DisplayConfig) =
  { config with
      IsFullscreen = not config.IsFullscreen }
