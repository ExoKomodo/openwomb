module Womb.Graphics.Types

open System.Numerics
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

type ShaderProgram =
  { Id: uint;
    FragmentPaths: list<string>;
    VertexPaths: list<string>; }

    static member Default =
      { Id = 0u
        FragmentPaths = list.Empty
        VertexPaths = list.Empty }

type Uniform =
  | Matrix4x4Uniform of Name:string * Data:Matrix4x4
  | Vector1Uniform of Name:string * Data:single
  | Vector2Uniform of Name:string * Data:Vector2
  | Vector3Uniform of Name:string * Data:Vector3
  | Vector4Uniform of Name:string * Data:Vector4
  | VectorUniform of Name:string * Data:array<single>
  | IVector1Uniform of Name:string * Data:int
  | IVector2Uniform of Name:string * Data:int * int
  | IVector3Uniform of Name:string * Data:int * int * int
  | IVector4Uniform of Name:string * Data:int * int * int * int
  | IVectorUniform of Name:string * Data:array<int>
  | UVector1Uniform of Name:string * Data:uint
  | UVector2Uniform of Name:string * Data:uint * uint
  | UVector3Uniform of Name:string * Data:uint * uint * uint
  | UVector4Uniform of Name:string * Data:uint * uint * uint * uint
  | UVectorUniform of Name:string * Data:array<uint>
