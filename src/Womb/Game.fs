module Womb.Game

open SDL2Bindings
open Womb.Engine.Internals
open Womb.Graphics
open Womb.Graphics.Types
open Womb.Logging
open Womb.Types

let play<'T>
  title
  width
  height
  (state:'T)
  (initHandlerOpt: option<Config<'T> -> Config<'T>>)
  (eventHandlerOpt: option<Config<'T> -> SDL.SDL_Event -> Config<'T>>)
  (loopHandlerOpt: option<Config<'T> -> Config<'T>>) =
    info $"Starting up %s{title}"
    let config =
      { Config.Default state with
          IsRunning = true
          DisplayConfig =
            { DisplayConfig.Default() with
                Title = title
                Width = width
                Height = height } }
    let config =
      { config with
          InitHandler =
            match initHandlerOpt with
              | Some initHandler -> initHandler
              | None -> config.InitHandler
          EventHandler =
            match eventHandlerOpt with
              | Some eventHandler -> eventHandler
              | None -> config.EventHandler
          LoopHandler =
            match loopHandlerOpt with
              | Some loopHandler -> loopHandler
              | None -> config.LoopHandler }

    info "Loaded starting config"

    let defaultReturn config state exitCode =
      printf "Default returning %d" exitCode
      { config with ExitCode = exitCode }

    match Display.initialize config.DisplayConfig with
    | None ->
        fail "Failed to initialize display"
        defaultReturn config state 1
    | Some displayConfig ->
        info "Running initialization code"
        let config =
          config.InitHandler config
        config.FrameTimer.Start()
        config.OverallTimer.Start()
        let config =
          updateLoop
            { config with
                DisplayConfig = displayConfig }
        shutdown config
