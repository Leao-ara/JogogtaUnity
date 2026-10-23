using System.Collections;
using UnityEngine;
namespace IcoRP
{
    // Optional animated door for the original Icó test cars.
    public class IcoCarDoor : MonoBehaviour
    {
        public Transform doorPivot;
        public float openAngle = -65f;
        public float duration = .35f;
        Coroutine active;
        public void SetOpen(bool open)
        {
            if(!doorPivot)return;
            if(active!=null)StopCoroutine(active);
            active=StartCoroutine(Animate(open));
        }
        IEnumerator Animate(bool open)
        {
            Quaternion start=doorPivot.localRotation;
            Quaternion end=Quaternion.Euler(0,open?openAngle:0,0);
            float t=0;
            while(t<duration)
            {
                t+=Time.deltaTime;
                doorPivot.localRotation=Quaternion.Slerp(start,end,Mathf.Clamp01(t/duration));
                yield return null;
            }
            doorPivot.localRotation=end;
            active=null;
        }
    }
}
