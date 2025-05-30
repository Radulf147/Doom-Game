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
    public float lookXLimit = 60.0f; // Limite de rotação vertical (para não dar loop)

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0; // Variável para acumular a rotação vertical da câmera

    [HideInInspector]
    public bool canMove = true; // Para travar o movimento se necessário (ex: menu)

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Travar cursor no centro da tela e escondê-lo
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Validação para garantir que a câmera foi atribuída
        if (playerCameraTransform == null)
        {
            // Tenta encontrar a câmera automaticamente se for filha direta e tiver o componente Camera
            Camera childCam = GetComponentInChildren<Camera>();
            if (childCam != null)
            {
                playerCameraTransform = childCam.transform;
                Debug.LogWarning("PlayerFPController: 'playerCameraTransform' não foi atribuído no Inspector. Câmera filha encontrada e atribuída automaticamente. Para evitar problemas, é recomendado atribuir manualmente.");
            }
            else
            {
                Debug.LogError("PlayerFPController: 'playerCameraTransform' não foi atribuído no Inspector e nenhuma câmera filha foi encontrada! A rotação vertical do mouse não funcionará corretamente. Por favor, atribua a câmera filha.");
            }
        }
    }

    void Update()
    {
        // --- Movimentação ---
        // Verifica se pode se mover antes de processar inputs de movimento
        float curSpeedX = 0;
        float curSpeedY = 0;

        if (canMove)
        {
            // Mantém a direção local do jogador
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);

            curSpeedX = moveSpeed * Input.GetAxis("Vertical");   // W/S para frente/trás
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
            moveDirection.y = movementDirectionY; // Mantém a velocidade vertical (ex: se já estiver caindo)
        }

        // Aplicar gravidade (apenas se não estiver no chão)
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Aplicar movimento ao Character Controller
        characterController.Move(moveDirection * Time.deltaTime);

        // --- Rotação (Mouse Look) ---
        if (canMove && playerCameraTransform != null) // Só rotaciona se puder mover E a câmera estiver atribuída
        {
            // Rotação Vertical (Pitch) - Aplicada à CÂMERA FILHA
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCameraTransform.localRotation = Quaternion.Euler(rotationX, 0, 0); // <<< MODIFICADO: Aplica apenas à câmera

            // Rotação Horizontal (Yaw) - Aplicada ao CharacterController/Player (este objeto)
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