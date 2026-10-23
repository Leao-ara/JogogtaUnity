#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using IcoRP;

public static class IcoRepairGameplay
{
    [MenuItem("Icó RP/Corrigir camera e entrada nos carros")]
    public static void Repair()
    {
        var scene=SceneManager.GetActiveScene();
        if(!scene.IsValid()||scene.name!="Ico_Prototipo_3D")
        {
            EditorUtility.DisplayDialog("Icó RP","Abra Ico_Prototipo_3D primeiro.","OK");return;
        }
        var player=GameObject.Find("ICO_JOGADOR_TESTE");
        var cars=GameObject.Find("ICO_VEICULOS_TESTE");
        if(!player||!cars)
        {
            EditorUtility.DisplayDialog("Icó RP","Adicione o personagem e os carros antes de corrigir.","OK");return;
        }
        IcoPlayerSceneSetup.Setup();
        var cam=GameObject.Find("ICO_CAMERA_JOGADOR");
        if(cam)
        {
            cam.tag="MainCamera";
            var camera=cam.GetComponent<Camera>();
            if(camera)camera.enabled=true;
            var follow=cam.GetComponent<IcoFollowCamera>();
            if(follow){follow.target=player.transform;follow.enabled=true;}
        }
        if(!player.GetComponent<IcoVehicleInteraction>())Undo.AddComponent<IcoVehicleInteraction>(player);
        // If cars were spawned far from the player, place player near the first one.
        Transform first=null;
        foreach(Transform child in cars.transform)
            if(child.GetComponent<IcoVehicle>()){first=child;break;}
        if(first && Vector3.Distance(player.transform.position,first.position)>15f)
        {
            Undo.RecordObject(player.transform,"Aproximar jogador dos carros");
            var controller=player.GetComponent<CharacterController>();
            if(controller)controller.enabled=false;
            player.transform.position=first.position+first.right*3f+Vector3.up*.4f;
            if(controller)controller.enabled=true;
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Icó RP: câmera principal e interação reparadas. No Play, clique na janela Game e pressione E perto do carro.");
    }
}
#endif
