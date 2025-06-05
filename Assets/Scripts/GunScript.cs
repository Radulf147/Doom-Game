using UnityEngine;
using System.Collections;

public class GunScript : MonoBehaviour
{
    [Header("Referências da Arma")]
    public GameObject projectilePrefab;
    public Transform muzzlePoint;
    [SerializeField] private float projectileSpeed = 30f;

    [Header("Gun Settings")]
    [SerializeField] private float fireRate = 0.5f;
    private float nextFireTime;

    [Header("Animação do Sprite da Arma")]
    public SpriteRenderer weaponSpriteRenderer;
    public Sprite idleSprite;
    public Sprite[] shootAnimationFrames;
    public float animationFrameRate = 15f;

    [Header("Som do Tiro")]
    public AudioClip shootSound;

    private bool isShootingAnimationPlaying = false;
    private AudioSource gunAudioSource;

    void Start()
    {
        gunAudioSource = GetComponent<AudioSource>();
        if (gunAudioSource == null)
        {
            gunAudioSource = gameObject.AddComponent<AudioSource>();
        }
        gunAudioSource.playOnAwake = false;
        gunAudioSource.loop = false;

        if (weaponSpriteRenderer != null && idleSprite != null)
        {
            weaponSpriteRenderer.sprite = idleSprite;
        }
        // Logs de erro/aviso para weaponSpriteRenderer e idleSprite já existem no seu código original.
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
        if (projectilePrefab == null || muzzlePoint == null)
        {
            if (projectilePrefab == null) Debug.LogError("GunScript: Projectile Prefab não atribuído!", this);
            if (muzzlePoint == null) Debug.LogError("GunScript: Muzzle Point não atribuído!", this);
            return;
        }

        // 1. Determinar o ponto de mira (centro da tela)
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Raio do centro da tela
        RaycastHit hitInfo;
        Vector3 targetPoint;

        // Define uma distância máxima para o raycast, para onde o projétil irá se não atingir nada.
        float maxRayDistance = 1000f;

        if (Physics.Raycast(ray, out hitInfo, maxRayDistance))
        {
            targetPoint = hitInfo.point; // O projétil mirará no ponto de colisão do raio
            Debug.Log("DEBUG (GunScript): Raio da mira atingiu " + hitInfo.collider.name + " em " + targetPoint);
        }
        else
        {
            targetPoint = ray.GetPoint(maxRayDistance); // O projétil mirará em um ponto distante na direção do raio
            Debug.Log("DEBUG (GunScript): Raio da mira não atingiu nada, mirando para " + targetPoint);
        }

        // 2. Calcular a direção do muzzlePoint até o targetPoint
        Vector3 directionToTarget = (targetPoint - muzzlePoint.position).normalized;

        // 3. Instanciar o projétil no muzzlePoint, mas rotacionado para a direção do alvo
        GameObject newProjectile = Instantiate(projectilePrefab, muzzlePoint.position, Quaternion.LookRotation(directionToTarget));
        Debug.Log("DEBUG (GunScript): Projétil instanciado em " + muzzlePoint.position + " olhando para " + directionToTarget);

        Rigidbody rb = newProjectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Aplicar força na direção calculada
            rb.AddForce(directionToTarget * projectileSpeed, ForceMode.VelocityChange);
            Debug.Log($"DEBUG (GunScript): Força de {projectileSpeed} aplicada ao projétil na direção {directionToTarget}");
        }
        else
        {
            Debug.LogError("ERRO DEBUG: O Prefab do Projétil não tem um componente Rigidbody!", newProjectile);
        }

        if (gunAudioSource != null && shootSound != null)
        {
            gunAudioSource.PlayOneShot(shootSound);
        }
        else if (shootSound == null)
        {
            Debug.LogWarning("GunScript: AudioClip de tiro (Shoot Sound) não atribuído no Inspector.", this);
        }
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