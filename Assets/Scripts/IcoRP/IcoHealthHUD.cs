using UnityEngine;
namespace IcoRP
{
    // Lightweight HUD for the city test scene, no GTA UI assets required.
    public class IcoHealthHUD : MonoBehaviour
    {
        public IcoCharacterHealth health;
        GUIStyle labelStyle;
        Texture2D white;
        void Awake()
        {
            if(!health)health=GetComponent<IcoCharacterHealth>();
            white=Texture2D.whiteTexture;
        }
        void OnGUI()
        {
            if(!health)return;
            float scale=Mathf.Clamp(Screen.width/1280f,.75f,1.6f);
            float x=20*scale,y=Screen.height-62*scale,w=230*scale,h=19*scale;
            Draw(new Rect(x-3*scale,y-27*scale,w+6*scale,h+34*scale),new Color(0,0,0,.67f));
            Draw(new Rect(x,y,w,h),new Color(.2f,.2f,.2f,1));
            Draw(new Rect(x,y,w*Mathf.Clamp01(health.currentHealth/Mathf.Max(1,health.maxHealth)),h),new Color(.77f,.12f,.13f,1));
            if(labelStyle==null){labelStyle=new GUIStyle(GUI.skin.label);labelStyle.fontStyle=FontStyle.Bold;labelStyle.normal.textColor=Color.white;}
            labelStyle.fontSize=Mathf.RoundToInt(15*scale);
            GUI.Label(new Rect(x,y-26*scale,w,25*scale),"VIDA  "+Mathf.CeilToInt(health.currentHealth)+" / "+Mathf.CeilToInt(health.maxHealth),labelStyle);
        }
        void Draw(Rect rect,Color color)
        {
            var old=GUI.color;GUI.color=color;GUI.DrawTexture(rect,white);GUI.color=old;
        }
    }
}