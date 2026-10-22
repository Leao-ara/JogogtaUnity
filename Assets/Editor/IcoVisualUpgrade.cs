#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// Visual pass for the existing Icó scene. Does not rebuild or reposition the city.
public static class IcoVisualUpgrade
{
    [MenuItem("Icó RP/Melhorar visual da cena atual")]
    public static void Upgrade()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded) return;
        if (GameObject.Find("ICO_CIDADE_3D") == null)
        {
            EditorUtility.DisplayDialog("Icó RP", "Abra a cena Ico_Prototipo_3D antes de melhorar o visual.", "OK");
            return;
        }
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        var asphalt = MaterialFor("Ico_Asfalto_Urbano", new Color(.115f,.123f,.13f));
        var sidewalk = MaterialFor("Ico_Calcada_Concreto", new Color(.56f,.55f,.52f));
        var grass = MaterialFor("Ico_Solo_Natural", new Color(.29f,.35f,.25f));
        var bark = MaterialFor("Ico_Tronco", new Color(.27f,.19f,.13f));
        var leaves = new [] {
            MaterialFor("Ico_Folhas_01",new Color(.12f,.31f,.15f)),
            MaterialFor("Ico_Folhas_02",new Color(.17f,.37f,.18f)),
            MaterialFor("Ico_Folhas_03",new Color(.22f,.40f,.19f))
        };
        var renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        var crowns = new List<Transform>();
        foreach(var r in renderers)
        {
            var n = r.gameObject.name;
            if (n.StartsWith("Via - ") || n == "Tabuleiro da ponte") r.sharedMaterial = asphalt;
            else if (n.StartsWith("Calcada ")) r.sharedMaterial = sidewalk;
            else if (n == "Terreno") r.sharedMaterial = grass;
            else if (n == "Tronco") r.sharedMaterial = bark;
            else if (n == "Copa") crowns.Add(r.transform);
        }
        // Extra overlapping ellipsoids give each tree a less spherical silhouette.
        // Decorative geometry is collider-free, and generated only once.
        var holder = GameObject.Find("ICO_MELHORIAS_VISUAIS");
        if (holder == null)
        {
            holder = new GameObject("ICO_MELHORIAS_VISUAIS");
            Undo.RegisterCreatedObjectUndo(holder, "Icó visual");
            int index = 0;
            foreach(var crown in crowns)
            {
                if (crown == null) continue;
                float s = crown.lossyScale.x;
                var p = crown.position;
                for(int i=0;i<3;i++)
                {
                    float a = (i*2.0944f) + (index%7)*.27f;
                    var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Undo.RegisterCreatedObjectUndo(g, "Copa detalhada");
                    g.name = "Folhagem secundária";
                    g.transform.SetParent(holder.transform);
                    g.transform.position = p + new Vector3(Mathf.Cos(a)*s*.28f, s*(i==1?.16f:-.08f), Mathf.Sin(a)*s*.28f);
                    g.transform.localScale = new Vector3(s*.68f,s*.55f,s*.67f);
                    g.GetComponent<Renderer>().sharedMaterial = leaves[(index+i)%leaves.Length];
                    Object.DestroyImmediate(g.GetComponent<Collider>());
                }
                index++;
            }
        }
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Icó RP: visual atualizado na cena atual. Salve/commite a cena no GitHub Desktop.");
    }
    static Material MaterialFor(string name, Color color)
    {
        const string folder = "Assets/Materials";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets","Materials");
        string path = folder+"/"+name+".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            m = new Material(shader);
            m.name = name;
            m.color = color;
            AssetDatabase.CreateAsset(m,path);
        }
        return m;
    }
}
#endif
