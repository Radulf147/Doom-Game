using UnityEngine;

[RequireComponent(typeof(CharacterController))] // Ou Rigidbody, se estiver usando
public class SpriteCharacterMovement3D : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -20f;
    public float jumpHeight = 1.5f;
    // Para rota��o (se n�o for apenas billboarding, ou para rota��o de input)
    // public float rotationSpeed = 700f;

    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool isGrounded;

    // Refer�ncia para o Animator (se voc� tiver anima��es de sprite sheet)
    // private Animator animator;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // animator = GetComponent<Animator>(); // Se tiver um Animator no mesmo objeto
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; // Mant�m no ch�o
        }

        float horizontalInput = Input.GetAxis("Horizontal"); // A/D ou Setas
        float verticalInput = Input.GetAxis("Vertical");     // W/S ou Setas

        // Calcula a dire��o do movimento baseada na orienta��o da c�mera
        // Isso permite que "frente" seja sempre para onde a c�mera aponta.
        // Se o seu billboarding j� faz o sprite virar com a c�mera, o input local pode ser mais simples.
        // No entanto, para movimento em um mundo 3D, geralmente voc� quer que W mova "para frente" no mundo.

        Vector3 moveDirectionInput = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        forward.y = 0; // Manter o movimento no plano horizontal
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 desiredMoveDirection = forward * verticalInput + right * horizontalInput;

        controller.Move(desiredMoveDirection.normalized * moveSpeed * Time.deltaTime);

        // Pulo
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravidade
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        // Anima��o (exemplo b�sico)
        // if (animator != null)
        // {
        //    float speed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
        //    animator.SetFloat("Speed", speed);
        //    animator.SetBool("IsGrounded", isGrounded);
        // }
    }
}