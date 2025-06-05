using UnityEngine;
using System.Collections;

public class GunScript : MonoBehaviour
{
    [Header("Referências da Arma")]
    public GameObject projectilePrefab;
    public Transform muzzlePoint; // Renomeei para muzzlePoint como no seu código antigo
    [SerializeField] private float projectileSpeed = 30f; // Sua velocidade de projétil

    [Header("Gun Settings")]
    [SerializeField] private float fireRate = 0.5f; // Seu controle de cadência
    private float nextFireTime; // Seu controle de cadência

    [Header("Animação do Sprite da Arma")]
    public SpriteRenderer weaponSpriteRenderer;
    public Sprite idleSprite;
    public Sprite[] shootAnimationFrames;
    public float animationFrameRate = 15f;
    
    [Header("Som do Tiro")] // <<< NOVO HEADER E VARIÁVEL
    public AudioClip shootSound; // <<< ADICIONE ESTA LINHA: Arraste seu som de tiro aqui

    private bool isShootingAnimationPlaying = false;
    private AudioSource gunAudioSource; // <<< ADICIONE ESTA LINHA

    void Start()
    {
        // Pega ou adiciona o AudioSource para os sons da arma
        gunAudioSource = GetComponent<AudioSource>();
        if (gunAudioSource == null)
        {
            Debug.LogWarning("GunScript: Nenhum AudioSource encontrado no objeto " + gameObject.name + ". Adicionando um novo.");
            gunAudioSource = gameObject.AddComponent<AudioSource>();
        }
        // Configurações padrão para o AudioSource da arma, caso ele tenha sido criado agora
        gunAudioSource.playOnAwake = false;
        gunAudioSource.loop = false;
        // Você pode definir o spatialBlend aqui também se quiser garantir:
        // gunAudioSource.spatialBlend = 0f; // Para som 2D

        // Garante que a arma começa com o sprite "idle" (parado)
        if (weaponSpriteRenderer != null && idleSprite != null)
        {
            weaponSpriteRenderer.sprite = idleSprite;
        }
        else if (weaponSpriteRenderer == null)
        {
            Debug.LogError("GunScript: 'Weapon Sprite Renderer' não foi atribuído no Inspector!");
        }
        else if (idleSprite == null)
        {
            Debug.LogWarning("GunScript: 'Idle Sprite' não foi atribuído.");
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            ShootProjectile(); 
            nextFireTime = Time.time + fireRate; 

            if (weaponSpriteRenderer != null && shootAnimationFrames != null && shootAnimationFrames.Length > 0 && !isShootingAnimationPlaying)
            {
                StartCoroutine(PlayShootAnimation());
            }
        }
    }

    void ShootProjectile()
    {
        // Debug.Log("DEBUG: Função ShootProjectile Iniciada."); // Seus logs antigos
        if (projectilePrefab == null || muzzlePoint == null)
        {
            // Debug.LogError("ERRO DEBUG: Projectile Prefab ou Muzzle Point não atribuído...", this); // Seus logs antigos
            if(projectilePrefab == null) Debug.LogError("GunScript: Projectile Prefab não atribuído!", this);
            if(muzzlePoint == null) Debug.LogError("GunScript: Muzzle Point não atribuído!", this);
            return;
        }

        GameObject newProjectile = Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
        // Debug.Log("DEBUG: Projétil instanciado..."); // Seus logs antigos

        Rigidbody rb = newProjectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(muzzlePoint.forward * projectileSpeed, ForceMode.VelocityChange);
            // Debug.Log($"DEBUG: Força aplicada..."); // Seus logs antigos
        }
        else
        {
            Debug.LogError("ERRO DEBUG: O Prefab do Projétil não tem um componente Rigidbody!", newProjectile);
        }

        // --- TOCAR O SOM DO TIRO --- // <<< ADICIONADO AQUI
        if (gunAudioSource != null && shootSound != null)
        {
            // PlayOneShot permite tocar vários sons sobrepostos (bom para tiros rápidos)
            // e você pode passar um segundo argumento para escalar o volume (0.0 a 1.0)
            gunAudioSource.PlayOneShot(shootSound /*, 1.0f */); // O segundo argumento é opcional (volume scale)
        }
        else if(shootSound == null)
        {
            Debug.LogWarning("GunScript: AudioClip de tiro (Shoot Sound) não atribuído no Inspector.", this);
        }
        // -------------------------- //
    }

    IEnumerator PlayShootAnimation()
    {
        isShootingAnimationPlaying = true;
        float delayBetweenFrames = 1.0f / animationFrameRate;

        for (int i = 0; i < shootAnimationFrames.Length; i++)
        {
            if (weaponSpriteRenderer != null && shootAnimationFrames[i] != null)
            {
                weaponSpriteRenderer.sprite = shootAnimationFrames[i];
            }
            yield return new WaitForSeconds(delayBetweenFrames);
        }

        if (weaponSpriteRenderer != null && idleSprite != null)
        {
            weaponSpriteRenderer.sprite = idleSprite;
        }
        isShootingAnimationPlaying = false;
    }
}