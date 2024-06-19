module Womb.Graphics.Types

#nowarn "9" // unverifiable IL due to NativePtr library usage

open Microsoft.FSharp.NativeInterop

open SDL2Bindings
open Womb.Lib.Types
open Womb.Logging

type DisplayConfig =
  { Width: uint;
    Height: uint;
    Title: string;
    IsFullscreen: bool;
    WindowFlags: SDL.SDL_WindowFlags;
    Window: nativeint;
    Context: nativeint; }

    static member Default() =
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

    static member Default() =
      { Id = 0u
        FragmentPaths = list.Empty
        VertexPaths = list.Empty }

type Uniform =
  | Matrix4x4Uniform of Name:string * Data:System.Numerics.Matrix4x4
  | Vector1Uniform of Name:string * Data:Vector1
  | Vector2Uniform of Name:string * Data:Vector2
  | Vector3Uniform of Name:string * Data:Vector3
  | Vector4Uniform of Name:string * Data:Vector4
  | VectorUniform of Name:string * Data:array<single>
  | IVector1Uniform of Name:string * Data:IVector1
  | IVector2Uniform of Name:string * Data:IVector2
  | IVector3Uniform of Name:string * Data:IVector3
  | IVector4Uniform of Name:string * Data:IVector4
  | IVectorUniform of Name:string * Data:array<int>
  | UVector1Uniform of Name:string * Data:UVector1
  | UVector2Uniform of Name:string * Data:UVector2
  | UVector3Uniform of Name:string * Data:UVector3
  | UVector4Uniform of Name:string * Data:UVector4
  | UVectorUniform of Name:string * Data:array<uint>
  | SamplerUniform of Name:string * Data:IVector1

type Texture2D = 
  { Surface: option<nativeptr<SDL.SDL_Surface>>;
    Width: int;
    Height: int;
    Data: voidptr;
    Format: nativeint; }

    static member Default() =
      { Surface = None
        Width = 0
        Height = 0
        Data = 0n.ToPointer()
        Format = 0n }

    static member FromPath path =
      let surfacePtr = Image.IMG_Load(path)
      if NativePtr.isNullPtr  surfacePtr then
        fail $"Failed to load surface from path - {path}"
        Texture2D.Default()
      else
        let surface = NativePtr.get surfacePtr 0
        { Surface = Some surfacePtr
          Width = surface.w
          Height = surface.h
          Data = surface.pixels.ToPointer()
          Format = surface.format }
