using UnityEngine;

namespace PrototipoRP
{
    public class CameraSeguidora : MonoBehaviour
    {
        [SerializeField] private Transform alvo;
        [SerializeField] private Vector3 deslocamento = new Vector3(0f, 4f, -7f);
        [SerializeField] private float suavidade = 8f;

        private void LateUpdate()
        {
            if (alvo == null) return;

            Vector3 destino = alvo.position + deslocamento;
            transform.position = Vector3.Lerp(transform.position, destino, suavidade * Time.deltaTime);
            transform.LookAt(alvo.position + Vector3.up);
        }
    }
}
