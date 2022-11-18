module Womb.Game

open SDL2Bindings
open Womb.Engine.Internals
open Womb.Graphics
open Womb.Logging

let play<'T>
  title
  width
  height
  (state:'T)
  (initHandlerOpt: option<Config<'T> -> Config<'T>>)
  (keyUpHandlerOpt: option<Config<'T> -> SDL.SDL_Event -> Config<'T>>)
  (drawHandlerOpt: option<Config<'T> -> Config<'T>>) =
    info $"Starting up %s{title}"
    let config =
      { Config.Default state with
          IsRunning = true
          DisplayConfig =
            { DisplayConfig.Default with
                Title = "Hello World"
                Width = width
                Height = height } }
    let config =
      { config with
          InitHandler =
            match initHandlerOpt with
              | Some(initHandler) -> initHandler
              | None -> config.InitHandler
          KeyUpHandler =
            match keyUpHandlerOpt with
              | Some(keyUpHandler) -> keyUpHandler
              | None -> config.KeyUpHandler
          DrawHandler =
            match drawHandlerOpt with
              | Some(drawHandler) -> drawHandler
              | None -> config.DrawHandler }

    info "Loaded starting config"

    let defaultReturn config state exitCode =
      printf "Default returning %d" exitCode
      { config with ExitCode = exitCode }

    match Display.initialize config.DisplayConfig with
    | None ->
        fail "Failed to initialize display"
        defaultReturn config state 1
    | Some(displayConfig) ->
        info "Running initialization code"
        let config = config.InitHandler config
        let config =
          updateLoop
            { config with
                DisplayConfig = displayConfig }
        shutdown config
