using UnityEngine;

namespace PrototipoRP
{
    [RequireComponent(typeof(CharacterController))]
    public class MovimentoJogador : MonoBehaviour
    {
        [SerializeField] private float velocidade = 5f;
        [SerializeField] private float gravidade = -20f;

        private CharacterController controlador;
        private float velocidadeVertical;

        private void Awake()
        {
            controlador = GetComponent<CharacterController>();
        }

        private void Update()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 direcao = new Vector3(horizontal, 0f, vertical).normalized;

            if (controlador.isGrounded && velocidadeVertical < 0f)
                velocidadeVertical = -2f;

            velocidadeVertical += gravidade * Time.deltaTime;
            Vector3 movimento = direcao * velocidade;
            movimento.y = velocidadeVertical;
            controlador.Move(movimento * Time.deltaTime);
        }
    }
}
