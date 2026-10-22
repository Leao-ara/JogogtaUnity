#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;

public static class IcoCenaBuilder {
 [Serializable] class Road {public string name; public float widthMeters; public Vector2[] points;}
 [Serializable] class River {public string name; public float widthMeters; public Vector2[] points;}
 [Serializable] class Roundabout {public Vector2 center; public float radiusMeters;}
 [Serializable] class Area {public string name; public Vector2[] polygon;}
 [Serializable] class Plan {public float metersPerPixel; public Road[] roads; public River river; public Roundabout[] roundabouts; public Area[] areas;}
 static Transform root; static Plan plan; static Material asphalt, water, grass, walls, roofs, glass, park, dirt, floor, bridge;
 static System.Random rng;
 static Vector3 W(Vector2 p) {return new Vector3((p.x-464)*plan.metersPerPixel,0,(825-p.y)*plan.metersPerPixel);}
 static Material Mat(string name, Color c) {var s=Shader.Find("Universal Render Pipeline/Lit"); if(s==null)s=Shader.Find("Standard"); if(s==null)s=Shader.Find("Diffuse"); var m=new Material(s);m.name=name;m.color=c;return m;}
 static GameObject Box(string name,Vector3 p,Vector3 scale,Material m,Transform parent=null) {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent==null?root:parent);g.transform.position=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;return g;}
 static void Segment(string name,Vector3 a,Vector3 b,float width,Material m,float height=.13f,float elevation=.06f) {var d=b-a;if(d.sqrMagnitude<.01f)return;var g=Box(name,(a+b)/2+Vector3.up*elevation,new Vector3(width,height,d.magnitude+.2f),m);g.transform.rotation=Quaternion.LookRotation(d.normalized,Vector3.up);}
 static void Path(string name,Vector2[] pts,float width,Material m,float elevation=.07f) {if(pts==null)return;for(int i=1;i<pts.Length;i++)Segment(name,W(pts[i-1]),W(pts[i]),width,m,.15f,elevation);}
 static bool Inside(Vector2 p,Vector2[] poly) {bool inside=false;for(int i=0,j=poly.Length-1;i<poly.Length;j=i++){Vector2 a=poly[i],b=poly[j];if((a.y>p.y)!=(b.y>p.y)&&p.x<(b.x-a.x)*(p.y-a.y)/(b.y-a.y)+a.x)inside=!inside;}return inside;}
 static float DistSeg(Vector2 p,Vector2 a,Vector2 b) {var v=b-a;float t=Mathf.Clamp01(Vector2.Dot(p-a,v)/Mathf.Max(v.sqrMagnitude,.0001f));return Vector2.Distance(p,a+v*t);}
 static bool ValidLot(Vector2 p) {if(p.x<24||p.x>916||p.y<45||p.y>1615)return false;
   if(plan.river!=null)for(int i=1;i<plan.river.points.Length;i++)if(DistSeg(p,plan.river.points[i-1],plan.river.points[i])<(plan.river.widthMeters/2+38)/plan.metersPerPixel)return false;
   foreach(var road in plan.roads)for(int i=1;i<road.points.Length;i++)if(DistSeg(p,road.points[i-1],road.points[i])<(road.widthMeters/2+17)/plan.metersPerPixel)return false;
   foreach(var a in plan.areas)if(Inside(p,a.polygon))return false;
   foreach(var r in plan.roundabouts)if(Vector2.Distance(p,r.center)<(r.radiusMeters+22)/plan.metersPerPixel)return false;
   return true;
 }
 static void House(Vector2 pos,int n) {Vector3 c=W(pos);float sx=10+rng.Next(0,9),sz=11+rng.Next(0,10),h=3.4f;var group=new GameObject("Casa_Visitavel_"+n.ToString("000"));group.transform.SetParent(root);group.transform.position=c;var t=group.transform;
   var wallMat=Mat("Fachada casa "+n,Color.Lerp(new Color(.79f,.69f,.56f),new Color(.89f,.86f,.77f),(float)rng.NextDouble()));
   Box("Piso",c+Vector3.up*.15f,new Vector3(sx,.3f,sz),floor,t);
   float thick=.28f;Box("Parede esquerda",c+new Vector3(-sx/2,h/2,0),new Vector3(thick,h,sz),wallMat,t);
   Box("Parede direita",c+new Vector3(sx/2,h/2,0),new Vector3(thick,h,sz),wallMat,t);
   Box("Parede fundos",c+new Vector3(0,h/2,sz/2),new Vector3(sx,h,thick),wallMat,t);
   // Front door opening: 1.7m wide, 2.4m high. No solid wall across doorway.
   float opening=1.7f, side=(sx-opening)/2;
   Box("Fachada esquerda",c+new Vector3(-(opening+side)/2,h/2,-sz/2),new Vector3(side,h,thick),wallMat,t);
   Box("Fachada direita",c+new Vector3((opening+side)/2,h/2,-sz/2),new Vector3(side,h,thick),wallMat,t);
   Box("Vergalhao acima porta",c+new Vector3(0,(h+2.4f)/2,-sz/2),new Vector3(opening,h-2.4f,thick),wallMat,t);
   // Interior partitions leave a passage at the front.
   Box("Divisoria interna",c+new Vector3(-sx*.12f,h/2,sz*.12f),new Vector3(sx*.65f,h,thick),wallMat,t);
   Box("Teto",c+new Vector3(0,h+.16f,0),new Vector3(sx+.5f,.3f,sz+.5f),roofs,t);
   Box("Janela frontal decorativa",c+new Vector3(sx*.28f,2.1f,-sz/2-.17f),new Vector3(1.35f,.85f,.08f),glass,t);
   // Open front entrance. House interiors are physically traversable; doors are not yet interactive.
 }
 [MenuItem("Icó RP/Criar cena 3D completa (protótipo)")]
 public static void Build() {
  var asset=Resources.Load<TextAsset>("IcoPlanta");if(asset==null){EditorUtility.DisplayDialog("Icó RP","Falta Assets/Resources/IcoPlanta.json","OK");return;}
  plan=JsonUtility.FromJson<Plan>(asset.text);if(plan==null||plan.roads==null){Debug.LogError("Planta inválida");return;}
  rng=new System.Random(116);var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  root=new GameObject("ICO_CIDADE_3D").transform;
  asphalt=Mat("Asfalto",new Color(.19f,.2f,.21f));water=Mat("Agua",new Color(.05f,.33f,.48f));grass=Mat("Grama",new Color(.3f,.48f,.23f));walls=Mat("Paredes",Color.white);roofs=Mat("Telhado",new Color(.58f,.23f,.16f));glass=Mat("Vidro",new Color(.25f,.55f,.7f));park=Mat("Praca",new Color(.33f,.52f,.28f));dirt=Mat("Terra",new Color(.65f,.53f,.35f));floor=Mat("Piso",new Color(.72f,.67f,.59f));bridge=Mat("Concreto ponte",new Color(.48f,.48f,.46f));
  Box("Terreno",new Vector3(0,-.65f,0),new Vector3(1900,1.2f,3450),grass);
  if(plan.river!=null)Path("Rio Ico",plan.river.points,plan.river.widthMeters,water,.14f);
  foreach(var road in plan.roads)Path("Via - "+road.name,road.points,road.widthMeters,asphalt,.12f);
  if(plan.roundabouts!=null)foreach(var r in plan.roundabouts){Vector3 c=W(r.center);for(int i=0;i<32;i++){float a=i*Mathf.PI*2/32,b=(i+1)*Mathf.PI*2/32;Segment("Rotatoria",c+new Vector3(Mathf.Cos(a)*r.radiusMeters,0,Mathf.Sin(a)*r.radiusMeters),c+new Vector3(Mathf.Cos(b)*r.radiusMeters,0,Mathf.Sin(b)*r.radiusMeters),9,asphalt,.18f,.14f);}}
  if(plan.areas!=null)foreach(var area in plan.areas){var p=area.polygon;Vector2 avg=Vector2.zero;foreach(var v in p)avg+=v;avg/=p.Length;var center=W(avg);if(area.name.ToLower().Contains("praca")) {Box("Praca Grande - centro",center+Vector3.up*.08f,new Vector3(160,.16f,150),park);for(int k=0;k<45;k++){var x=center+new Vector3(rng.Next(-75,75),0,rng.Next(-68,68));Box("Arvore tronco",x+Vector3.up*2,new Vector3(.7f,4,.7f),dirt);var crown=GameObject.CreatePrimitive(PrimitiveType.Sphere);crown.name="Copa arvore";crown.transform.SetParent(root);crown.transform.position=x+Vector3.up*5;crown.transform.localScale=new Vector3(5,5,5);crown.GetComponent<Renderer>().sharedMaterial=grass;}}else{Box("Area Delta - referencia",center+Vector3.up*.04f,new Vector3(135,.08f,95),dirt);}}
  // Raised bridge above the river crossing; keep approximate map position.
  Segment("Ponte elevada",W(new Vector2(650,610)),W(new Vector2(620,864)),18,bridge,.55f,1.4f);
  int count=0;var placed=new List<Vector2>();for(int attempt=0;attempt<7000&&count<240;attempt++){Vector2 p=new Vector2(rng.Next(35,906),rng.Next(55,1600));if(!ValidLot(p))continue;bool near=false;foreach(var prev in placed)if(Vector2.Distance(prev,p)<21){near=true;break;}if(near)continue;placed.Add(p);House(p,++count);}
  var light=new GameObject("Sol").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.25f;light.transform.rotation=Quaternion.Euler(48,-30,0);
  var cam=new GameObject("Camera Aerea").AddComponent<Camera>();cam.transform.position=new Vector3(0,1600,-1100);cam.transform.rotation=Quaternion.LookRotation(-cam.transform.position+Vector3.zero);cam.farClipPlane=6000;cam.fieldOfView=60;
  Directory.CreateDirectory("Assets/IcoCenas");EditorSceneManager.SaveScene(scene,"Assets/IcoCenas/Ico_Prototipo_3D.unity");AssetDatabase.SaveAssets();
  EditorUtility.DisplayDialog("Icó RP","Cena criada e salva em Assets/IcoCenas/Ico_Prototipo_3D.unity. Casas com entrada aberta: "+count+". Protótipo, não é reprodução geográfica exata.","OK");
 }
}
#endif
