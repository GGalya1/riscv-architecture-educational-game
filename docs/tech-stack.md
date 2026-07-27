# Technology

!!! warning "Draft"
    This is a test version of the documentation site, and this page is a draft. Content is basic for now and will grow.

## Engine

- **Unity 6**, Universal Render Pipeline (URP)
- C# (it is preferable to use **Rider**)

## Platforms

- Android (Google Play)
- Windows, Linux (GitHub, Itch.io)

## A few things worth knowing about the codebase

- **Logic is decoupled from Unity where it matters.** The core components - `ALU`, `Register`, `RegisterFile`, `Extender`, `Multiplexer`, `DataInstMemory` - are plain C# classes with no `MonoBehaviour`/scene dependency. That's deliberate: it means the actual computer-architecture logic can be **unit-tested directly**, without spinning up a scene.
- **A shared two-phase clock model.** Every stateful component exposes `PreClockUpdate()` (buffers what the next value *will* be) and `Clock()` (commits it).
- **Localization** currently covers English and German.

See [Architecture](architecture.md) for the deeper structural walkthrough, and [Contributing](contributing.md) for how changes get made (branching, PRs, what CI checks).
