module Womb.Types

open SDL2Bindings
open System.Diagnostics
open Womb.Graphics
open Womb.Graphics.Types
open Womb.Input.Types
open Womb.Lib.Math
open Womb.Logging

type Config<'T> =
  { DisplayConfig: DisplayConfig;
    IsRunning: bool;
    IsShutdown: bool;
    ExitCode: int;
    ResourcePathRoot: string;
    State: 'T;
    Mouse: MouseState;
    InitHandler: Config<'T> -> Config<'T>;
    EventHandler: Config<'T> -> SDL.SDL_Event -> Config<'T>;
    LoopHandler: Config<'T> -> Config<'T>;
    StopHandler: Config<'T> -> Config<'T>;
    FrameTimer: Stopwatch;
    FrameDelta: System.TimeSpan;
    OverallTimer: Stopwatch;
    OverallDelta: System.TimeSpan; }

    static member Default<'T> (state:'T) =
      let stopHandler config =
        debug "Handling quit event and may call twice"
        if config.IsRunning then
          { config with
              DisplayConfig = Display.shutdown config.DisplayConfig
              IsRunning = false }
        else
          config
      { DisplayConfig = DisplayConfig.Default()
        IsRunning = false
        IsShutdown = false
        ExitCode = 0
        ResourcePathRoot = "Resources"
        State = state
        Mouse = MouseState.Default
        InitHandler = fun config -> config
        EventHandler = fun config (event:SDL.SDL_Event) ->
          match event.key.keysym.sym with
          | SDL.SDL_Keycode.SDLK_ESCAPE -> stopHandler config
          | SDL.SDL_Keycode.SDLK_f ->
            { config with
                DisplayConfig = Display.toggleFullscreen config.DisplayConfig }
          | _ -> config
        LoopHandler = fun config ->
          let displayConfig = Display.clear config.DisplayConfig
          { config with
              DisplayConfig = Display.swap displayConfig }
        StopHandler = stopHandler
        OverallTimer = new Stopwatch()
        FrameTimer = new Stopwatch()
        FrameDelta = new System.TimeSpan()
        OverallDelta = new System.TimeSpan() }

    member this.VirtualMousePosition() =
      let (x, y) = this.Mouse.Position
      let viewport = new System.Numerics.Vector2(this.DisplayConfig.Width |> single, this.DisplayConfig.Height |> single)
      let curried_map = map 0f 1f -1f 1f
      let virtualPosition = (
        new System.Numerics.Vector2(x, y) /
        viewport
      )
      (curried_map virtualPosition.X, curried_map virtualPosition.Y)
