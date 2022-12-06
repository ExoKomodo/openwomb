module Womb.Types

open SDL2Bindings
open System.Numerics
open Womb.Graphics
open Womb.Graphics.Types
open Womb.Input.Types
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
    KeyUpHandler: Config<'T> -> SDL.SDL_Event -> Config<'T>;
    DrawHandler: Config<'T> -> Config<'T>;
    StopHandler: Config<'T> -> Config<'T>; }

    static member Default<'T> (state:'T) =
      let stopHandler config =
        debug "Handling quit event and may call twice"
        if config.IsRunning then
          { config with
              DisplayConfig = Display.shutdown config.DisplayConfig
              IsRunning = false }
        else
          config
      { DisplayConfig = DisplayConfig.Default
        IsRunning = false
        IsShutdown = false
        ExitCode = 0
        ResourcePathRoot = "Resources"
        State = state
        Mouse = MouseState.Default
        InitHandler = fun config -> config
        KeyUpHandler = fun config (event:SDL.SDL_Event) ->
          match event.key.keysym.sym with
          | SDL.SDL_Keycode.SDLK_ESCAPE -> stopHandler config
          | SDL.SDL_Keycode.SDLK_f ->
            { config with
                DisplayConfig = Display.toggleFullscreen config.DisplayConfig }
          | _ -> config
        DrawHandler = fun config ->
          let displayConfig = Display.clear config.DisplayConfig
          { config with
              DisplayConfig = Display.swap displayConfig }
        StopHandler = stopHandler }

    member this.VirtualMousePosition() =
      let virtualPosition = (
        this.Mouse.Position /
        new Vector2(this.DisplayConfig.Width |> single, this.DisplayConfig.Height |> single)
      )
      (virtualPosition.X, virtualPosition.Y)
