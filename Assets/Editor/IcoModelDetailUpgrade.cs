#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using IcoRP;

public static class IcoModelDetailUpgrade
{
    [MenuItem("Icó RP/Ferramentas anteriores/Melhorar personagem e 3 carros")]
    public static void Upgrade()
    {
        var scene=SceneManager.GetActiveScene();
        if(!scene.IsValid()||scene.name!="Ico_Prototipo_3D")
        {
            EditorUtility.DisplayDialog("Icó RP","Abra a cena Ico_Prototipo_3D primeiro.","OK");return;
        }
        var player=GameObject.Find("ICO_JOGADOR_TESTE");
        var cars=GameObject.Find("ICO_VEICULOS_TESTE");
        if(!player||!cars)
        {
            EditorUtility.DisplayDialog("Icó RP","Primeiro use os comandos Colocar personagem e Adicionar carros.","OK");return;
        }
        UpgradePlayer(player.transform);
        int i=0;
        foreach(Transform car in cars.transform) if(car.GetComponent<IcoVehicle>()) UpgradeCar(car,i++);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Icó RP: visuais dos carros e personagem atualizados sem duplicar objetos.");
    }
    static Material Mat(string name,Color color,float metallic=0,float smooth=.3f)
    {
        if(!AssetDatabase.IsValidFolder("Assets/Materials"))AssetDatabase.CreateFolder("Assets","Materials");
        string path="Assets/Materials/Ico_"+name+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m)
        {
            Shader shader=Shader.Find("Universal Render Pipeline/Lit");
            if(!shader)shader=Shader.Find("Standard");
            m=new Material(shader);
            m.name="Ico_"+name;
            AssetDatabase.CreateAsset(m,path);
        }
        m.color=color;
        if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",color);
        if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metallic);
        if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",smooth);
        if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",smooth);
        EditorUtility.SetDirty(m);
        return m;
    }
    static GameObject Part(Transform parent,PrimitiveType type,string name,Vector3 p,Vector3 s,Material mat,Vector3? angles=null)
    {
        var o=GameObject.CreatePrimitive(type);o.name=name;
        o.transform.SetParent(parent,false);
        o.transform.localPosition=p;o.transform.localScale=s;
        if(angles.HasValue)o.transform.localEulerAngles=angles.Value;
        o.GetComponent<Renderer>().sharedMaterial=mat;
        Object.DestroyImmediate(o.GetComponent<Collider>());
        return o;
    }
    static void Clear(Transform parent)
    {
        for(int i=parent.childCount-1;i>=0;i--)Object.DestroyImmediate(parent.GetChild(i).gameObject);
    }
    static void UpgradePlayer(Transform player)
    {
        var old=player.Find("Corpo provisório");
        if(old)Object.DestroyImmediate(old.gameObject);
        var visual=player.Find("ICO_MODELO_PERSONAGEM");
        if(!visual){var g=new GameObject("ICO_MODELO_PERSONAGEM");g.transform.SetParent(player,false);visual=g.transform;}
        Clear(visual);
        if(!player.GetComponent<IcoWalkAnimation>()) Undo.AddComponent<IcoWalkAnimation>(player.gameObject);
        var skin=Mat("Pele",new Color(.55f,.34f,.22f));
        var shirt=Mat("Camisa_Azul",new Color(.09f,.24f,.42f));
        var pants=Mat("Calca_Jeans",new Color(.07f,.12f,.21f));
        var shoes=Mat("Tenis",new Color(.09f,.09f,.10f));
        var hair=Mat("Cabelo",new Color(.075f,.045f,.035f));
        Part(visual,PrimitiveType.Capsule,"Tronco",new Vector3(0,1.24f,0),new Vector3(.46f,.36f,.28f),shirt);
        Part(visual,PrimitiveType.Sphere,"Cabeca",new Vector3(0,1.69f,0),new Vector3(.28f,.33f,.28f),skin);
        Part(visual,PrimitiveType.Sphere,"Cabelo",new Vector3(0,1.87f,-.015f),new Vector3(.285f,.12f,.285f),hair);
        for(int side=-1;side<=1;side+=2)
        {
            Part(visual,PrimitiveType.Capsule,side<0?"Braco esquerdo":"Braco direito",new Vector3(side*.35f,1.18f,0),new Vector3(.18f,.38f,.18f),shirt,new Vector3(0,0,side*12));
            Part(visual,PrimitiveType.Sphere,"Mao",new Vector3(side*.43f,.83f,0),new Vector3(.15f,.17f,.15f),skin);
            Part(visual,PrimitiveType.Capsule,side<0?"Perna esquerda":"Perna direita",new Vector3(side*.15f,.52f,0),new Vector3(.22f,.48f,.22f),pants);
            Part(visual,PrimitiveType.Cube,"Tenis",new Vector3(side*.15f,.10f,.11f),new Vector3(.25f,.18f,.42f),shoes);
        }
        // A real rigged humanoid and walking animations are a separate later step.
    }
    static void UpgradeCar(Transform car,int index)
    {
        var visual=car.Find("ICO_MODELO_CARRO");
        if(!visual){var g=new GameObject("ICO_MODELO_CARRO");g.transform.SetParent(car,false);visual=g.transform;}
        Clear(visual);
        for(int k=car.childCount-1;k>=0;k--)
        {
            var c=car.GetChild(k);
            if(c!=visual&&(c.name=="Carroceria provisoria"||c.name=="Cabine"||c.name=="Roda"))
                Object.DestroyImmediate(c.gameObject);
        }
        Color[] colors={new Color(.57f,.055f,.045f),new Color(.055f,.17f,.35f),new Color(.16f,.17f,.18f)};
        var paint=Mat("Carro_Pintura_"+index,colors[index%3],.65f,.75f);
        var glass=Mat("Vidro_Fume",new Color(.035f,.075f,.10f),.3f,.82f);
        var rubber=Mat("Borracha_Pneu",new Color(.025f,.027f,.029f),0,.12f);
        var alloy=Mat("Roda_Liga",new Color(.42f,.44f,.46f),.75f,.65f);
        var lights=Mat("Farol",new Color(.85f,.88f,.79f),.15f,.65f);
        var tail=Mat("Lanterna",new Color(.65f,.035f,.025f),.15f,.55f);
        var black=Mat("Acabamento_Preto",new Color(.045f,.048f,.052f),.15f,.35f);
        float length=index==1?4.55f:index==2?4.75f:4.15f;
        Part(visual,PrimitiveType.Cube,"Chassi",new Vector3(0,.79f,0),new Vector3(1.94f,.56f,length),paint);
        Part(visual,PrimitiveType.Cube,"Capo",new Vector3(0,1.11f,1.20f),new Vector3(1.87f,.12f,1.38f),paint);
        Part(visual,PrimitiveType.Cube,"Porta malas",new Vector3(0,1.09f,-1.58f),new Vector3(1.88f,.14f,.72f),paint);
        Part(visual,PrimitiveType.Cube,"Cabine envidracada",new Vector3(0,1.51f,-.32f),new Vector3(1.65f,.62f,2.06f),glass);
        Part(visual,PrimitiveType.Cube,"Teto",new Vector3(0,1.87f,-.34f),new Vector3(1.70f,.12f,1.74f),paint);
        Part(visual,PrimitiveType.Cube,"Parachoque dianteiro",new Vector3(0,.67f,length*.5f+.04f),new Vector3(1.91f,.22f,.19f),black);
        Part(visual,PrimitiveType.Cube,"Parachoque traseiro",new Vector3(0,.67f,-length*.5f-.04f),new Vector3(1.91f,.22f,.19f),black);
        Part(visual,PrimitiveType.Cube,"Grade frontal",new Vector3(0,.91f,length*.5f+.015f),new Vector3(.72f,.19f,.04f),black);
        // Driver door uses a hinge pivot; do not replace the vehicle Rigidbody.
        var hinge=new GameObject("Porta motorista - dobradica").transform;
        hinge.SetParent(visual,false);
        hinge.localPosition=new Vector3(-.96f,1.17f,.64f);
        Part(hinge,PrimitiveType.Cube,"Porta motorista",new Vector3(0,0,-.49f),
            new Vector3(.08f,.64f,.96f),paint);
        var door=car.GetComponent<IcoCarDoor>();
        if(!door)door=Undo.AddComponent<IcoCarDoor>(car.gameObject);
        door.doorPivot=hinge;
        door.openAngle=67f;
        for(int side=-1;side<=1;side+=2)
        {
            Part(visual,PrimitiveType.Cube,"Farol dianteiro",new Vector3(side*.71f,1.04f,length*.5f+.03f),new Vector3(.36f,.19f,.06f),lights);
            Part(visual,PrimitiveType.Cube,"Lanterna traseira",new Vector3(side*.72f,1.01f,-length*.5f-.03f),new Vector3(.32f,.22f,.06f),tail);
            Part(visual,PrimitiveType.Cube,"Espelho lateral",new Vector3(side*.99f,1.42f,.45f),new Vector3(.20f,.16f,.31f),black);
            Part(visual,PrimitiveType.Cube,"Macaneta dianteira",new Vector3(side*.97f,1.16f,.11f),new Vector3(.035f,.06f,.24f),alloy);
            Part(visual,PrimitiveType.Cube,"Macaneta traseira",new Vector3(side*.97f,1.16f,-.96f),new Vector3(.035f,.06f,.24f),alloy);
            for(int end=-1;end<=1;end+=2)
            {
                float z=end*1.34f;
                Part(visual,PrimitiveType.Cylinder,"Pneu",new Vector3(side*1.04f,.43f,z),
                    new Vector3(.68f,.20f,.68f),rubber,new Vector3(0,0,90));
                Part(visual,PrimitiveType.Cylinder,"Aro",new Vector3(side*1.17f,.43f,z),
                    new Vector3(.42f,.21f,.42f),alloy,new Vector3(0,0,90));
            }
        }
    }
}
#endif
