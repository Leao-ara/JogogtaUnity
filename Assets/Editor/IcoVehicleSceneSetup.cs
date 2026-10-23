#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using IcoRP;

public static class IcoVehicleSceneSetup
{
    [MenuItem("Icó RP/Ferramentas anteriores/Adicionar carros de teste na cidade")]
    public static void Install()
    {
        Scene scene=SceneManager.GetActiveScene();
        if(!scene.IsValid()||scene.name!="Ico_Prototipo_3D")
        {
            EditorUtility.DisplayDialog("Icó RP","Abra Ico_Prototipo_3D primeiro.","OK");
            return;
        }
        Transform root=GameObject.Find("ICO_VEICULOS_TESTE")?.transform;
        if(root==null)
        {
            var g=new GameObject("ICO_VEICULOS_TESTE");
            Undo.RegisterCreatedObjectUndo(g,"Carros de teste");
            root=g.transform;
        }
        if(root.childCount==0)
        {
            Vector3 center=Vector3.zero;
            foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
                if(r.name.StartsWith("Via - ")){center=r.bounds.center;break;}
            for(int i=0;i<3;i++)
            {
                var car=new GameObject("Carro RP "+(i+1));
                Undo.RegisterCreatedObjectUndo(car,"Carro RP");
                car.transform.SetParent(root);
                car.transform.position=center+new Vector3(i*7,3,0);
                var collider=car.AddComponent<BoxCollider>();
                collider.size=new Vector3(2,1.35f,4.1f);
                collider.center=new Vector3(0,.85f,0);
                car.AddComponent<Rigidbody>();
                car.AddComponent<IcoVehicle>();
                var body=GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name="Carroceria provisoria";
                body.transform.SetParent(car.transform,false);
                body.transform.localPosition=new Vector3(0,.85f,0);
                body.transform.localScale=new Vector3(2,1.25f,4);
                Object.DestroyImmediate(body.GetComponent<Collider>());
                var cabin=GameObject.CreatePrimitive(PrimitiveType.Cube);
                cabin.name="Cabine";
                cabin.transform.SetParent(car.transform,false);
                cabin.transform.localPosition=new Vector3(0,1.65f,-.3f);
                cabin.transform.localScale=new Vector3(1.65f,.75f,2.15f);
                Object.DestroyImmediate(cabin.GetComponent<Collider>());
                for(int side=-1;side<=1;side+=2)
                    for(int end=-1;end<=1;end+=2)
                    {
                        var wheel=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        wheel.name="Roda";
                        wheel.transform.SetParent(car.transform,false);
                        wheel.transform.localPosition=new Vector3(side*1.04f,.45f,end*1.28f);
                        wheel.transform.localRotation=Quaternion.Euler(0,0,90);
                        wheel.transform.localScale=new Vector3(.7f,.15f,.7f);
                        Object.DestroyImmediate(wheel.GetComponent<Collider>());
                    }
            }
        }
        var player=GameObject.Find("ICO_JOGADOR_TESTE");
        if(player && !player.GetComponent<IcoVehicleInteraction>())
            Undo.AddComponent<IcoVehicleInteraction>(player);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Icó RP: 3 carros provisórios instalados. E para entrar/sair; WASD para dirigir.");
    }
}
#endif
