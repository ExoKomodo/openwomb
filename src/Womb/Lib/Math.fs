module Womb.Lib.Math

open Womb.Lib.Types

let map (a1: single) (a2: single) (b1: single) (b2: single) (s: single): single =
    b1 + (s - a1) * (b2 - b1) / (a2 - a1)
