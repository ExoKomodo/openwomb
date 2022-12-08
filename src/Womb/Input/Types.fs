module Womb.Input.Types

open Womb.Lib.Types

type MouseState =
  { Position: Vector2 }

    static member Default = {
      Position = (0f, 0f) }
