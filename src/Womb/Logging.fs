module Womb.Logging

open System

[<Literal>]
let is_debug = false

let inline
  debug_if condition message =
    if condition then
      Console.ForegroundColor <- ConsoleColor.Cyan
      printfn $"DEBUG:WOMB:%s{message}"
      Console.ResetColor()

let inline
  debug message = debug_if is_debug message

let inline
  fail message =
    Console.ForegroundColor <- ConsoleColor.Red
    printfn $"FAIL:WOMB:%s{message}"
    Console.ResetColor()

let inline
  info message =
    Console.ForegroundColor <- ConsoleColor.White
    printfn $"INFO:WOMB:%s{message}"
    Console.ResetColor()

let inline
  warn message = 
    Console.ForegroundColor <- ConsoleColor.Yellow
    printfn $"WARN:WOMB:%s{message}"
    Console.ResetColor()
