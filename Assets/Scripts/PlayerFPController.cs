using UnityEngine;

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
    public float groundedGravity = 2.0f; // Valor ajustado para "grudar" melhor no chão

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
    
    private AudioSource footstepAudioSource;
    private float stepTimer = 0f;
    private float _currentAppliedSpeed = 0f;

    CharacterController characterController;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

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

        // --- Lógica Vertical (Pulo e Gravidade) --- MODIFICADA AQUI ---
        if (characterController.isGrounded)
        {
            // Aplica uma velocidade vertical negativa constante para manter "colado" ao chão/rampas
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
            stepTimer = 0f; 
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