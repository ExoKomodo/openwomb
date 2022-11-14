module Womb.Game

open Womb.Engine.Internals
open Womb.Graphics
open Womb.Logging

let play<'T>
  title
  width
  height
  (state:'T)
  (initHandlerOpt: option<Config * 'T -> Config * 'T>)
  (drawHandlerOpt: option<Display.Config * 'T -> Display.Config * 'T>) =
    info $"Starting up %s{title}"
    let config =
      { Config.Default with
          IsRunning = true
          DisplayConfig =
          { Display.Config.Default with
              Title = "Hello World"
              Width = width
              Height = height } }

    info "Loaded starting config"

    let defaultReturn config state exitCode =
      printf "Default returning %d" exitCode
      { config with ExitCode = exitCode }, state

    let curriedUpdate =
      updateDefault
        ( match drawHandlerOpt with
          | Some(drawHandler) -> drawHandler
          | None -> drawHandlerDefault )

    match Display.initialize config.DisplayConfig with
    | None ->
        fail "Failed to initialize display"
        defaultReturn config state 1
    | Some(displayConfig) ->
        let init =
          match initHandlerOpt with
          | Some(initHandler) -> initHandler
          | None -> initHandlerDefault
        info "Running initialization code"
        let (initConfig, initState) = init (config, state)
        updateLoop
          curriedUpdate
          ( { initConfig with
                DisplayConfig = displayConfig },
            initState)
        |> shutdown
