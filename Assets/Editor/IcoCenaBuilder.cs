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
 static Transform root; static Plan plan; static Material asphalt, water, grass, walls, roofs, glass, park, dirt, floor, bridge, sidewalk, stripe, curb, trunk, foliage;
 static System.Random rng;
 static Vector3 W(Vector2 p) {return new Vector3((p.x-464)*plan.metersPerPixel,0,(825-p.y)*plan.metersPerPixel);}
 static Material Mat(string name, Color c) {var s=Shader.Find("Universal Render Pipeline/Lit"); if(s==null)s=Shader.Find("Standard"); if(s==null)s=Shader.Find("Diffuse"); var m=new Material(s);m.name=name;m.color=c;return m;}
 static GameObject Box(string name,Vector3 p,Vector3 scale,Material m,Transform parent=null) {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent==null?root:parent);g.transform.position=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;return g;}
 static void Segment(string name,Vector3 a,Vector3 b,float width,Material m,float height=.13f,float elevation=.06f) {var d=b-a;if(d.sqrMagnitude<.01f)return;var g=Box(name,(a+b)/2+Vector3.up*elevation,new Vector3(width,height,d.magnitude+.2f),m);g.transform.rotation=Quaternion.LookRotation(d.normalized,Vector3.up);}
 static void Path(string name,Vector2[] pts,float width,Material m,float elevation=.07f) {if(pts==null)return;for(int i=1;i<pts.Length;i++)Segment(name,W(pts[i-1]),W(pts[i]),width,m,.15f,elevation);}
 static void RoadDetail(Road road) {
   if(road.points==null)return;
   for(int i=1;i<road.points.Length;i++){
    Vector3 a=W(road.points[i-1]),b=W(road.points[i]); Vector3 d=b-a;
    if(d.sqrMagnitude<1f)continue;
    Vector3 right=Vector3.Cross(Vector3.up,d.normalized);
    float edge=road.widthMeters*.5f+1.35f;
    Segment("Calcada esquerda "+road.name,a-right*edge,b-right*edge,2.1f,sidewalk,.20f,.18f);
    Segment("Calcada direita "+road.name,a+right*edge,b+right*edge,2.1f,sidewalk,.20f,.18f);
    Segment("Meio-fio esquerdo",a-right*(road.widthMeters*.5f+.15f),b-right*(road.widthMeters*.5f+.15f),.22f,curb,.20f,.22f);
    Segment("Meio-fio direito",a+right*(road.widthMeters*.5f+.15f),b+right*(road.widthMeters*.5f+.15f),.22f,curb,.20f,.22f);
    float length=d.magnitude;
    for(float t=4;t<length-4;t+=11){
      Vector3 start=a+d.normalized*t,end=a+d.normalized*Mathf.Min(t+4,length-2);
      if(road.widthMeters>=12)Segment("Faixa central tracejada",start,end,.18f,stripe,.025f,.225f);
    }
   }
 }
 static void BridgeDetails(Vector2 a,Vector2 b,float width) {
   Vector3 start=W(a),end=W(b),dir=(end-start).normalized,right=Vector3.Cross(Vector3.up,dir);
   Segment("Tabuleiro da ponte",start,end,width,bridge,.55f,1.35f);
   for(int side=-1;side<=1;side+=2){
    Vector3 offset=right*(width*.5f+.45f)*side;
    Segment("Guarda-corpo da ponte",start+offset+Vector3.up*2.1f,end+offset+Vector3.up*2.1f,.35f,curb,.5f,1.35f);
    for(float t=0;t<(end-start).magnitude;t+=7){
      Vector3 p=start+dir*t+offset;
      Box("Pilar guarda-corpo",p+Vector3.up*2.05f,new Vector3(.35f,1.5f,.35f),curb);
    }
   }
 }
 static void Tree(Vector3 x,float size) {
   Box("Tronco",x+Vector3.up*size*.45f,new Vector3(.5f*size/5f,size*.9f,.5f*size/5f),trunk);
   var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name="Copa";g.transform.SetParent(root);
   g.transform.position=x+Vector3.up*size*1.25f;g.transform.localScale=Vector3.one*size;
   g.GetComponent<Renderer>().sharedMaterial=foliage;
 }
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
   // Removable roof object makes interior inspection and future interaction straightforward.
   Box("Telhado removivel",c+new Vector3(0,h+.25f,0),new Vector3(sx+.65f,.45f,sz+.65f),roofs,t);
   Box("Porta batente esquerdo",c+new Vector3(-opening*.5f,1.2f,-sz/2-.17f),new Vector3(.12f,2.4f,.16f),dirt,t);
   Box("Porta batente direito",c+new Vector3(opening*.5f,1.2f,-sz/2-.17f),new Vector3(.12f,2.4f,.16f),dirt,t);
   Box("Piso interno claro",c+Vector3.up*.32f,new Vector3(sx-.65f,.03f,sz-.65f),floor,t);
   Box("Janela lateral",c+new Vector3(-sx/2-.16f,2.15f,sz*.12f),new Vector3(.09f,1f,1.65f),glass,t);
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
  sidewalk=Mat("Calcada concreto",new Color(.56f,.55f,.52f));stripe=Mat("Pintura viaria amarela",new Color(.96f,.8f,.25f));curb=Mat("Meio fio",new Color(.83f,.81f,.73f));trunk=Mat("Madeira",new Color(.29f,.19f,.12f));foliage=Mat("Folhagem",new Color(.14f,.37f,.16f));
  Box("Terreno",new Vector3(0,-.65f,0),new Vector3(1900,1.2f,3450),grass);
  if(plan.river!=null)Path("Rio Ico",plan.river.points,plan.river.widthMeters,water,.14f);
  foreach(var road in plan.roads){Path("Via - "+road.name,road.points,road.widthMeters,asphalt,.12f);RoadDetail(road);}
  if(plan.roundabouts!=null)foreach(var r in plan.roundabouts){Vector3 c=W(r.center);for(int i=0;i<32;i++){float a=i*Mathf.PI*2/32,b=(i+1)*Mathf.PI*2/32;Segment("Rotatoria",c+new Vector3(Mathf.Cos(a)*r.radiusMeters,0,Mathf.Sin(a)*r.radiusMeters),c+new Vector3(Mathf.Cos(b)*r.radiusMeters,0,Mathf.Sin(b)*r.radiusMeters),9,asphalt,.18f,.14f);}}
  if(plan.areas!=null)foreach(var area in plan.areas){var p=area.polygon;Vector2 avg=Vector2.zero;foreach(var v in p)avg+=v;avg/=p.Length;var center=W(avg);if(area.name.ToLower().Contains("praca")) {Box("Praca Grande - centro",center+Vector3.up*.08f,new Vector3(160,.16f,150),park);for(int k=0;k<45;k++){var x=center+new Vector3(rng.Next(-75,75),0,rng.Next(-68,68));Tree(x,4.5f);}}else{Box("Area Delta - referencia",center+Vector3.up*.04f,new Vector3(135,.08f,95),dirt);}}
  // Raised bridge above the river crossing; keep approximate map position.
  BridgeDetails(new Vector2(650,612),new Vector2(615,867),18);
  int count=0;var placed=new List<Vector2>();for(int attempt=0;attempt<7000&&count<240;attempt++){Vector2 p=new Vector2(rng.Next(35,906),rng.Next(55,1600));if(!ValidLot(p))continue;bool near=false;foreach(var prev in placed)if(Vector2.Distance(prev,p)<21){near=true;break;}if(near)continue;placed.Add(p);House(p,++count);}
  // Low-density roadside vegetation, away from river, roads, park and institutional Delta.
  for(int i=0,tries=0;i<450&&tries<6000;tries++){
   Vector2 p=new Vector2(rng.Next(30,910),rng.Next(30,1620));
   if(!ValidLot(p))continue;
   Tree(W(p),3.2f+(float)rng.NextDouble()*3.4f);i++;
  }
  var light=new GameObject("Sol").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.25f;light.transform.rotation=Quaternion.Euler(48,-30,0);
  var cam=new GameObject("Camera Aerea").AddComponent<Camera>();cam.transform.position=new Vector3(0,1600,-1100);cam.transform.rotation=Quaternion.LookRotation(-cam.transform.position+Vector3.zero);cam.farClipPlane=6000;cam.fieldOfView=60;
  Directory.CreateDirectory("Assets/IcoCenas");EditorSceneManager.SaveScene(scene,"Assets/IcoCenas/Ico_Prototipo_3D.unity");AssetDatabase.SaveAssets();
  EditorUtility.DisplayDialog("Icó RP","Cena criada e salva em Assets/IcoCenas/Ico_Prototipo_3D.unity. Casas com entrada aberta: "+count+". Protótipo, não é reprodução geográfica exata.","OK");
 }
}
#endif
