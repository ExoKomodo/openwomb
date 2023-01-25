module Womb.Lib.Math

open Womb.Lib.Types

let inline map a1 a2 b1 b2 s =
    b1 + (s - a1) * (b2 - b1) / (a2 - a1)
