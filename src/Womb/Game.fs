module Womb.Game

open Womb.Engine.Internals
open Womb.Graphics
open Womb.Logging

let play<'T>
  title
  width
  height
  (state:'T)
  (initHandlerOpt: option<Config -> 'T -> Config * 'T>)
  (drawHandlerOpt: option<Display.Config -> 'T -> Display.Config * 'T>) =
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

    let defaultReturn config exitCode =
      printf "Default returning %d" exitCode
      { config with ExitCode = exitCode }

    let curriedUpdate =
      updateDefault
        ( match drawHandlerOpt with
          | Some(drawHandler) -> drawHandler
          | None -> drawHandlerDefault )

    match Display.initialize config.DisplayConfig with
    | None ->
        fail "Failed to initialize display"
        defaultReturn config 1
    | Some(displayConfig) ->
        let init =
          match initHandlerOpt with
          | Some(initHandler) -> initHandler
          | None -> initHandlerDefault
        info "Running initialization code"
        updateLoop
          curriedUpdate
            { (init config) with
                DisplayConfig = displayConfig }
        |> shutdown
