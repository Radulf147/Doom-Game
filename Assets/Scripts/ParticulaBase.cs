using UnityEngine;

public abstract class ParticulaBase : MonoBehaviour
{
    [Header("Configurações Base da Partícula")]
    public Vector3 velocity;
    public float lifetime;

    protected float age = 0f;
    protected Vector3 baseScale;

    protected Renderer particleRenderer;
    protected MaterialPropertyBlock propertyBlock;
    protected Color originalMaterialColor = Color.white;
    protected string mainColorPropertyName = "_Color"; // Padrão

    protected virtual void Awake()
    {
        particleRenderer = GetComponent<Renderer>();
        if (particleRenderer != null)
        {
            propertyBlock = new MaterialPropertyBlock();
            particleRenderer.GetPropertyBlock(propertyBlock);

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
        }
        baseScale = transform.localScale;
    }

    public virtual void Initialize(Vector3 startPosition, Vector3 initialVelocity, float life, float scaleMultiplier)
    {
        transform.position = startPosition;
        transform.localScale = baseScale * scaleMultiplier;

        this.velocity = initialVelocity;
        this.lifetime = life;
        this.age = 0f;

        // Define a cor inicial (com alfa máximo do original) no property block
        if (particleRenderer != null && propertyBlock != null)
        {
            Color initialColorWithFullAlpha = originalMaterialColor;
            initialColorWithFullAlpha.a = originalMaterialColor.a; // Usa o alfa original
            propertyBlock.SetColor(mainColorPropertyName, initialColorWithFullAlpha);
            particleRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    protected virtual void Update()
    {
        age += Time.deltaTime;
        transform.Translate(velocity * Time.deltaTime, Space.World);

        UpdateVisuals();
        CheckLifetime();
    }

    protected virtual void UpdateVisuals()
    {
        // Fade out da transparência ao longo da vida
        if (particleRenderer != null && propertyBlock != null && particleRenderer.sharedMaterial.HasProperty(mainColorPropertyName))
        {
            float lifeRatio = Mathf.Clamp01(age / lifetime);
            Color currentColor = originalMaterialColor;
            currentColor.a = Mathf.Lerp(originalMaterialColor.a, 0f, lifeRatio); // Fade do alfa original para 0

            propertyBlock.SetColor(mainColorPropertyName, currentColor);
            particleRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    protected virtual void CheckLifetime()
    {
        if (age >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}