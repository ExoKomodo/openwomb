module Womb.Graphics.Types

open Womb.Backends.OpenGL.Api
open Womb.Backends.OpenGL.Api.Constants
open Womb.Logging

///////////
// Types //
///////////

open SDL2Bindings

type DisplayConfig =
  { Width: uint;
    Height: uint;
    Title: string;
    IsFullscreen: bool;
    WindowFlags: SDL.SDL_WindowFlags;
    Window: nativeint;
    Context: nativeint; }

    static member Default =
      { Width = 800u
        Height = 600u
        Title = "Womb"
        IsFullscreen = false
        WindowFlags = SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN
        Window = 0n
        Context = 0n }
