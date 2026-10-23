#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class IcoRemyInstaller
{
    [MenuItem("Icó RP/Etapa 1/Colocar Remy no jogador automaticamente")]
    public static void Apply()
    {
        var scene=SceneManager.GetActiveScene();
        if(scene.name!="Ico_Prototipo_3D")
        {
            EditorUtility.DisplayDialog("Icó RP","Abra a cena Ico_Prototipo_3D antes de instalar o Remy.","OK");return;
        }
        var player=GameObject.Find("ICO_JOGADOR_TESTE");
        if(!player){EditorUtility.DisplayDialog("Icó RP","ICO_JOGADOR_TESTE nao encontrado.","OK");return;}
        var visual=player.transform.Find("ICO_MODELO_PERSONAGEM");
        if(!visual){EditorUtility.DisplayDialog("Icó RP","ICO_MODELO_PERSONAGEM nao encontrado dentro do jogador.","OK");return;}
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/IcoRP/Personagens/Remy.fbx");
        if(!asset)
        {
            EditorUtility.DisplayDialog("Icó RP","Nao encontrei Assets/IcoRP/Personagens/Remy.fbx. Confira o nome e o caminho do arquivo.","OK");return;
        }
        // Preserve a manually positioned Remy if it is already parented correctly.
        Transform remy=visual.Find("Remy");
        if(!remy)
        {
            var existing=GameObject.Find("Remy");
            if(existing && existing.scene==scene)
            {
                remy=existing.transform;
                Undo.SetTransformParent(remy,visual,"Mover Remy para jogador");
            }
            else
            {
                var instance=(GameObject)PrefabUtility.InstantiatePrefab(asset,scene);
                Undo.RegisterCreatedObjectUndo(instance,"Adicionar Remy");
                remy=instance.transform;
                Undo.SetTransformParent(remy,visual,"Vincular Remy");
            }
        }
        Undo.RecordObject(remy,"Ajustar Remy");
        remy.localPosition=Vector3.zero;
        remy.localRotation=Quaternion.identity;
        remy.localScale=Vector3.one;
        var animator=remy.GetComponent<Animator>();
        if(animator)
        {
            Undo.RecordObject(animator,"Desativar root motion");
            animator.applyRootMotion=false;
            EditorUtility.SetDirty(animator);
        }
        // Keep old geometry in the scene but hide its renderers only.
        int hidden=0;
        foreach(var renderer in visual.GetComponentsInChildren<Renderer>(true))
        {
            if(renderer.transform==remy || renderer.transform.IsChildOf(remy))continue;
            if(!renderer.enabled)continue;
            Undo.RecordObject(renderer,"Ocultar visual antigo");
            renderer.enabled=false;
            hidden++;
        }
        EditorUtility.SetDirty(remy);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject=remy.gameObject;
        EditorUtility.DisplayDialog("Icó RP","Remy vinculado ao jogador, sem alterar a posicao do jogador na cidade. "+hidden+" renderizadores antigos ocultados. Salve e teste na aba Game. As animacoes ainda precisam de um Animator Controller.","OK");
    }
}
#endif