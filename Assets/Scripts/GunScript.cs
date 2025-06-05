using UnityEngine;
using System.Collections; // Necessário para usar Coroutines

public class GunScript : MonoBehaviour
{
    [Header("Gun Settings")]
    [SerializeField] private float fireRate = 0.5f; // Seu controle de cadência
    [SerializeField] private float projectileSpeed = 30f; // Sua velocidade de projétil

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform muzzlePoint;

    [Header("Animação do Sprite da Arma")]
    public SpriteRenderer weaponSpriteRenderer;
    public Sprite idleSprite;
    public Sprite[] shootAnimationFrames;
    public float animationFrameRate = 15f;
    
    private bool isShootingAnimationPlaying = false;
    private float nextFireTime; // Seu controle de cadência

    void Start()
    {
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
        // Verifica o input e a cadência de tiro
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            // Lógica de Disparo (baseada no seu código antigo)
            ShootProjectile(); 
            nextFireTime = Time.time + fireRate; // Define o tempo para o próximo tiro

            // Inicia a animação do sprite da arma, se não estiver tocando
            if (weaponSpriteRenderer != null && shootAnimationFrames != null && shootAnimationFrames.Length > 0 && !isShootingAnimationPlaying)
            {
                StartCoroutine(PlayShootAnimation());
            }
        }
    }

    void ShootProjectile()
    {
        Debug.Log("DEBUG: Função ShootProjectile Iniciada.");
        if (projectilePrefab == null)
        {
            Debug.LogError("ERRO DEBUG: Projectile Prefab não atribuído no GunScript!", this);
            return;
        }
        if (muzzlePoint == null)
        {
            Debug.LogError("ERRO DEBUG: Muzzle Point não atribuído no GunScript!", this);
            return;
        }

        GameObject newProjectile = Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
        Debug.Log("DEBUG: Projétil instanciado com sucesso: " + newProjectile.name);

        Rigidbody rb = newProjectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(muzzlePoint.forward * projectileSpeed, ForceMode.VelocityChange);
            Debug.Log($"DEBUG: Força de {projectileSpeed} aplicada ao projétil. Direção: {muzzlePoint.forward}");
        }
        else
        {
            Debug.LogError("ERRO DEBUG: O Prefab do Projétil não tem um componente Rigidbody!", newProjectile);
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