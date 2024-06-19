# Issues

## Important Links

- [Official Issue Tracker](https://github.com/exokomodo/openwomb/issues)

### Why?

Sick of not having issues co-local to the codebase, and this allows for more freeform planning and discussion, before dumping it into a PR or official issue. Also allows for platform-agnostic issue handling if we ever move off of Github.

## Open

To check all open issues on Github, [go here](https://github.com/ExoKomodo/openwomb/issues?q=is%3Aopen+is%3Aissue)

## In-Progress

### Render textures/sprites

#### [Issue Link](https://github.com/ExoKomodo/openwomb/issues/3)

#### Branch - [3_sprites](https://github.com/exokomodo/openwomb/tree/3_sprites)

#### Overview

There are 2 different ways to render images with SDL:

1. Using [SDL_Texture](https://lazyfoo.net/tutorials/SDL/07_texture_loading_and_rendering/index.php) (terrible option for general use but great for low-overhead texturing)
1. Using OpenGL to render textures in a shader (best option for overall usage)

This issue is to only implement the OpenGL way, as it provides a general-purpose use case, since rendering to a quad is all that is "needed" for dealing with images and textures in graphics.

#### Instructions

1. Read the [OpenGL tutorial for rendering textures to a quad](https://learnopengl.com/Getting-started/Textures). Now prepare for implementation.
1. Will require some changes to the [Primitives.ShadedObject.From function](./src/Womb/Graphics/Primitives.fs)
1. Will require some changes to the [Primitives.ShadedObject.UseMvpShader function](./src/Womb/Graphics/Primitives.fs)
1. Will require some changes to the [example shaders in the Playground example](./examples/Playground/Resources/Shaders/)

#### Acceptance Criteria

Be able to render an image to a shaded quad. To demonstrate this, modify the [Playground example](./examples/Playground) to render the [hello_world.bmp](./examples/Playground/Resources/Textures/hello_world.bmp). For proper completeness, demonstrate how we can color the texture in the shader as well.

## Closed

To check all closed issues on Github, [go here](https://github.com/ExoKomodo/openwomb/issues?q=is%3Aissue+is%3Aclosed)
