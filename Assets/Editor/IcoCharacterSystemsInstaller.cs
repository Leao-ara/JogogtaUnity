#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using IcoRP;

public static class IcoCharacterSystemsInstaller
{
    [MenuItem("Icó RP/Etapa 1/Instalar sistemas na cena SISTEMASRP")]
    public static void InstallSystems()
    {
        var scene=SceneManager.GetActiveScene();
        if(scene.name!="SISTEMASRP")
        {
            EditorUtility.DisplayDialog("Icó RP","Abra Assets/SISTEMASRP.unity antes de instalar os sistemas.","OK");
            return;
        }
        var root=GameObject.Find("ICO_SISTEMAS_PERSONAGEM");
        if(!root)
        {
            root=new GameObject("ICO_SISTEMAS_PERSONAGEM");
            Undo.RegisterCreatedObjectUndo(root,"Criar sistemas do personagem");
        }
        if(!root.GetComponent<IcoCharacterSystemsConfig>())
            Undo.AddComponent<IcoCharacterSystemsConfig>(root);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorUtility.DisplayDialog("Icó RP","Configuracao dos sistemas salva em SISTEMASRP. Agora abra a cena da cidade e use o outro comando da Etapa 1.","OK");
    }

    [MenuItem("Icó RP/Etapa 1/Instalar componentes no jogador da cidade")]
    public static void InstallPlayer()
    {
        var scene=SceneManager.GetActiveScene();
        if(scene.name!="Ico_Prototipo_3D")
        {
            EditorUtility.DisplayDialog("Icó RP","Abra Assets/IcoCenas/Ico_Prototipo_3D.unity.","OK");
            return;
        }
        var player=GameObject.Find("ICO_JOGADOR_TESTE");
        if(!player)
        {
            EditorUtility.DisplayDialog("Icó RP","Jogador ICO_JOGADOR_TESTE nao encontrado. Nao sera criado outro.","OK");
            return;
        }
        if(!player.GetComponent<IcoCharacterHealth>())Undo.AddComponent<IcoCharacterHealth>(player);
        if(!player.GetComponent<IcoCharacterFall>())Undo.AddComponent<IcoCharacterFall>(player);
        if(!player.GetComponent<IcoCharacterAnimationState>())Undo.AddComponent<IcoCharacterAnimationState>(player);
        if(!player.GetComponent<IcoWalkAnimation>())Undo.AddComponent<IcoWalkAnimation>(player);
        if(!player.GetComponent<IcoVehicleInteraction>())Undo.AddComponent<IcoVehicleInteraction>(player);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorUtility.DisplayDialog("Icó RP","Jogador atualizado sem duplicar objetos. WASD, Shift, Espaco, C ou Ctrl para agachar, E para entrar/sair do carro.","OK");
    }
}
#endif
