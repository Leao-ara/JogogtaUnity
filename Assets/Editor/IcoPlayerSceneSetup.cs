#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using IcoRP;

public static class IcoPlayerSceneSetup
{
    [MenuItem("Icó RP/Colocar personagem na cidade")]
    public static void Setup()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != "Ico_Prototipo_3D")
        {
            EditorUtility.DisplayDialog("Icó RP",
                "Abra Assets/IcoCenas/Ico_Prototipo_3D.unity antes de adicionar o personagem.", "OK");
            return;
        }
        var player = GameObject.Find("ICO_JOGADOR_TESTE");
        if (player == null)
        {
            player = new GameObject("ICO_JOGADOR_TESTE");
            Undo.RegisterCreatedObjectUndo(player, "Criar jogador Icó");
            Vector3 spawn = new Vector3(0, 5, 0);
            // Prefer a road surface, falling back to the center of the scene.
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if (!r.gameObject.name.StartsWith("Via - ")) continue;
                spawn = r.bounds.center + Vector3.up * 3f;
                break;
            }
            if (Physics.Raycast(spawn + Vector3.up * 150f, Vector3.down,
                out RaycastHit hit, 400f, ~0, QueryTriggerInteraction.Ignore))
                spawn = hit.point + Vector3.up * 1.2f;
            player.transform.position = spawn;
            var capsule = player.AddComponent<CharacterController>();
            capsule.height = 1.85f; capsule.radius = .33f; capsule.center = Vector3.up * .93f;
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Corpo provisório";
            Undo.RegisterCreatedObjectUndo(body, "Corpo jogador");
            body.transform.SetParent(player.transform, false);
            body.transform.localPosition = Vector3.up * .93f;
            body.transform.localScale = new Vector3(.68f,.93f,.68f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            player.AddComponent<IcoThirdPersonPlayer>();
        }
        var camObj = GameObject.Find("ICO_CAMERA_JOGADOR");
        if (camObj == null)
        {
            camObj = new GameObject("ICO_CAMERA_JOGADOR");
            Undo.RegisterCreatedObjectUndo(camObj, "Criar câmera jogador");
            camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<IcoFollowCamera>();
        }
        var follow = camObj.GetComponent<IcoFollowCamera>();
        follow.target = player.transform;
        player.GetComponent<IcoThirdPersonPlayer>().view = camObj.transform;
        camObj.transform.position = player.transform.position + new Vector3(0,3,-6);
        // Keep original aerial camera object; disable only while testing the player.
        foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            if (cam.gameObject != camObj) { Undo.RecordObject(cam,"Desativar câmera aérea"); cam.enabled = false; }
        foreach (var listener in Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
            if (listener.gameObject != camObj) { Undo.RecordObject(listener,"Desativar áudio aéreo"); listener.enabled = false; }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Icó RP: jogador de teste instalado. Play: WASD, Shift, Espaço, botão direito do mouse.");
    }
}
#endif
