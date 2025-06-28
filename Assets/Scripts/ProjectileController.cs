using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float velocidade = 50f;
    private float danoDoTiro;
    private float alcanceMaximo;
    
    // CORREÇÃO: Declaramos a variável aqui, no nível da classe.
    private Vector3 pontoInicial;

    // ATUALIZADO: O método Inicializar agora também recebe o alcance
    public void Inicializar(float dano, float alcance)
    {
        this.danoDoTiro = dano;
        this.alcanceMaximo = alcance;
    }

    void Start()
    {
        // Agora, aqui dentro, nós apenas ATRIBUÍMOS o valor à variável que já existe.
        pontoInicial = transform.position;

        GetComponent<Rigidbody>().linearVelocity = transform.forward * velocidade;
    }

    void Update()
    {
        // Agora o Update() consegue acessar a variável 'pontoInicial' sem problemas.
        if (Vector3.Distance(pontoInicial, transform.position) >= alcanceMaximo)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Inimigo"))
        {
            // Tenta encontrar o script de vida do inimigo e aplicar dano
            EnemyHealth vidaInimigo = other.gameObject.GetComponent<EnemyHealth>();
            if (vidaInimigo != null)
            {
                vidaInimigo.TomarDano(danoDoTiro);
            }
        }
        
        // Independentemente do que atingiu, o projétil se destrói
        Destroy(gameObject);
    }
}