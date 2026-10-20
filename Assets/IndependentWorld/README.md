# Independent World prototype

This folder intentionally avoids dependencies on the original GTA SA data formats.

## Prototype vehicle catalog
- Create a vehicle asset through **Assets > Create > Independent World > Vehicle Definition**.
- Assign a legally usable Unity vehicle prefab and configure its name, ID, mass and max-speed metadata.
- Attach `IndependentVehicleSpawner` to an empty scene GameObject and assign the vehicle asset and an optional spawn transform.
- The spawner instantiates the prefab and applies Rigidbody mass if present.
- **Not implemented:** driving controls, wheel physics, networking, garage persistence, max-speed enforcement or integration with the existing game modes.

## Test scene
Run **Tools > Independent World > Create Test Scene** in the Unity Editor.
This produces a separate scene with built-in Unity primitives; it does not replace the project's normal startup flow.

## Safety and compatibility
- The original `Loader.cs`, GTA-specific `VehicleDef` and networking are unchanged.
- The prototype is not a proof that the entire project runs without GTA assets.
- Use Unity **2020.3.26f1** initially, matching `ProjectSettings/ProjectVersion.txt`.
- Clone with submodules and test compilation before attempting to merge the draft PR.
- All code changes are experimental and have not yet been executed in Unity.

## Keyboard movement prototype
- Regenerate the scene with **Tools > Independent World > Create Test Scene** after pulling these changes.
- The capsule placeholder now has a CharacterController and IndependentPlayerController.
- Use WASD or arrow keys to move, Left Shift to run and Space to jump (legacy Unity Input Manager bindings).
- The camera follows the placeholder. No mouse orbit or touch controls are implemented yet.
- The scene and components have not been compiled or run in Unity; test for project-wide errors first.
