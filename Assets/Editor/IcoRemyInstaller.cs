#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using IcoRP;

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
        bool newlyCreated=false;
        if(!remy)
        {
            var existing=GameObject.Find("Remy");
            if(existing && existing.scene==scene)
            {
                remy=existing.transform;
                newlyCreated=true;
                Undo.SetTransformParent(remy,visual,"Mover Remy para jogador");
            }
            else
            {
                var instance=(GameObject)PrefabUtility.InstantiatePrefab(asset,scene);
                Undo.RegisterCreatedObjectUndo(instance,"Adicionar Remy");
                remy=instance.transform;
                newlyCreated=true;
                Undo.SetTransformParent(remy,visual,"Vincular Remy");
            }
        }
        Undo.RecordObject(remy,"Ajustar Remy");
        if(newlyCreated)
        {
            remy.localPosition=Vector3.zero;
            remy.localRotation=Quaternion.identity;
            remy.localScale=Vector3.one;
        }
        var animator=remy.GetComponent<Animator>();
        if(animator)
        {
            Undo.RecordObject(animator,"Desativar root motion");
            animator.applyRootMotion=false;
            EditorUtility.SetDirty(animator);
        }
        // The temporary bone script rotates Mixamo arms backwards on this avatar.
        var procedural=remy.GetComponent<IcoRemyProceduralMotion>();
        if(!procedural)procedural=Undo.AddComponent<IcoRemyProceduralMotion>(remy.gameObject);
        Undo.RecordObject(procedural,"Ativar movimento humanoide");
        procedural.enabled=true;
        if(newlyCreated)FitRemyToPlayer(remy,visual);
        RepairMaterials(remy);
        remy.gameObject.SetActive(true);
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
        EditorUtility.DisplayDialog("Icó RP","Remy vinculado ao jogador, sem alterar a posicao do jogador na cidade. "+hidden+" renderizadores antigos ocultados. Salve e teste na aba Game. Movimento humanoide provisório ativado. Para animacoes profissionais, importe clips de caminhada/corrida.","OK");
    }

    static void FitRemyToPlayer(Transform remy,Transform visual)
    {
        // Scale using the imported mesh bounds rather than assuming FBX units.
        var meshes=remy.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        Bounds bounds=new Bounds();
        bool hasBounds=false;
        foreach(var mesh in meshes)
        {
            if(!mesh.sharedMesh)continue;
            var b=mesh.bounds;
            if(b.size.y<.01f)continue;
            if(!hasBounds){bounds=b;hasBounds=true;}else bounds.Encapsulate(b);
        }
        if(!hasBounds)
        {
            foreach(var mesh in remy.GetComponentsInChildren<MeshRenderer>(true))
            {if(!hasBounds){bounds=mesh.bounds;hasBounds=true;}else bounds.Encapsulate(mesh.bounds);}
        }
        if(!hasBounds || bounds.size.y<.01f)return;
        float ratio=Mathf.Clamp(1.75f/bounds.size.y,.01f,100f);
        remy.localScale=remy.localScale*ratio;
        // Recompute after scaling; align feet to the visual container's origin.
        Physics.SyncTransforms();
        hasBounds=false;
        foreach(var mesh in remy.GetComponentsInChildren<Renderer>(true))
        {
            if(mesh.bounds.size.y<.01f)continue;
            if(!hasBounds){bounds=mesh.bounds;hasBounds=true;}else bounds.Encapsulate(mesh.bounds);
        }
        if(hasBounds)
        {
            Vector3 offset=visual.position-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            remy.position+=offset;
        }
        Debug.Log("Ico RP: Remy ajustado para aproximadamente 1,75m de altura.");
    }

    static void RepairMaterials(Transform remy)
    {
        // Use existing textures where present. A plain FBX cannot supply missing images.
        var shader=Shader.Find("Universal Render Pipeline/Lit");
        if(!shader)shader=Shader.Find("Standard");
        if(!shader)return;
        int adjusted=0,missing=0;
        foreach(var renderer in remy.GetComponentsInChildren<Renderer>(true))
        {
            var originals=renderer.sharedMaterials;
            var changed=(Material[])originals.Clone();
            bool rendererChanged=false;
            for(int i=0;i<originals.Length;i++)
            {
                var source=originals[i];
                if(!source)continue;
                var tex=source.HasProperty("_BaseMap")?source.GetTexture("_BaseMap"):null;
                if(!tex && source.HasProperty("_MainTex"))tex=source.GetTexture("_MainTex");
                if(!tex)missing++;
                bool broken=!source.shader || source.shader.name=="Hidden/InternalErrorShader";
                if(!broken && !tex)continue;
                if(!broken)continue;
                var replacement=new Material(shader);
                replacement.name=source.name+"_IcoRP";
                if(tex)
                {
                    if(replacement.HasProperty("_BaseMap"))replacement.SetTexture("_BaseMap",tex);
                    if(replacement.HasProperty("_MainTex"))replacement.SetTexture("_MainTex",tex);
                }
                if(source.HasProperty("_Color") && replacement.HasProperty("_Color"))
                    replacement.color=source.color;
                if(source.HasProperty("_BaseColor") && replacement.HasProperty("_BaseColor"))
                    replacement.SetColor("_BaseColor",source.GetColor("_BaseColor"));
                const string folder="Assets/IcoRP/Personagens/GeneratedMaterials";
                if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/IcoRP/Personagens","GeneratedMaterials");
                string filename=System.Text.RegularExpressions.Regex.Replace(source.name,@"[^a-zA-Z0-9_-]","_");
                string materialPath=AssetDatabase.GenerateUniqueAssetPath(folder+"/"+filename+".mat");
                AssetDatabase.CreateAsset(replacement,materialPath);
                changed[i]=replacement;
                adjusted++;
                rendererChanged=true;
            }
            if(rendererChanged)
            {
                Undo.RecordObject(renderer,"Corrigir shader do Remy");
                renderer.sharedMaterials=changed;
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Ico RP: shaders corrigidos: "+adjusted+"; materiais sem textura associada: "+missing+
                  ". Se o personagem continuar branco, importe as imagens de textura do Mixamo.");
    }
}
#endif