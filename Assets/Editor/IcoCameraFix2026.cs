#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using IcoRP;

public static class IcoCameraFix2026
{
    [MenuItem("Icó RP/Atualizar camera de acompanhamento")]
    public static void Apply()
    {
        var scene=SceneManager.GetActiveScene();
        if(!scene.IsValid()||scene.name!="Ico_Prototipo_3D")
        {
            EditorUtility.DisplayDialog("Icó RP","Abra a cena Ico_Prototipo_3D.","OK");return;
        }
        var player=GameObject.Find("ICO_JOGADOR_TESTE");
        if(!player){EditorUtility.DisplayDialog("Icó RP","Jogador nao encontrado na cena.","OK");return;}
        var go=GameObject.Find("ICO_CAMERA_JOGADOR");
        if(!go){go=new GameObject("ICO_CAMERA_JOGADOR");Undo.RegisterCreatedObjectUndo(go,"Camera Icó");}
        go.SetActive(true);
        go.tag="MainCamera";
        var cam=go.GetComponent<Camera>();
        if(!cam)cam=Undo.AddComponent<Camera>(go);
        cam.enabled=true;
        cam.depth=100;
        var follow=go.GetComponent<IcoFollowCamera>();
        if(!follow)follow=Undo.AddComponent<IcoFollowCamera>(go);
        follow.enabled=true;
        follow.target=player.transform;
        follow.avoidObstacles=false;
        follow.distance=6f;
        follow.height=2.2f;
        go.transform.position=player.transform.position+new Vector3(0,3,-6);
        go.transform.LookAt(player.transform.position+Vector3.up*1.6f);
        var listener=go.GetComponent<AudioListener>();
        if(!listener)listener=Undo.AddComponent<AudioListener>(go);
        listener.enabled=true;
        foreach(var other in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            if(other==cam)continue;
            Undo.RecordObject(other,"Desativar camera antiga");
            other.enabled=false;
            if(other.CompareTag("MainCamera"))other.tag="Untagged";
        }
        foreach(var other in Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
            if(other!=listener)other.enabled=false;
        var movement=player.GetComponent<IcoThirdPersonPlayer>();
        if(movement){movement.enabled=true;movement.view=go.transform;}
        if(!player.GetComponent<IcoVehicleInteraction>())Undo.AddComponent<IcoVehicleInteraction>(player);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorUtility.DisplayDialog("Icó RP","Camera configurada. Aperte Play e clique na aba GAME (nao Scene) para testar. Ao entrar no carro com E, a camera muda de alvo.","OK");
    }
}
#endif
