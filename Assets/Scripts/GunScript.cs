using UnityEngine;
using System.Collections;

public class GunScript : MonoBehaviour
{
    // Suas variáveis existentes ...
    [Header("Referências da Arma")]
    // public GameObject projectilePrefab; // NÃO MAIS NECESSÁRIO para hitscan puro
    public Transform muzzlePoint; // Ainda útil para efeitos visuais como muzzle flash
    // [SerializeField] private float projectileSpeed = 30f; // NÃO MAIS NECESSÁRIO

    [Header("Gun Settings")]
    [SerializeField] private float fireRate = 0.5f;
    private float nextFireTime;
    public float weaponRange = 1000f; // Distância máxima do tiro hitscan
    public int weaponDamage = 20;   // Dano da arma

    [Header("Animação do Sprite da Arma")]
    public SpriteRenderer weaponSpriteRenderer;
    public Sprite idleSprite;
    public Sprite[] shootAnimationFrames;
    public float animationFrameRate = 15f;

    [Header("Som do Tiro")]
    public AudioClip shootSound;

    [Header("Efeitos de Impacto (para Hitscan)")] // NOVO
    public GameObject hitEffectPrefab; // Efeito de faíscas/poeira no ponto de impacto
    public GameObject bulletHoleDecalPrefab; // Prefab do decalque de buraco de bala
    public float decalOffset = 0.01f;

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
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            // Renomeado para FireHitscan()
            FireHitscan();
            nextFireTime = Time.time + fireRate;

            if (weaponSpriteRenderer != null && shootAnimationFrames != null && shootAnimationFrames.Length > 0 && !isShootingAnimationPlaying)
            {
                StartCoroutine(PlayShootAnimation());
            }
        }
    }

    void FireHitscan()
    {
        // 1. Tocar o som do tiro imediatamente
        if (gunAudioSource != null && shootSound != null)
        {
            gunAudioSource.PlayOneShot(shootSound);
        }
        else if (shootSound == null)
        {
            Debug.LogWarning("GunScript: AudioClip de tiro (Shoot Sound) não atribuído no Inspector.", this);
        }

        // 2. Lançar o Raycast do centro da tela (mira)
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hitInfo;

        Debug.Log("DEBUG (GunScript): Disparando Hitscan...");

        if (Physics.Raycast(ray, out hitInfo, weaponRange)) // weaponRange define o alcance máximo
        {
            Debug.Log("DEBUG (GunScript): Hitscan atingiu " + hitInfo.collider.name + " em " + hitInfo.point);
            Vector3 hitPoint = hitInfo.point;
            Vector3 hitNormal = hitInfo.normal;

            // 3. Aplicar efeitos de impacto e dano
            // Instancia efeito de impacto visual (faíscas, poeira)
            if (hitEffectPrefab != null)
            {
                GameObject impactFX = Instantiate(hitEffectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
                Destroy(impactFX, 2f); // Destrói o efeito após 2 segundos
            }

            // Instancia decalque de buraco de bala
            if (bulletHoleDecalPrefab != null && hitInfo.collider.attachedRigidbody == null) // Só em objetos estáticos
            {
                Vector3 decalPosition = hitPoint + hitNormal * decalOffset;
                Quaternion decalRotation = Quaternion.LookRotation(hitNormal) * Quaternion.Euler(0, 0, Random.Range(0f, 360f));
                GameObject decalInstance = Instantiate(bulletHoleDecalPrefab, decalPosition, decalRotation);
                decalInstance.transform.SetParent(hitInfo.transform);
                Destroy(decalInstance, 10f);
            }

            // Tenta aplicar dano ao caixote destrutível
            DestructibleCrate crate = hitInfo.collider.GetComponentInParent<DestructibleCrate>();
            if (crate != null)
            {
                crate.TakeDamage(weaponDamage, hitPoint, hitNormal);
            }
            // Adicione aqui lógica para danificar outros tipos de inimigos/objetos
            // Ex: EnemyHealth enemy = hitInfo.collider.GetComponent<EnemyHealth>();
            // if (enemy != null) { enemy.TakeDamage(weaponDamage); }

        }
        else
        {
            Debug.Log("DEBUG (GunScript): Hitscan não atingiu nada dentro do alcance de " + weaponRange + " unidades.");
            // Opcional: Você pode querer instanciar um "tracer" visual (um rastro de bala)
            // que vai do muzzlePoint até um ponto distante na direção do raio, mesmo se nada for atingido.
        }

        // 4. Efeitos visuais da arma (Muzzle flash, etc. - Opcional)
        // Se você tiver um efeito de "fogo" no cano da arma (muzzle flash), instancie-o aqui
        // no seu 'muzzlePoint.position' e 'muzzlePoint.rotation'.
        // Ex: if (muzzleFlashPrefab != null) Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
    }

    IEnumerator PlayShootAnimation()
    {
        // Seu código de animação de sprite continua o mesmo
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