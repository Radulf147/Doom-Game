using UnityEngine;
using System.Collections;
using TMPro; // Adicione esta linha para poder usar TextMeshPro

public class GunScript : MonoBehaviour
{
    [Header("Referências da Arma")]
    public Transform muzzlePoint;

    [Header("Gun Settings")]
    [SerializeField] private float fireRate = 0.5f;
    private float nextFireTime;
    public float weaponRange = 1000f;
    public int weaponDamage = 20;

    [Header("Configurações de Munição")]
    public AmmoPickup.AmmoType weaponAmmoType;
    public int currentAmmo;
    public int maxAmmoInClip = 6;
    public int currentReserveAmmo;
    public int maxReserveAmmo = 60;
    public float reloadTime = 1.5f;
    private bool isReloading = false;

    [Header("Animação do Sprite da Arma")]
    public SpriteRenderer weaponSpriteRenderer;
    public Sprite idleSprite;
    public Sprite[] shootAnimationFrames;
    public float animationFrameRate = 15f;

    [Header("Som do Tiro")]
    public AudioClip shootSound;
    // Opcional: Adicione um som para quando a arma estiver vazia
    // public AudioClip emptyClipSound;

    [Header("Efeitos de Impacto (para Hitscan)")]
    public GameObject hitEffectPrefab;
    public GameObject bulletHoleDecalPrefab;
    public float decalOffset = 0.01f;

    [Header("Referências da UI de Munição")]
    public TextMeshProUGUI ammoText;

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
        
        currentAmmo = maxAmmoInClip;
        UpdateAmmoUI();
    }
    
    void OnEnable()
    {
        isReloading = false;
        UpdateAmmoUI();
    }

    void Update()
    {
        // Se estiver recarregando, não faz mais nada.
        if (isReloading) return;

        // Tenta atirar se o botão for pressionado e o tempo de espera tiver passado.
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime) // Usando GetButton para tiros automáticos se segurar
        {
            AttemptToFire();
        }

        // Inicia a recarga com a tecla R.
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmoInClip && currentReserveAmmo > 0)
        {
            StartCoroutine(Reload());
        }
    }

    void AttemptToFire()
    {
        // Só atira se tiver munição no pente.
        if (currentAmmo > 0)
        {
            FireHitscan(); // Chama a lógica do tiro
            nextFireTime = Time.time + fireRate; // Define o tempo para o próximo tiro
        }
        else
        {
            // Se tentar atirar sem munição, toca um som de clique (se configurado)
            // e tenta recarregar automaticamente se houver munição na reserva.
            if (Time.time >= nextFireTime) // Evita múltiplos sons de clique por segundo
            {
                Debug.Log("Arma sem munição!");
                // if (gunAudioSource != null && emptyClipSound != null)
                // {
                //     gunAudioSource.PlayOneShot(emptyClipSound);
                // }
                nextFireTime = Time.time + fireRate;

                // Tenta recarregar automaticamente
                if (currentReserveAmmo > 0)
                {
                    StartCoroutine(Reload());
                }
            }
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Recarregando...");
        // Tocar som de recarga aqui (opcional)

        yield return new WaitForSeconds(reloadTime);

        int ammoNeeded = maxAmmoInClip - currentAmmo;
        int ammoToMove = Mathf.Min(ammoNeeded, currentReserveAmmo);

        currentAmmo += ammoToMove;
        currentReserveAmmo -= ammoToMove;
        
        UpdateAmmoUI();
        isReloading = false;
    }

    void FireHitscan()
    {
        currentAmmo--; // Gasta uma bala
        UpdateAmmoUI();
        
        // --- CORREÇÃO AQUI: A ANIMAÇÃO E O SOM SÃO CHAMADOS DENTRO DO TIRO REAL ---
        // Inicia a animação do sprite do tiro
        if (weaponSpriteRenderer != null && shootAnimationFrames != null && shootAnimationFrames.Length > 0 && !isShootingAnimationPlaying)
        {
            StartCoroutine(PlayShootAnimation());
        }

        // Toca o som do tiro
        if (gunAudioSource != null && shootSound != null)
        {
            gunAudioSource.PlayOneShot(shootSound);
        }
        
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hitInfo;
        
        if (Physics.Raycast(ray, out hitInfo, weaponRange))
        {
            // Toda a sua lógica de impacto, dano e decalques continua a mesma aqui...
            Vector3 hitPoint = hitInfo.point;
            Vector3 hitNormal = hitInfo.normal;

            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
            }

            if (bulletHoleDecalPrefab != null && hitInfo.collider.attachedRigidbody == null)
            {
                GameObject decalInstance = Instantiate(bulletHoleDecalPrefab, hitPoint + hitNormal * decalOffset, Quaternion.LookRotation(hitNormal));
                decalInstance.transform.SetParent(hitInfo.transform);
                Destroy(decalInstance, 10f);
            }

            IDamageable damageableObject = hitInfo.collider.GetComponentInParent<IDamageable>();
            if (damageableObject != null)
            {
                damageableObject.TakeDamage(weaponDamage);
            }
        }
    }
    
    public void AddAmmo(int amount, AmmoPickup.AmmoType type)
    {
        if (type == weaponAmmoType)
        {
            currentReserveAmmo += amount;
            currentReserveAmmo = Mathf.Min(currentReserveAmmo, maxReserveAmmo);
            UpdateAmmoUI();
        }
    }
    
    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo + " / " + currentReserveAmmo;
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