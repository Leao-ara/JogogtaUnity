// Independent test scene generator. Does not load or redistribute GTA San Andreas assets.
// In Unity: Tools > Independent World > Create Test Scene.
// This is an isolated proof of concept, not a replacement for the existing Loader.
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using IndependentWorld;

public static class IndependentWorldSceneBuilder
{
    [MenuItem("Tools/Independent World/Create Test Scene")]
    public static void CreateTestScene()
    {
        const string folder = "Assets/IndependentWorld";
        const string scenePath = folder + "/IndependentWorld_Test.unity";

        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "IndependentWorld");

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Original Test Ground";
        ground.transform.position = new Vector3(0f, -0.5f, 0f);
        ground.transform.localScale = new Vector3(80f, 1f, 80f);

        var playerMarker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        playerMarker.name = "Player Placeholder - replace with licensed character";
        playerMarker.transform.position = new Vector3(0f, 1f, 0f);
        var capsuleCollider = playerMarker.GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
            Object.DestroyImmediate(capsuleCollider);
        playerMarker.AddComponent<CharacterController>();
        playerMarker.AddComponent<IndependentPlayerController>();

        var vehicleMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vehicleMarker.name = "Vehicle Placeholder - replace with original vehicle prefab";
        vehicleMarker.transform.position = new Vector3(5f, 0.75f, 3f);
        vehicleMarker.transform.localScale = new Vector3(2f, 1.5f, 4f);

        var lightObject = new GameObject("Directional Light");
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        lightObject.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.transform.position = new Vector3(0f, 12f, -20f);
        cameraObject.transform.LookAt(new Vector3(2f, 0f, 1f));
        var followCamera = cameraObject.AddComponent<IndependentFollowCamera>();
        followCamera.SetTarget(playerMarker.transform);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();
        Selection.activeGameObject = ground;
        Debug.Log("Independent test scene created at " + scenePath + ". No GTA SA files were used.");
    }
}
#endif
