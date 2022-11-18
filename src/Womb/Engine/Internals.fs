module Womb.Engine.Internals

open SDL2Bindings
open System.Numerics
open Womb.Graphics
open Womb.Input
open Womb.Logging
open Womb.Types

let internal handleQuit (config:Config<'T>) (event:SDL.SDL_Event) =
  config.StopHandler config

let internal handleEvent (config:Config<'T>) (event:SDL.SDL_Event) =
  match event.typeFSharp with
  | SDL.SDL_EventType.SDL_KEYUP -> config.KeyUpHandler config event
  | SDL.SDL_EventType.SDL_QUIT -> handleQuit config event
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
      Position = new Vector2(single(adjustedX), single(adjustedY)) }

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

let drawBegin (config:DisplayConfig) =
  Display.clear config

let drawEnd (config:DisplayConfig) =
  Display.swap config

let rec internal updateLoop<'T> (config:Config<'T>) : Config<'T> =
  if not config.IsRunning then
    config
  else
    pollInputs config
      |> eventLoop
      |> config.DrawHandler
      |> updateLoop
