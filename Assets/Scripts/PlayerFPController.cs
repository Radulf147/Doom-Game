using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerFPController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 7.5f;
    public float sprintSpeed = 12.0f; // Velocidade ao correr
    public float jumpSpeed = 8.0f;

    [Header("Jump and Gravity Settings")]
    public float upwardGravity = 25.0f;      // Gravidade aplicada enquanto o jogador está subindo no pulo.
    public float downwardGravity = 40.0f;    // Gravidade mais forte aplicada quando o jogador está caindo.
    public float earlyJumpReleaseMultiplier = 1.5f; // Multiplicador para cair mais rápido se soltar o pulo cedo.
    public float groundedGravity = 10.0f;     // Pequena força para baixo para manter o jogador "colado" ao chão.

    private float _verticalVelocity = 0f;     // Variável interna para armazenar a velocidade vertical atual

    [Header("Mouse Look Settings")]
    public Transform playerCameraTransform;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f; // Limite de rotação vertical (para não dar loop)

    [Header("Footstep Sound Settings")]
    public AudioClip footstepSound; // Arraste seu arquivo "Andar_na_terra" aqui
    public float walkStepInterval = 0.6f;  // Tempo entre os passos ao andar
    public float sprintStepInterval = 0.35f; // Tempo entre os passos ao correr
    public float walkPitch = 1.0f;         // Tom do som ao andar
    public float sprintPitch = 1.3f;       // Tom do som ao correr (um pouco mais agudo/rápido)
    
    private AudioSource footstepAudioSource;
    private float stepTimer = 0f;
    private float _currentAppliedSpeed = 0f; // Para sabermos a velocidade atual e decidir o som do passo

    CharacterController characterController;
    float rotationX = 0; // Variável para acumular a rotação vertical da câmera

    [HideInInspector]
    public bool canMove = true; // Para travar o movimento se necessário (ex: menu)

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        footstepAudioSource = GetComponent<AudioSource>(); 
        if (footstepAudioSource == null)
        {
            Debug.LogWarning("PlayerFPController: Nenhum AudioSource principal encontrado para passos. Adicionando um novo e configurando para 3D...");
            footstepAudioSource = gameObject.AddComponent<AudioSource>();
        }
        footstepAudioSource.playOnAwake = false;
        footstepAudioSource.loop = false;
        footstepAudioSource.spatialBlend = 1.0f; 

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCameraTransform == null)
        {
            Camera childCam = GetComponentInChildren<Camera>();
            if (childCam != null)
            {
                playerCameraTransform = childCam.transform;
                Debug.LogWarning("PlayerFPController: 'playerCameraTransform' não foi atribuído no Inspector. Câmera filha encontrada e atribuída automaticamente. Para evitar problemas, é recomendado atribuir manualmente.");
            }
            else
            {
                Debug.LogError("PlayerFPController: 'playerCameraTransform' não foi atribuído no Inspector e nenhuma c�mera filha foi encontrada! A rotação vertical do mouse não funcionar� corretamente. Por favor, atribua a c�mera filha.");
            }
        }
    }

    void Update()
    {
        Vector3 horizontalInputVector = Vector3.zero; 
        _currentAppliedSpeed = 0f; 

        if (canMove)
        {
            float actualSpeedForFrame = moveSpeed; 
            if (Input.GetKey(KeyCode.LeftShift)) 
            {
                actualSpeedForFrame = sprintSpeed; 
            }

            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            
            float forwardInputAxis = Input.GetAxis("Vertical");    
            float strafeInputAxis = Input.GetAxis("Horizontal");  

            horizontalInputVector = (forward * forwardInputAxis) + (right * strafeInputAxis);

            if (horizontalInputVector.sqrMagnitude > 0.01f) 
            {
                horizontalInputVector.Normalize(); 
                horizontalInputVector *= actualSpeedForFrame; 
                _currentAppliedSpeed = actualSpeedForFrame; 
            }
        }

        if (characterController.isGrounded)
        {
            _verticalVelocity = -groundedGravity * Time.deltaTime; 
            if (Input.GetButtonDown("Jump") && canMove)
            {
                _verticalVelocity = jumpSpeed; 
            }
        }
        else 
        {
            if (_verticalVelocity > 0 && !Input.GetButton("Jump") && canMove) 
            {
                _verticalVelocity -= upwardGravity * earlyJumpReleaseMultiplier * Time.deltaTime;
            }
            else if (_verticalVelocity < 0) 
            {
                _verticalVelocity -= downwardGravity * Time.deltaTime;
            }
            else 
            {
                _verticalVelocity -= upwardGravity * Time.deltaTime;
            }
        }

        // --- Lógica dos Sons de Passos --- MODIFICADA ABAIXO ---
        bool isActuallyMovingOnGround = characterController.isGrounded && _currentAppliedSpeed > 0.01f && canMove;

        if (isActuallyMovingOnGround)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstepSound();
                bool isSprintingNow = (_currentAppliedSpeed == sprintSpeed);
                stepTimer = isSprintingNow ? sprintStepInterval : walkStepInterval; 
            }
        }
        else
        {
            // Se não está se movendo (ou está no ar, ou canMove é false),
            // resetamos o stepTimer para 0.
            // Isso fará com que o primeiro passo seja imediato ao começar a andar de novo.
            stepTimer = 0f; // <<< ALTERAÇÃO PARA CORRIGIR O DELAY INICIAL DO SOM
        }

        Vector3 finalMove = horizontalInputVector; 
        finalMove.y = _verticalVelocity;

        characterController.Move(finalMove * Time.deltaTime);

        if (canMove && playerCameraTransform != null) 
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCameraTransform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * lookSpeed);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        // if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None && canMove)
        // {
        //     Cursor.lockState = CursorLockMode.Locked;
        //     Cursor.visible = false;
        // }
    }

    void PlayFootstepSound()
    {
        if (footstepAudioSource == null || footstepSound == null)
        {
            return; 
        }

        bool isSprinting = (_currentAppliedSpeed == sprintSpeed);
        footstepAudioSource.pitch = isSprinting ? sprintPitch : walkPitch;
        footstepAudioSource.PlayOneShot(footstepSound);
    }
}