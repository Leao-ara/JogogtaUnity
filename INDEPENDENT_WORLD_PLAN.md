# Independent world migration — experimental branch

Goal: build a standalone Unity game without GTA San Andreas data or copyrighted GTA assets.

## First isolated milestone

The editor script at `Assets/Editor/IndependentWorldSceneBuilder.cs` adds:
**Tools > Independent World > Create Test Scene**.

It creates `Assets/IndependentWorld/IndependentWorld_Test.unity` containing Unity built-in primitive placeholders, a camera and lighting. No GTA installation or files are required to generate this scene. The script does **not** alter the existing GTA loader, startup scenes, networking or build settings.

## Validation on a computer

1. Clone this branch **with submodules**.
2. Open with the Unity version required by `ProjectSettings/ProjectVersion.txt`; avoid upgrading immediately.
3. Check editor compilation and submodule availability.
4. Use the Tools menu to generate the independent test scene.
5. Open the scene, press Play and confirm the camera displays ground, capsule and vehicle placeholder without GTA data.
6. Record compile errors and determine whether unrelated legacy scripts prevent project-wide compilation.

No automated build or runtime test has been performed yet.

## Next steps (not implemented)

- Audit `Assets/Scripts/Behaviours/Loader.cs` GTA path, archives, collision, handling, animation and map dependencies.
- Design an independent content provider using original/licensed Unity assets.
- Integrate character controller, vehicle prefab, physics, and multiplayer incrementally.
- Add Android/Windows validation and performance tests.

**Important:** A standalone placeholder scene does not prove the existing gameplay or multiplayer can run without GTA files. Retain the original branch until independent replacements pass tests.
