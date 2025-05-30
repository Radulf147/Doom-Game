using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerFPController : MonoBehaviour
{
    public float moveSpeed = 7.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 10000.0f;

    [Header("Mouse Look Settings")]
    public Transform playerCameraTransform; // <<< ADICIONADO: Arraste sua Main Camera (filha deste objeto) aqui
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f; // Limite de rota��o vertical (para n�o dar loop)

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0; // Vari�vel para acumular a rota��o vertical da c�mera

    [HideInInspector]
    public bool canMove = true; // Para travar o movimento se necess�rio (ex: menu)

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Travar cursor no centro da tela e escond�-lo
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Valida��o para garantir que a c�mera foi atribu�da
        if (playerCameraTransform == null)
        {
            // Tenta encontrar a c�mera automaticamente se for filha direta e tiver o componente Camera
            Camera childCam = GetComponentInChildren<Camera>();
            if (childCam != null)
            {
                playerCameraTransform = childCam.transform;
                Debug.LogWarning("PlayerFPController: 'playerCameraTransform' n�o foi atribu�do no Inspector. C�mera filha encontrada e atribu�da automaticamente. Para evitar problemas, � recomendado atribuir manualmente.");
            }
            else
            {
                Debug.LogError("PlayerFPController: 'playerCameraTransform' n�o foi atribu�do no Inspector e nenhuma c�mera filha foi encontrada! A rota��o vertical do mouse n�o funcionar� corretamente. Por favor, atribua a c�mera filha.");
            }
        }
    }

    void Update()
    {
        // --- Movimenta��o ---
        // Verifica se pode se mover antes de processar inputs de movimento
        float curSpeedX = 0;
        float curSpeedY = 0;

        if (canMove)
        {
            // Mant�m a dire��o local do jogador
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);

            curSpeedX = moveSpeed * Input.GetAxis("Vertical");   // W/S para frente/tr�s
            curSpeedY = moveSpeed * Input.GetAxis("Horizontal"); // A/D para esquerda/direita
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        }
        else
        {
            // Zera o movimento se canMove for false, exceto para a gravidade/pulo
            moveDirection.x = 0;
            moveDirection.z = 0;
        }


        float movementDirectionY = moveDirection.y; // Salva a velocidade vertical atual (para pulo/queda)


        // --- Pulo ---
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY; // Mant�m a velocidade vertical (ex: se j� estiver caindo)
        }

        // Aplicar gravidade (apenas se n�o estiver no ch�o)
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Aplicar movimento ao Character Controller
        characterController.Move(moveDirection * Time.deltaTime);

        // --- Rota��o (Mouse Look) ---
        if (canMove && playerCameraTransform != null) // S� rotaciona se puder mover E a c�mera estiver atribu�da
        {
            // Rota��o Vertical (Pitch) - Aplicada � C�MERA FILHA
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCameraTransform.localRotation = Quaternion.Euler(rotationX, 0, 0); // <<< MODIFICADO: Aplica apenas � c�mera

            // Rota��o Horizontal (Yaw) - Aplicada ao CharacterController/Player (este objeto)
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * lookSpeed); // <<< MANTIDO: Gira o corpo do jogador
        }

        // Pressione Esc para destravar o cursor (para desenvolvimento)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}