module Womb.Logging

open System

#if INLINE_LOGGER
let inline
#else
let
#endif
  debug_if condition message =
    if condition then
      Console.ForegroundColor <- ConsoleColor.Cyan
      printfn $"DEBUG:WOMB:%s{message}"
      Console.ResetColor()

#if INLINE_LOGGER
let inline
#else
let
#endif
  debug message =
    #if DEBUG
    Console.ForegroundColor <- ConsoleColor.Cyan
    printfn $"DEBUG:WOMB:%s{message}"
    Console.ResetColor()
    #else
    ()
    #endif

#if INLINE_LOGGER
let inline
#else
let
#endif
  fail message =
    Console.ForegroundColor <- ConsoleColor.Red
    printfn $"FAIL:WOMB:%s{message}"
    Console.ResetColor()

#if INLINE_LOGGER
let inline
#else
let
#endif
  info message =
    Console.ForegroundColor <- ConsoleColor.White
    printfn $"INFO:WOMB:%s{message}"
    Console.ResetColor()

#if INLINE_LOGGER
let inline
#else
let
#endif
  warn message = 
    Console.ForegroundColor <- ConsoleColor.Yellow
    printfn $"WARN:WOMB:%s{message}"
    Console.ResetColor()
