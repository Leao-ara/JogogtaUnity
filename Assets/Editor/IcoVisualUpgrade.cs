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
        var trunks = new List<Transform>();
        foreach(var r in renderers)
        {
            var n = r.gameObject.name;
            if (n.StartsWith("Via - ") || n == "Tabuleiro da ponte") r.sharedMaterial = asphalt;
            else if (n.StartsWith("Calcada ")) r.sharedMaterial = sidewalk;
            else if (n == "Terreno") r.sharedMaterial = grass;
            else if (n == "Tronco") { r.sharedMaterial = bark; trunks.Add(r.transform); }
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
            var branches = MaterialFor("Ico_Galhos", new Color(.22f,.15f,.09f));
            var roofEdge = MaterialFor("Ico_Beiral",new Color(.43f,.19f,.13f));
            var steps = MaterialFor("Ico_Degrau",new Color(.63f,.62f,.58f));
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
                for(int j=0;j<4;j++)
                {
                    float angle=j*1.5708f+(index%11)*.21f;
                    AddPrimitive(holder.transform,PrimitiveType.Sphere,"Folhas externas",
                        p+new Vector3(Mathf.Cos(angle)*s*.38f,s*.1f,Mathf.Sin(angle)*s*.38f),
                        new Vector3(s*.46f,s*.35f,s*.44f),leaves[(index+j)%3]);
                }
                index++;
            }
            for(int i=0;i<trunks.Count;i++)
            {
                var t=trunks[i];
                float h=t.lossyScale.y;
                float radius=Mathf.Max(.12f,t.lossyScale.x*.55f);
                Vector3 start=t.position+Vector3.up*h*.16f;
                for(int j=0;j<3;j++)
                {
                    float a=(i%9)*.29f+j*2.094f;
                    Vector3 end=start+new Vector3(Mathf.Cos(a)*h*.4f,h*.38f,Mathf.Sin(a)*h*.4f);
                    var branch=AddPrimitive(holder.transform,PrimitiveType.Cylinder,"Galho inclinado",
                        (start+end)*.5f,new Vector3(radius,(end-start).magnitude*.5f,radius),branches);
                    branch.transform.up=(end-start).normalized;
                }
            }
            foreach(var house in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if(!house.name.StartsWith("Casa_Visitavel_"))continue;
                var roof=house.Find("Telhado removivel");
                if(roof==null)continue;
                Vector3 p=roof.position, size=roof.lossyScale;
                AddPrimitive(holder.transform,PrimitiveType.Cube,"Beiral frontal",
                    p+new Vector3(0,-.12f,-size.z*.5f),
                    new Vector3(size.x+.3f,.2f,.36f),roofEdge);
                AddPrimitive(holder.transform,PrimitiveType.Cube,"Degrau da entrada",
                    house.position+new Vector3(0,.15f,-size.z*.5f-.75f),
                    new Vector3(2.5f,.25f,1.1f),steps);
            }
        }
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Icó RP: visual atualizado na cena atual. Salve/commite a cena no GitHub Desktop.");
    }
    static GameObject AddPrimitive(Transform parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material)
    {
        var g=GameObject.CreatePrimitive(type);
        g.name=name;g.transform.SetParent(parent);g.transform.position=position;
        g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=material;
        Object.DestroyImmediate(g.GetComponent<Collider>());
        return g;
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
