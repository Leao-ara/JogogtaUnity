#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using IcoRP;

public static class IcoRPSystemsSetup
{
    [MenuItem("Icó RP/Ferramentas anteriores/Preparar cena Sistemas RP")]
    public static void Prepare()
    {
        var scene=SceneManager.GetActiveScene();
        if(!scene.IsValid() || scene.name!="SISTEMASRP")
        {
            EditorUtility.DisplayDialog("Icó RP","Abra Assets/SISTEMASRP.unity antes de preparar os sistemas.","OK");
            return;
        }
        var root=GameObject.Find("ICO_RP_SISTEMAS");
        if(root==null)
        {
            root=new GameObject("ICO_RP_SISTEMAS");
            Undo.RegisterCreatedObjectUndo(root,"Preparar sistemas RP");
        }
        if(root.GetComponent<RPSystemsRoot>()==null) Undo.AddComponent<RPSystemsRoot>(root);
        EnsureChild(root.transform,"Contas e RG (servidor futuro)");
        EnsureChild(root.transform,"Inventario e skins (servidor futuro)");
        EnsureChild(root.transform,"Veiculos e garagens (servidor futuro)");
        EnsureChild(root.transform,"Casas e propriedades (servidor futuro)");
        EnsureChild(root.transform,"Economia e empregos (servidor futuro)");
        EnsureChild(root.transform,"Multiplayer - servidor proprio futuro");
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Icó RP: estrutura inicial de sistemas salva; login, banco e multiplayer ainda precisam de implementação.");
    }
    static void EnsureChild(Transform parent,string name)
    {
        if(parent.Find(name)!=null)return;
        var child=new GameObject(name);
        Undo.RegisterCreatedObjectUndo(child,"Criar modulo RP");
        child.transform.SetParent(parent,false);
    }
}
#endif
