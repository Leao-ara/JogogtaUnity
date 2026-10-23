#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class IcoRemyAppearance
{
    [MenuItem("Icó RP/Etapa 1/Recuperar texturas e roupas do Remy")]
    public static void Repair()
    {
        const string modelPath = "Assets/IcoRP/Personagens/Remy.fbx";
        var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        if (importer == null) { EditorUtility.DisplayDialog("Remy", "Arquivo Remy.fbx nao encontrado.", "OK"); return; }
        const string folder = "Assets/IcoRP/Personagens";
        const string texturesFolder = folder + "/Textures";
        const string materialsFolder = folder + "/GeneratedMaterials";
        if (!AssetDatabase.IsValidFolder(texturesFolder)) AssetDatabase.CreateFolder(folder, "Textures");
        if (!AssetDatabase.IsValidFolder(materialsFolder)) AssetDatabase.CreateFolder(folder, "GeneratedMaterials");

        // Extract embedded FBX images if they exist; extraction cannot invent missing textures.
        try { importer.ExtractTextures(texturesFolder); }
        catch (Exception e) { Debug.LogWarning("Remy: nenhuma textura embutida extraida: " + e.Message); }
        AssetDatabase.Refresh();

        var player = GameObject.Find("ICO_JOGADOR_TESTE");
        var visual = player ? player.transform.Find("ICO_MODELO_PERSONAGEM") : null;
        var remy = visual ? visual.Find("Remy") : null;
        if (!remy) { EditorUtility.DisplayDialog("Remy", "Primeiro abra a cena e instale o Remy no ICO_JOGADOR_TESTE.", "OK"); return; }

        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (!shader) shader = Shader.Find("Standard");
        if (!shader) { Debug.LogError("Remy: shader Lit/Standard nao encontrado."); return; }

        string[] ids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
        int textured = 0, fallback = 0;
        foreach (var renderer in remy.GetComponentsInChildren<Renderer>(true))
        {
            string part = renderer.gameObject.name.ToLowerInvariant();
            var originals = renderer.sharedMaterials;
            var assigned = new Material[originals.Length];
            for (int i = 0; i < originals.Length; i++)
            {
                Material source = originals[i];
                string name = source ? source.name : part;
                string key = (part + " " + name).ToLowerInvariant();
                string file = System.Text.RegularExpressions.Regex.Replace(part + "_" + i, @"[^a-z0-9_-]", "_");
                string path = materialsFolder + "/Remy_" + file + ".mat";
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!mat) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
                else mat.shader = shader;

                Texture texture = null;
                if (source)
                {
                    if (source.HasProperty("_BaseMap")) texture = source.GetTexture("_BaseMap");
                    if (!texture && source.HasProperty("_MainTex")) texture = source.GetTexture("_MainTex");
                }
                if (!texture)
                {
                    int best = -1;
                    foreach (string id in ids)
                    {
                        string candidate = AssetDatabase.GUIDToAssetPath(id);
                        string stem = Path.GetFileNameWithoutExtension(candidate).ToLowerInvariant();
                        int score = 0;
                        if (stem.Contains(part) && part.Length > 2) score += 4;
                        if (key.Contains(stem) && stem.Length > 3) score += 3;
                        if (stem.Contains("diffuse") || stem.Contains("albedo") || stem.Contains("basecolor")) score++;
                        if (score > best && score >= 3)
                        {
                            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(candidate);
                            best = score;
                        }
                    }
                }
                Color tint = Color.white;
                if (!texture)
                {
                    if (key.Contains("hair")) tint = new Color(.37f,.25f,.14f);
                    else if (key.Contains("top") || key.Contains("shirt")) tint = new Color(.19f,.21f,.23f);
                    else if (key.Contains("bottom") || key.Contains("short") || key.Contains("pant")) tint = new Color(.22f,.24f,.27f);
                    else if (key.Contains("shoe")) tint = new Color(.12f,.13f,.15f);
                    else if (key.Contains("eye")) tint = new Color(.28f,.30f,.31f);
                    else tint = new Color(.79f,.58f,.45f);
                    fallback++;
                }
                else textured++;
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", tint);
                EditorUtility.SetDirty(mat);
                assigned[i] = mat;
            }
            Undo.RecordObject(renderer, "Restaurar materiais Remy");
            renderer.sharedMaterials = assigned;
            renderer.enabled = true;
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Remy: materiais com textura: " + textured + "; sem textura (cores temporarias): " + fallback);
        EditorUtility.DisplayDialog("Remy", "Materiais atualizados. Texturas encontradas: " + textured + ". Materiais sem imagem original: " + fallback + ". Se ainda faltar realismo, importe as texturas originais do Remy.", "OK");
    }
}
#endif
