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

let internal handleQuit config (event:SDL.SDL_Event) =
  config.StopHandler config

let internal handleKeyUp config (event:SDL.SDL_Event) =
  match event.key.keysym.sym with
  | SDL.SDL_Keycode.SDLK_ESCAPE -> stop config
  | SDL.SDL_Keycode.SDLK_f ->
    { config with
        DisplayConfig = Display.toggleFullscreen config.DisplayConfig }
  | _ -> config

let internal handleEvent config (event:SDL.SDL_Event) =
  match event.typeFSharp with
  | SDL.SDL_EventType.SDL_KEYUP -> handleKeyUp config event
  | SDL.SDL_EventType.SDL_QUIT -> handleQuit config event
  | _ -> config

////////////
// Module //
////////////

let private drawLoop<'T> (drawHandler:Display.Config -> 'T -> Display.Config * 'T) config state =
  let (updatedConfig, updatedState) = drawHandler config.DisplayConfig state
  (
    { config with
        DisplayConfig = updatedConfig },
    updatedState
  )

let rec private eventLoop config =
  let mutable event = SDL.SDL_Event()
  if SDL.SDL_PollEvent(&event) = 0 then
    config
  else
    let updated = handleEvent config event
    eventLoop updated

let internal shutdown (config: Config) : Config =
  warn "Shutting down internals"
  SDL.SDL_Quit()
  stop config

let drawBegin (config:Display.Config) =
  Display.clear config

let drawEnd (config:Display.Config) =
  Display.swap config

let internal drawHandlerDefault (config:Display.Config) =
  Display.clear config
  |> Display.swap

let internal initHandlerDefault<'T> (config:Config) (state:'T) : Config * 'T =
  (config, state)

let internal updateDefault<'T>
  (drawHandler:Display.Config -> 'T -> Display.Config * 'T)
  (config:Config)
  (state:'T) : Config *'T =
    eventLoop config |> drawLoop drawHandler

let rec internal updateLoop (updateImpl:Config -> Config) (config:Config) : Config =
  if not config.IsRunning then
    config
  else
    updateImpl config |> updateLoop updateImpl
