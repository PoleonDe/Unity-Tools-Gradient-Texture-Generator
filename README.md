# Control Tools - Gradient Texture Generator

ScriptableObject tooling for generating 1D texture assets from Unity gradients. Create a generator asset, edit a Unity `Gradient`, and the package stores the generated 1-pixel-high `Texture2D` as a sub-asset.

## Requirements

Unity 6 / 6000.0 or newer.

## Installation

Install the package directly from GitHub with Unity Package Manager:

1. Open `Window > Package Manager` in Unity.
2. Select `+ > Install package from git URL...`.
3. Enter this URL:

```text
https://github.com/PoleonDe/Unity-Tools-Gradient-Texture-Generator.git
```

You can also add the package to `Packages/manifest.json`:

```json
"com.control-tools.gradient-texture-generator": "https://github.com/PoleonDe/Unity-Tools-Gradient-Texture-Generator.git"
```

To install a specific version later, create and push a Git tag, then append it to the URL, for example `#0.1.0`.

## Basic Usage

Create a generator asset from:

```text
Assets/Create/Control Tools/Gradient Texture Generator
```

Edit the gradient or width in the inspector. The generated texture is stored as a sub-asset of the generator asset and is regenerated automatically after inspector changes. Use `Regenerate` when you need to rebuild it manually.

```csharp
using Control.Tools;
using UnityEngine;

public sealed class GradientTextureExample : MonoBehaviour
{
    public GradientTextureGenerator generator;
}
```

## Folder Structure

- `Runtime/`: the `GradientTextureGenerator` asset API.
- `Editor/`: Unity Editor inspector tooling.
- `Tests/`: package test folders.
- `Samples~/Basic Usage/`: importable sample notes.
- `Documentation~/`: setup and usage notes.

## Known Limitations

- Asset regeneration runs in the Unity Editor only.
- Generated textures are stored as sub-assets of the generator asset.

## Changelog

See [CHANGELOG.md](CHANGELOG.md).
