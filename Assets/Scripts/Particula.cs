using UnityEngine;

public class Particula : MonoBehaviour // Certifique-se que o nome da classe é Particula
{
    public Vector3 velocity;
    public float lifetime;
    private float age = 0f;
    private float groundHitY; // Altura Y para a partícula sumir

    // Método para ser chamado pelo Emissor
    public void Initialize(Vector3 startPosition, Vector3 initialVelocity, float life, float groundLevel)
    {
        transform.position = startPosition;
        this.velocity = initialVelocity;
        this.lifetime = life;
        this.age = 0f;
        this.groundHitY = groundLevel; // Guarda o nível do chão
    }

    void Update()
    {
        age += Time.deltaTime;

        // Move a partícula
        transform.Translate(velocity * Time.deltaTime, Space.World);

        // Se passou do tempo de vida OU atingiu o nível do chão/abaixo, destrói
        if (age >= lifetime || transform.position.y <= groundHitY)
        {
            Destroy(gameObject); // Para um sistema otimizado, aqui seria Deactivate() e retornar ao pool
            return;
        }
    }
}