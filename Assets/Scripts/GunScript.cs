using UnityEngine;
using System.Collections; // Necessário para usar Coroutines

public class GunScript : MonoBehaviour
{
    [Header("Referências da Arma")]
    public GameObject projectilePrefab; // Seu prefab de projétil (se estiver usando um)
    public Transform projectileSpawnPoint; // Ponto de onde o projétil sai (opcional)
    // Adicione aqui outras variáveis da sua arma, como dano, cadência de tiro, etc.

    [Header("Animação do Sprite da Arma")]
    public SpriteRenderer weaponSpriteRenderer; // Arraste o GameObject "SpriteArmaFP" aqui
    public Sprite idleSprite;                 // Arraste o sprite da arma parada (último frame do GIF)
    public Sprite[] shootAnimationFrames;     // Arraste os frames da animação de tiro (em ordem)
    public float animationFrameRate = 15f;    // Frames por segundo da animação (ex: 10, 15, 20)
    
    private bool isShootingAnimationPlaying = false;
    // private float nextFireTime = 0f; // Para controlar a cadência de tiro, se necessário

    void Start()
    {
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
            Debug.LogWarning("GunScript: 'Idle Sprite' não foi atribuído. A arma pode não resetar para o visual correto após a animação.");
        }
    }

    void Update()
    {
        // Verifica o input para atirar (botão esquerdo do mouse)
        // Adicione aqui sua lógica de cadência de tiro (if Time.time > nextFireTime) se necessário
        if (Input.GetMouseButtonDown(0)) // GetMouseButtonDown para tiro único por clique
        // Se for arma automática, use GetMouseButton(0) e controle a cadência com nextFireTime
        {
            // --- SUA LÓGICA DE TIRO PRINCIPAL VAI AQUI ---
            // Exemplo:
            // if (projectilePrefab != null && projectileSpawnPoint != null)
            // {
            //     Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
            // }
            // Debug.Log("Tiro disparado!"); // Placeholder
            // --------------------------------------------

            // Inicia a animação do sprite da arma, se não estiver tocando
            if (weaponSpriteRenderer != null && shootAnimationFrames != null && shootAnimationFrames.Length > 0 && !isShootingAnimationPlaying)
            {
                StartCoroutine(PlayShootAnimation());
            }
        }
    }

    IEnumerator PlayShootAnimation()
    {
        isShootingAnimationPlaying = true;
        
        // Calcula o tempo de espera entre cada frame da animação
        float delayBetweenFrames = 1.0f / animationFrameRate;

        // Percorre todos os frames da animação de tiro
        for (int i = 0; i < shootAnimationFrames.Length; i++)
        {
            if (weaponSpriteRenderer != null && shootAnimationFrames[i] != null)
            {
                weaponSpriteRenderer.sprite = shootAnimationFrames[i];
            }
            yield return new WaitForSeconds(delayBetweenFrames);
        }

        // Após a animação, volta para o sprite "idle"
        // (que você disse ser o último frame do GIF, então pode ser o último da animação
        // ou um sprite específico que você arrastou para idleSprite)
        if (weaponSpriteRenderer != null && idleSprite != null)
        {
            weaponSpriteRenderer.sprite = idleSprite;
        }
        // Alternativamente, se o idle é SEMPRE o último frame da animação:
        // else if (weaponSpriteRenderer != null && shootAnimationFrames.Length > 0)
        // {
        //     weaponSpriteRenderer.sprite = shootAnimationFrames[shootAnimationFrames.Length - 1];
        // }

        isShootingAnimationPlaying = false;
    }
}