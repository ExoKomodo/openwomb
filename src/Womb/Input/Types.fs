module Womb.Input.Types

open System.Numerics

type MouseState =
  { Position: Vector2 }

    static member Default = {
      Position = Vector2.Zero; }
