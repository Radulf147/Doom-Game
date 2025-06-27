using UnityEngine;
using System.Collections;

public class GunScript : MonoBehaviour
{
    // ... (suas variáveis existentes, não precisam mudar) ...
    [Header("Referências da Arma")]
    public Transform muzzlePoint;

    [Header("Gun Settings")]
    [SerializeField] private float fireRate = 0.5f;
    private float nextFireTime;
    public float weaponRange = 1000f;
    public int weaponDamage = 20;

    [Header("Animação do Sprite da Arma")]
    public SpriteRenderer weaponSpriteRenderer;
    public Sprite idleSprite;
    public Sprite[] shootAnimationFrames;
    public float animationFrameRate = 15f;

    [Header("Som do Tiro")]
    public AudioClip shootSound;

    [Header("Efeitos de Impacto (para Hitscan)")]
    public GameObject hitEffectPrefab;
    public GameObject bulletHoleDecalPrefab;
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
        if (gunAudioSource != null && shootSound != null)
        {
            gunAudioSource.PlayOneShot(shootSound);
        }
        else if (shootSound == null)
        {
            Debug.LogWarning("GunScript: AudioClip de tiro (Shoot Sound) não atribuído no Inspector.", this);
        }

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, weaponRange))
        {
            Vector3 hitPoint = hitInfo.point;
            Vector3 hitNormal = hitInfo.normal;

            if (hitEffectPrefab != null)
            {
                GameObject impactFX = Instantiate(hitEffectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
                Destroy(impactFX, 2f);
            }

            if (bulletHoleDecalPrefab != null && hitInfo.collider.attachedRigidbody == null)
            {
                Vector3 decalPosition = hitPoint + hitNormal * decalOffset;
                Quaternion decalRotation = Quaternion.LookRotation(hitNormal) * Quaternion.Euler(0, 0, Random.Range(0f, 360f));
                GameObject decalInstance = Instantiate(bulletHoleDecalPrefab, decalPosition, decalRotation);
                decalInstance.transform.SetParent(hitInfo.transform);
                Destroy(decalInstance, 10f);
            }

            // --- LÓGICA DE DANO MODIFICADA AQUI ---
            // Procuramos por QUALQUER objeto que tenha um script implementando "IDamageable".
            // Pode ser o nosso zumbi, um caixote futuro, um chefe, etc.
            IDamageable damageableObject = hitInfo.collider.GetComponentInParent<IDamageable>();
            
            if (damageableObject != null)
            {
                // Se encontramos algo "danificável", chamamos o método TakeDamage dele.
                // A própria lógica do zumbi (ou de outro objeto) cuidará do que fazer com o dano.
                Debug.Log("Atingiu um objeto danificável: " + hitInfo.collider.name);
                damageableObject.TakeDamage(weaponDamage);
            }

            // A lógica específica do DestructibleCrate pode ser removida se você o modificar
            // para também implementar IDamageable. Por enquanto, podemos deixar os dois.
            DestructibleCrate crate = hitInfo.collider.GetComponentInParent<DestructibleCrate>();
            if (crate != null)
            {
                crate.TakeDamage(weaponDamage, hitPoint, hitNormal);
            }
            // --- FIM DA LÓGICA DE DANO ---
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