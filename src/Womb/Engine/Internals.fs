module Womb.Engine.Internals

open Womb.Graphics
open Womb.Logging

///////////
// Types //
///////////

type Config =
  { DisplayConfig: Display.Config;
    IsRunning: bool;
    IsShutdown: bool;
    ExitCode: int;
    ResourcePathRoot: string;
    StopHandler: Config -> Config }

    static member Default =
      { DisplayConfig = Display.Config.Default
        IsRunning = false
        IsShutdown = false
        ExitCode = 0
        ResourcePathRoot = "Resources"
        StopHandler = fun config ->
          debug "Handling quit event and may call twice"
          if config.IsRunning then
            { config with
                DisplayConfig = Display.shutdown config.DisplayConfig
                IsRunning = false }
          else
            config }

/////////////
// Helpers //
/////////////

let private stop = Config.Default.StopHandler

//////////////
// Handlers //
//////////////

open SDL2Bindings

let internal handleQuit (configState:Config * 'T) (event:SDL.SDL_Event) =
  let (config, state) = configState
  (config.StopHandler config, state)

let internal handleKeyUp (configState:Config * 'T) (event:SDL.SDL_Event) =
  let (config, state) = configState
  match event.key.keysym.sym with
  | SDL.SDL_Keycode.SDLK_ESCAPE -> (stop config, state)
  | SDL.SDL_Keycode.SDLK_f -> (
      { config with
          DisplayConfig = Display.toggleFullscreen config.DisplayConfig },
      state
    )
  | _ -> configState

let internal handleEvent (configState:Config * 'T) (event:SDL.SDL_Event) =
  let (config, state) = configState
  match event.typeFSharp with
  | SDL.SDL_EventType.SDL_KEYUP -> handleKeyUp (config, state) event
  | SDL.SDL_EventType.SDL_QUIT -> handleQuit (config, state) event
  | _ -> configState

////////////
// Module //
////////////

let private drawLoop<'T> (drawHandler:Display.Config * 'T -> Display.Config * 'T) (configState:Config * 'T) =
  let (config, state) = configState
  let (updatedConfig, updatedState) = drawHandler (config.DisplayConfig, state)
  (
    { config with
        DisplayConfig = updatedConfig },
    updatedState
  )

let rec private eventLoop (configState:Config * 'T) =
  let mutable event = SDL.SDL_Event()
  if SDL.SDL_PollEvent(&event) = 0 then
    configState
  else
    handleEvent configState event |> eventLoop

let internal shutdown (configState: Config * 'T) : Config * 'T =
  let (config, state) = configState
  warn "Shutting down internals"
  SDL.SDL_Quit()
  (stop config, state)

let drawBegin (config:Display.Config) =
  Display.clear config

let drawEnd (config:Display.Config) =
  Display.swap config

let internal drawHandlerDefault<'T> (configState:Display.Config * 'T) =
  let (config, state) = configState
  ( Display.clear config
      |> Display.swap,
    state
  )

let internal initHandlerDefault<'T> (configState:Config * 'T) : Config * 'T =
  configState

let internal updateDefault<'T>
  (drawHandler:Display.Config * 'T -> Display.Config * 'T)
  (configState:Config * 'T) : Config * 'T =
    eventLoop configState |> drawLoop drawHandler

let rec internal updateLoop<'T> (updateImpl:Config * 'T -> Config * 'T) (configState:Config * 'T) : Config * 'T =
  let (config, _) = configState
  if not config.IsRunning then
    configState
  else
    updateImpl configState |> updateLoop updateImpl
