module Komodo.Game

open Komodo.Engine.Internals
open Komodo.Graphics
open Komodo.Logging

let play
  title
  width
  height
  (initHandlerOpt: option<Config -> Config>)
  (drawHandlerOpt: option<Display.Config -> Display.Config>) =
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
