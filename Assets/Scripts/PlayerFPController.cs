using UnityEngine;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class PlayerFPController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 7.5f;
    public float sprintSpeed = 12.0f;
    public float jumpSpeed = 8.0f;

    [Header("Jump and Gravity Settings")]
    public float upwardGravity = 25.0f;
    public float downwardGravity = 40.0f;
    public float earlyJumpReleaseMultiplier = 1.5f;
    public float groundedGravity = 2.0f;

    private float _verticalVelocity = 0f;

    [Header("Mouse Look Settings")]
    public Transform playerCameraTransform;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;

    [Header("Footstep Sound Settings")]
    public AudioClip footstepSound;
    public float walkStepInterval = 0.6f;
    public float sprintStepInterval = 0.35f;
    public float walkPitch = 1.0f;
    public float sprintPitch = 1.3f;

    // --- NOVO CABEÇALHO PARA O SISTEMA DE COLETA ---
    [Header("Interaction Settings")]
    public float interactionDistance = 3.0f; // Distância máxima para poder pegar um item
    public TextMeshProUGUI interactionPromptText; // O texto "Pressione E para pegar"
    public FaseDoisManager faseDoisManager; // Referência ao nosso gerenciador de fase
    // --- FIM DAS NOVAS VARIÁVEIS ---


    private AudioSource footstepAudioSource;
    private float stepTimer = 0f;
    private float _currentAppliedSpeed = 0f;

    CharacterController characterController;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    // --- ADICIONADO PARA O SISTEMA DE ESTAMINA ---
    private StaminaController staminaController; // Referência ao controlador de estamina
    private float speedModifier = 1.0f; // Multiplicador para a velocidade (1f = 100% da velocidade)
    // --- FIM DAS ADIÇÕES ---

    void OnEnable()
    {
        // Zera a velocidade vertical para impedir quedas ou pulos fantasmas.
        _verticalVelocity = 0f;

        if (characterController != null)
        {
            characterController.Move(Vector3.zero);
        }
    }


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
                Debug.LogError("PlayerFPController: 'playerCameraTransform' não foi atribuído no Inspector e nenhuma câmera filha foi encontrada! A rotação vertical do mouse não funcionará corretamente. Por favor, atribua a câmera filha.");
            }
        }

        // --- ADICIONADO PARA O SISTEMA DE ESTAMINA ---
        // Pega a referência ao StaminaController no mesmo GameObject
        staminaController = GetComponent<StaminaController>();
        if (staminaController == null)
        {
            Debug.LogError("PlayerFPController: StaminaController não encontrado no jogador! O sistema de estamina não funcionará.", this);
        }

        // Garante que o texto de interação comece desligado
        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }

    } 

    void Update()
    {
        Vector3 horizontalInputVector = Vector3.zero; 
        _currentAppliedSpeed = 0f; 

        if (canMove)
        {
            // --- LÓGICA DE VELOCIDADE MODIFICADA PARA ESTAMINA ---
            float actualSpeedForFrame = moveSpeed; 
            bool isTryingToRun = Input.GetKey(KeyCode.LeftShift);

            // Verifica se o jogador está tentando correr E se tem permissão do StaminaController
            if (isTryingToRun && staminaController != null && staminaController.CanRun())
            {
                actualSpeedForFrame = sprintSpeed; 
            }
            // Se ele tentar correr mas não puder (por estar exausto), a velocidade permanecerá como moveSpeed.

            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            
            float forwardInputAxis = Input.GetAxis("Vertical");    
            float strafeInputAxis = Input.GetAxis("Horizontal");  

            horizontalInputVector = (forward * forwardInputAxis) + (right * strafeInputAxis);

            if (horizontalInputVector.sqrMagnitude > 0.01f) 
            {
                horizontalInputVector.Normalize(); 
                horizontalInputVector *= actualSpeedForFrame;

                // Aplica o modificador de velocidade (penalidade de exaustão)
                horizontalInputVector *= speedModifier;

                _currentAppliedSpeed = actualSpeedForFrame; 
            }
            // --- FIM DA LÓGICA MODIFICADA ---
        }

        // --- Lógica Vertical (Pulo e Gravidade) ---
        if (characterController.isGrounded)
        {
            _verticalVelocity = -Mathf.Abs(groundedGravity); 

            if (Input.GetButtonDown("Jump") && canMove)
            {
                _verticalVelocity = jumpSpeed; 
            }
        }
        else // No ar
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

        // --- Lógica dos Sons de Passos ---
        if (IsMovingOnGround() && canMove)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstepSound();
                // A velocidade do som dos passos agora também depende da permissão para correr
                bool canCurrentlyRun = staminaController != null && staminaController.CanRun();
                bool isSprintingNow = Input.GetKey(KeyCode.LeftShift) && canCurrentlyRun;
                stepTimer = isSprintingNow ? sprintStepInterval : walkStepInterval; 
            }
        }
        else
        {
            stepTimer = 0f; 
        }

        Vector3 finalMove = horizontalInputVector; 
        finalMove.y = _verticalVelocity;

        characterController.Move(finalMove * Time.deltaTime);

        // A nova condição verifica se o jogo NÃO está pausado
        if (canMove && playerCameraTransform != null && PauseMenuController.isPaused == false) 
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

        HandleInteraction();
    }

    private bool HandleInteraction()
    {
        if (!canMove || faseDoisManager == null) 
        {
            if (interactionPromptText != null) interactionPromptText.gameObject.SetActive(false);
            return false;
        }

        RaycastHit hit;
        if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out hit, interactionDistance))
        {
            if (hit.collider.CompareTag("Trilho") || hit.collider.CompareTag("Fuel") || hit.collider.CompareTag("Radiator"))
            {
                if (interactionPromptText != null) interactionPromptText.gameObject.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (interactionPromptText != null) interactionPromptText.gameObject.SetActive(false);

                    // MUDANÇA AQUI: Em vez de passar a tag, passamos o GameObject inteiro.
                    faseDoisManager.ColetarItem(hit.collider.gameObject);
                    
                    // MUDANÇA IMPORTANTE: A linha abaixo foi REMOVIDA daqui.
                    // Destroy(hit.collider.gameObject); 
                    
                    return true; // Avisa que um item foi coletado para parar o movimento.
                }
                return false;
            }
        }
        
        if (interactionPromptText != null) interactionPromptText.gameObject.SetActive(false);
        return false;
    }

    void PlayFootstepSound()
    {
        if (footstepAudioSource == null || footstepSound == null)
        {
            return; 
        }

        bool canCurrentlyRun = staminaController != null && staminaController.CanRun();
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && canCurrentlyRun;
        footstepAudioSource.pitch = isSprinting ? sprintPitch : walkPitch;
        footstepAudioSource.PlayOneShot(footstepSound);
    }
    
    // --- MÉTODOS PÚBLICOS ADICIONADOS PARA ESTAMINA ---
    
    /// <summary>
    /// Permite que outros scripts (como o StaminaController) alterem a velocidade do jogador.
    /// </summary>
    /// <param name="modifier">O multiplicador a ser aplicado (ex: 0.5 para 50% da velocidade).</param>
    public void ApplySpeedModifier(float modifier)
    {
        speedModifier = modifier;
    }

    /// <summary>
    /// Permite que outros scripts (como o StaminaController) saibam se o jogador está se movendo no chão.
    /// </summary>
    /// <returns>Verdadeiro se o jogador está no chão e se movendo horizontalmente.</returns>
    public bool IsMovingOnGround()
    {
        // Usa a lógica que já existia no Update para determinar o movimento no chão.
        return characterController.isGrounded && _currentAppliedSpeed > 0.01f;
    }
    // --- FIM DOS MÉTODOS PÚBLICOS ---
}