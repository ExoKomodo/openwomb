module Womb.Engine.Internals

open SDL2Bindings
open Womb.Graphics
open Womb.Graphics.Types
open Womb.Input.Types
open Womb.Logging
open Womb.Types
open Womb.Backends.OpenGL.Api

let private handleQuit (config:Config<'T>) (event:SDL.SDL_Event) =
  config.StopHandler config

let private handleWindowResize (config:Config<'T>) (event:SDL.SDL_WindowEvent) =
  let width = event.data1
  let height = event.data2
  
  debug $"Resize to (width: {width}, height: {height})"

  glViewport
    0
    0
    (int width)
    (int height)
  
  { config with
      DisplayConfig =
        {config.DisplayConfig with
            Width = uint width
            Height = uint height } }

let private handleWindowEvent (config:Config<'T>) (event:SDL.SDL_WindowEvent) =
  match event.windowEvent with
  | SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED -> handleWindowResize config event
  | _ -> config

let private handleEvent (config:Config<'T>) (event:SDL.SDL_Event) =
  match event.typeFSharp with
  | SDL.SDL_EventType.SDL_WINDOWEVENT -> handleWindowEvent config event.window
  | SDL.SDL_EventType.SDL_QUIT -> handleQuit config event
  | SDL.SDL_EventType.SDL_KEYUP
    | SDL.SDL_EventType.SDL_KEYDOWN
    | SDL.SDL_EventType.SDL_MOUSEBUTTONUP
    | SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN -> config.EventHandler config event
  | _ -> config

////////////
// Module //
////////////

let private pollMouseState<'T> (config:Config<'T>) : MouseState =
  let (_, x, y) = SDL.SDL_GetMouseState()
  let (adjustedX, adjustedY) = (
    x,
    int(config.DisplayConfig.Height) - y
  )

  { MouseState.Default with
      Position = (adjustedX |> single, adjustedY |> single) }

let private pollInputs (config:Config<'T>) : Config<'T> =
  { config with
      Mouse = pollMouseState config }

let rec private eventLoop (config:Config<'T>) =
  let mutable event = SDL.SDL_Event()
  if SDL.SDL_PollEvent(&event) = 0 then
    config
  else
    handleEvent config event |> eventLoop

let internal shutdown (config: Config<'T>) : Config<'T> =
  warn "Shutting down internals"
  SDL.SDL_Quit()
  config.StopHandler config

let clock config =
  config.FrameTimer.Stop()
  config.OverallTimer.Stop()
  let (frame, overall) = (config.FrameTimer.Elapsed, config.OverallTimer.Elapsed)
  config.FrameTimer.Reset()
  config.FrameTimer.Start()
  config.OverallTimer.Start()
  { config with
      FrameDelta = frame
      OverallDelta = overall }

let rec internal updateLoop<'T> (config:Config<'T>) : Config<'T> =
  if not config.IsRunning then
    config.FrameTimer.Stop()
    config.OverallTimer.Stop()
    config
  else
    pollInputs config
      |> clock
      |> eventLoop
      |> config.LoopHandler
      |> updateLoop

let drawBegin (config:DisplayConfig) =
  Display.clear config

let drawEnd (config:DisplayConfig) =
  Display.swap config
