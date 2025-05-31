using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerFPController : MonoBehaviour
{
    public float moveSpeed = 7.5f;
    public float jumpSpeed = 8.0f;
    // A variável 'gravity' original foi removida.

    [Header("Jump and Gravity Settings")]
    public float upwardGravity = 25.0f;      // Gravidade aplicada enquanto o jogador está subindo no pulo.
    public float downwardGravity = 40.0f;    // Gravidade mais forte aplicada quando o jogador está caindo.
    public float earlyJumpReleaseMultiplier = 1.5f; // Multiplicador para cair mais rápido se soltar o pulo cedo.
    public float groundedGravity = 10.0f;     // Pequena força para baixo para manter o jogador "colado" ao chão.

    private float _verticalVelocity = 0f;     // Variável interna para armazenar a velocidade vertical atual

    [Header("Mouse Look Settings")]
    public Transform playerCameraTransform;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;

    CharacterController characterController;
    // Vector3 moveDirection = Vector3.zero; // _verticalVelocity e horizontalInput vão cuidar disso
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCameraTransform == null)
        {
            Camera childCam = GetComponentInChildren<Camera>();
            if (childCam != null)
            {
                playerCameraTransform = childCam.transform;
                Debug.LogWarning("PlayerFPController: 'playerCameraTransform' não foi atribuído no Inspector. Câmera filha encontrada e atribu�da automaticamente. Para evitar problemas, é recomendado atribuir manually.");
            }
            else
            {
                Debug.LogError("PlayerFPController: 'playerCameraTransform' não foi atribuído no Inspector e nenhuma c�mera filha foi encontrada! A rotação vertical do mouse não funcionar� corretamente. Por favor, atribua a c�mera filha.");
            }
        }
    }

    void Update()
    {
        // --- Movimentação Horizontal ---
        Vector3 horizontalInput = Vector3.zero; 
        if (canMove)
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            
            float curSpeedX = moveSpeed * Input.GetAxis("Vertical");    // W/S para frente/trás
            float curSpeedZ = moveSpeed * Input.GetAxis("Horizontal");  // A/D para esquerda/direita
            horizontalInput = (forward * curSpeedX) + (right * curSpeedZ);
        }

        // --- Lógica Vertical (Pulo e Gravidade) ---
        if (characterController.isGrounded)
        {
            _verticalVelocity = -groundedGravity * Time.deltaTime; // Mantém colado ao chão

            if (Input.GetButtonDown("Jump") && canMove)
            {
                _verticalVelocity = jumpSpeed; // Impulso inicial do pulo
            }
        }
        else // No ar
        {
            // Se o jogador soltou o botão de pulo cedo enquanto ainda estava subindo
            // E pode se mover (para não cortar o pulo se o movimento estiver travado por um menu, por exemplo)
            if (_verticalVelocity > 0 && !Input.GetButton("Jump") && canMove) 
            {
                // Aplica gravidade mais forte para "cortar" o pulo
                _verticalVelocity -= upwardGravity * earlyJumpReleaseMultiplier * Time.deltaTime;
            }
            // Aplicar gravidade diferenciada normal (subida ou descida)
            else if (_verticalVelocity < 0) // Já está descendo
            {
                _verticalVelocity -= downwardGravity * Time.deltaTime;
            }
            else // Ainda está subindo (e segurando o pulo, ou a opção de soltar cedo não ativou)
            {
                _verticalVelocity -= upwardGravity * Time.deltaTime;
            }
        }

        // Construir o vetor de movimento final combinando horizontal e vertical
        Vector3 finalMove = horizontalInput;
        finalMove.y = _verticalVelocity;

        // Aplicar movimento ao Character Controller
        characterController.Move(finalMove * Time.deltaTime);

        // --- Rotação (Mouse Look) ---
        if (canMove && playerCameraTransform != null) 
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCameraTransform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * lookSpeed);
        }

        // --- Cursor ---
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        // Opcional: Lógica para re-travar o cursor, por exemplo, ao clicar na tela
        // if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None && canMove)
        // {
        //     Cursor.lockState = CursorLockMode.Locked;
        //     Cursor.visible = false;
        // }
    }
}