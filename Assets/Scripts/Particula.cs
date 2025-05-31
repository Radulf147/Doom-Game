using UnityEngine;

public class Particula : MonoBehaviour
{
    public Vector3 velocity;
    public float lifetime;

    private float age = 0f;
    private float groundHitY;
    private Vector3 baseScale;

    private Renderer particleRenderer;
    private MaterialPropertyBlock propertyBlock;
    private Color originalMaterialColor = Color.white; // Guarda a cor original (incluindo alfa) do material

    // Nome da propriedade de cor principal do shader (pode variar)
    // Comum: "_Color" para Standard/Legacy, "_BaseColor" para URP/HDRP Unlit
    private string mainColorPropertyName = "_Color"; 

    void Awake()
    {
        particleRenderer = GetComponent<Renderer>();
        if (particleRenderer != null)
        {
            propertyBlock = new MaterialPropertyBlock();
            particleRenderer.GetPropertyBlock(propertyBlock); // Pega as propriedades atuais

            // Tenta descobrir o nome da propriedade de cor e pegar a cor original
            if (particleRenderer.sharedMaterial.HasProperty("_BaseColor"))
            {
                mainColorPropertyName = "_BaseColor";
                originalMaterialColor = particleRenderer.sharedMaterial.GetColor("_BaseColor");
            }
            else if (particleRenderer.sharedMaterial.HasProperty("_Color"))
            {
                mainColorPropertyName = "_Color";
                originalMaterialColor = particleRenderer.sharedMaterial.GetColor("_Color");
            }
            // Se não encontrar nenhuma, originalMaterialColor permanece branca, o que pode não ser o ideal
            // mas evita o erro de propriedade não encontrada ao tentar ler .color
        }
        baseScale = transform.localScale;
    }

    public void Initialize(Vector3 startPosition, Vector3 initialVelocity, float life, float groundLevel, float scaleMultiplier)
    {
        transform.position = startPosition;
        transform.localScale = baseScale * scaleMultiplier;

        this.velocity = initialVelocity;
        this.lifetime = life;
        this.age = 0f;
        this.groundHitY = groundLevel;

        if (particleRenderer != null && propertyBlock != null)
        {
            // Define a cor inicial (com alfa máximo do original) no property block
            Color initialColorWithFullAlpha = originalMaterialColor;
            initialColorWithFullAlpha.a = originalMaterialColor.a; // Usa o alfa original do material como base
            propertyBlock.SetColor(mainColorPropertyName, initialColorWithFullAlpha);
            particleRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    void Update()
    {
        age += Time.deltaTime;
        transform.Translate(velocity * Time.deltaTime, Space.World);

        // Fade out da transparência ao longo da vida
        if (particleRenderer != null && propertyBlock != null)
        {
            // Verifica se a propriedade de cor ainda existe (segurança extra)
            if (particleRenderer.sharedMaterial.HasProperty(mainColorPropertyName))
            {
                float lifeRatio = Mathf.Clamp01(age / lifetime);
                
                // A cor base para o Lerp deve ser a cor original do material para manter o RGB,
                // e o alfa original como ponto de partida para o Lerp.
                Color currentColor = originalMaterialColor; 
                currentColor.a = Mathf.Lerp(originalMaterialColor.a, 0f, lifeRatio); // Faz o fade do alfa original para 0

                propertyBlock.SetColor(mainColorPropertyName, currentColor);
                particleRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        if (age >= lifetime || transform.position.y <= groundHitY)
        {
            Destroy(gameObject);
            return;
        }
    }
}