using UnityEngine;
using System.Collections.Generic;

public class MeleeScript : MonoBehaviour
{
    // --- Atributos da Arma (Carregados pela Ficha) ---
    private float dano;
    private float cadencia; // Sim, armas brancas também têm cadência (velocidade de ataque)
    
    // --- Controle de Estado ---
    private float proximoAtaqueDisponivel = 0f;

    [Header("Configuração do Ataque Corpo a Corpo")]
    [Tooltip("Um objeto vazio na frente da câmera que marca o centro da área de ataque.")]
    public Transform pontoDeAtaque;
    [Tooltip("O tamanho (largura, altura, profundidade) da caixa de dano.")]
    public Vector3 tamanhoDaAreaDeAtaque = new Vector3(1, 1, 2);
    [Tooltip("A camada (Layer) em que os inimigos estão. Evita acertar o cenário.")]
    public LayerMask camadaDoInimigo;

    private HUDManager hudManager;
    private AudioSource audioSource;
    private AudioClip somDoAtaque;

    void Awake()
    {
        hudManager = FindFirstObjectByType<HUDManager>();
        audioSource = GetComponent<AudioSource>();
    }

    // Assim como o GunScript, ele carrega os dados da ficha
    public void CarregarDadosDaArma(WeaponData data)
    {
        this.dano = data.dano;
        this.cadencia = data.cadenciaDeTiro;
        this.somDoAtaque = data.somDoTiro; // Reutilizamos o campo de som de tiro

        if (hudManager != null)
        {
            hudManager.CarregarAnimadorDaArma(data.animadorDaArma);
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.time >= proximoAtaqueDisponivel)
        {
            Atacar();
        }
    }

    private void Atacar()
    {
        proximoAtaqueDisponivel = Time.time + cadencia;

        // Toca a animação e o som do corte
        if (hudManager != null) hudManager.PlayAnimacaoTiro(); // Reutilizamos o gatilho "Tiro"
        if (audioSource != null && somDoAtaque != null) audioSource.PlayOneShot(somDoAtaque);

        // --- A LÓGICA DE DETECÇÃO DE ÁREA ---
        // Cria uma "caixa" invisível na frente do jogador
        Collider[] alvos = Physics.OverlapBox(pontoDeAtaque.position, tamanhoDaAreaDeAtaque / 2, pontoDeAtaque.rotation, camadaDoInimigo);

        if (alvos.Length == 0) return;

        Debug.Log("Katana acertou " + alvos.Length + " alvos!");

        // Aplica dano a todos os inimigos dentro da caixa
        foreach (Collider alvo in alvos)
        {
            IDamageable damageableObject = alvo.GetComponentInParent<IDamageable>();
            if (damageableObject != null)
            {
                // Para simplificar, o dano é aplicado no corpo, mas você pode refinar isso.
                damageableObject.TakeDamage(dano, alvo.transform.position, transform.forward, HitType.BodyShot);
            }
        }
    }

    // Para visualizar a caixa de ataque no Editor da Unity, descomente este método
    
    private void OnDrawGizmosSelected()
    {
        if (pontoDeAtaque == null) return;
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(pontoDeAtaque.position, pontoDeAtaque.rotation, tamanhoDaAreaDeAtaque);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
    
}