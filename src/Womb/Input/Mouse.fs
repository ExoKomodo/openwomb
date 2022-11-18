module Womb.Input.Mouse

open SDL2Bindings
open System.Numerics
open Womb.Types

type MouseState =
  { Position: Vector2 }

    static member Default = {
      Position = Vector2.Zero; }

let getState (config:Config<'T>) : MouseState =
  let displayConfig = config.DisplayConfig
  let (_, x, y) = SDL.SDL_GetMouseState()
  let (adjustedX, adjustedY) = (
    x,
    int(displayConfig.Height) - y
  )

  { MouseState.Default with
      Position = new Vector2(single(adjustedX), single(adjustedY)) }
