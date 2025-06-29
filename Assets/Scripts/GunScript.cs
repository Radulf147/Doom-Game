// GunScript.cs
using UnityEngine;
using System.Collections;
using TMPro;

public class GunScript : MonoBehaviour
{
    // --- Atributos da Arma (Carregados pela Ficha de Dados) ---
    private AmmoPickup.AmmoType weaponAmmoType;
    private float dano; // Este é o DANO BASE da arma (sem multiplicadores de headshot)
    private float cadencia;
    private int tamanhoPente;
    private int municaoReservaMax;
    private int projeteisPorTiro;
    private float fatorDeDispersao;
    private float alcance;
    private float reloadTime;

    // Prefabs de Efeitos (arrastar no Inspector via WeaponData)
    private GameObject hitEffectPrefab;

    // Variáveis de estado
    private int municaoNoPente;
    private int municaoNaReserva;
    private float proximoTiroDisponivel = 0f;
    private bool isReloading = false;

    [Header("Referências da Cena (Arrastar no Inspector)")]
    public Transform pontoDeDisparo; // Ponto de onde os projéteis 'são disparados' visualmente
    public TextMeshProUGUI textoMunicao; // Texto para exibir munição na UI

    private HUDManager hudManager; // Referência ao seu HUDManager
    private Camera mainCamera; // A câmera principal do jogador

    void Awake()
    {
        hudManager = FindFirstObjectByType<HUDManager>();
        mainCamera = Camera.main; // Garanta que sua câmera de jogador tenha a tag "MainCamera"
    }

    // Método para carregar os dados da arma (chamado ao equipar a arma)
    public void CarregarDadosDaArma(WeaponData data)
    {
        this.weaponAmmoType = data.weaponAmmoType;
        this.dano = data.danoDoProjetil; // Dano BASE
        this.cadencia = data.cadenciaDeTiro;
        this.tamanhoPente = data.tamanhoDoPente;
        this.municaoReservaMax = data.municaoReservaMax;
        this.alcance = data.alcanceDaArma;
        this.projeteisPorTiro = data.projeteisPorTiro;
        this.fatorDeDispersao = data.fatorDeDispersao;
        this.reloadTime = data.reloadTime;
        this.hitEffectPrefab = data.hitEffectPrefab;
        this.municaoNoPente = this.tamanhoPente;
        this.municaoNaReserva = this.municaoReservaMax;
        AtualizarUI();
    }

    void OnEnable()
    {
        isReloading = false;
        AtualizarUI();
    }

    void Update()
    {
        if (isReloading) return;

        // Lógica de disparo (botão esquerdo do mouse por padrão)
        if (Input.GetButton("Fire1") && Time.time >= proximoTiroDisponivel)
        {
            AttemptToFire();
        }

        // Lógica de recarga (tecla R por padrão)
        if (Input.GetKeyDown(KeyCode.R) && municaoNoPente < tamanhoPente && municaoNaReserva > 0)
        {
            StartCoroutine(Reload());
        }
    }

    private void AttemptToFire()
    {
        if (municaoNoPente > 0)
        {
            FireHitscan();
            proximoTiroDisponivel = Time.time + cadencia;
        }
        else // Sem munição no pente
        {
            // Tenta recarregar automaticamente se possível
            if (Time.time >= proximoTiroDisponivel)
            {
                proximoTiroDisponivel = Time.time + cadencia; // Evita spam de tentativa de recarga
                if (municaoNaReserva > 0)
                {
                    StartCoroutine(Reload());
                }
            }
        }
    }

    // Simula um tiro hitscan (instantâneo)
    private void FireHitscan()
    {
        municaoNoPente--;
        AtualizarUI();

        if (hudManager != null) hudManager.PlayAnimacaoTiro(); // Exemplo de efeito visual/sonoro da HUD

        // Cria um raio do centro da tela para onde a câmera está apontando
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // Para armas de múltiplos projéteis (escopetas, por exemplo)
        for (int i = 0; i < projeteisPorTiro; i++)
        {
            Vector3 direcaoDoTiro = ray.direction;

            // Adiciona dispersão (spread) se configurado
            if (fatorDeDispersao > 0)
            {
                Vector2 circuloDispersao = Random.insideUnitCircle * fatorDeDispersao;
                direcaoDoTiro += mainCamera.transform.up * circuloDispersao.y + mainCamera.transform.right * circuloDispersao.x;
            }

            RaycastHit hitInfo;
            // Realiza o Raycast. Opcional: Adicionar um LayerMask para otimizar colisões.
            if (Physics.Raycast(ray.origin, direcaoDoTiro, out hitInfo, alcance))
            {
                ProcessarImpacto(hitInfo, direcaoDoTiro);
            }
        }
    }

    // Processa o resultado do impacto do Raycast
    private void ProcessarImpacto(RaycastHit hitInfo, Vector3 direcaoDoTiro)
    {
        Vector3 hitPoint = hitInfo.point;
        // A direção de onde o tiro *veio* (oposta à direção do tiro) é útil para a emissão de sangue, etc.
        Vector3 incomingHitDirection = -direcaoDoTiro.normalized; 

        // 1. Tenta obter o EnemyLimbDamageReceiver no colisor atingido
        // Isso é para inimigos com partes de dano localizado (cabeça, corpo, etc.)
        EnemyLimbDamageReceiver limbDamageReceiver = hitInfo.collider.GetComponent<EnemyLimbDamageReceiver>();

        if (limbDamageReceiver != null)
        {
            // Se encontrou, chame o método ReceiveHit dele, passando o dano BASE da arma.
            // O limbDamageReceiver se encarregará de aplicar seu próprio multiplicador e passar para o EnemyNavigation.
            limbDamageReceiver.ReceiveHit(dano, hitPoint, incomingHitDirection);
        }
        else
        {
            // 2. Se não encontrou um EnemyLimbDamageReceiver, tenta encontrar um IDamageable
            //    no próprio objeto atingido ou em um de seus pais.
            //    Isso é um fallback para objetos destrutíveis gerais que não têm partes específicas de dano
            //    (ex: um barril explosivo, uma caixa, etc., que podem ter um script IDamageable diretamente).
            IDamageable damageableObject = hitInfo.collider.GetComponentInParent<IDamageable>();
            if (damageableObject != null)
            {
                // Se for um objeto IDamageable genérico, aplica o dano BASE da arma.
                // Passa HitType.BodyShot como o tipo de acerto padrão para tiros de arma.
                damageableObject.TakeDamage(dano, hitPoint, incomingHitDirection, HitType.BodyShot);
            }
            else
            {
                // Opcional: Debug para saber o que foi atingido caso não seja um alvo damageable.
                // Debug.Log("Acertou: " + hitInfo.collider.name + " (Não é um inimigo ou objeto IDamageable)");
            }
        }

        // Instancia o efeito de impacto (buraco de bala, faíscas, etc.) onde o tiro acertou
        if (hitEffectPrefab != null)
        {
            // Quaternion.LookRotation(hitInfo.normal) faz com que o efeito se alinhe com a superfície atingida
            Instantiate(hitEffectPrefab, hitPoint, Quaternion.LookRotation(hitInfo.normal));
        }
    }

    // Corrotina para simular o tempo de recarga
    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Recarregando...");
        // Opcional: Adicione aqui uma animação ou som de recarga
        yield return new WaitForSeconds(reloadTime);

        int ammoNeeded = tamanhoPente - municaoNoPente;
        int ammoToMove = Mathf.Min(ammoNeeded, municaoNaReserva); // Pega o mínimo entre o que precisa e o que tem na reserva

        municaoNoPente += ammoToMove;
        municaoNaReserva -= ammoToMove;
        AtualizarUI();
        isReloading = false;
        // Opcional: Adicione aqui um som de recarga completa
    }

    // Adiciona munição à reserva (chamado por pickups de munição)
    public void AddAmmo(int quantidade, AmmoPickup.AmmoType tipoDaMunicaoRecebida)
    {
        if (tipoDaMunicaoRecebida == this.weaponAmmoType)
        {
            municaoNaReserva += quantidade;
            municaoNaReserva = Mathf.Min(municaoNaReserva, municaoReservaMax); // Garante que não excede o máximo
            AtualizarUI();
        }
    }

    // Atualiza o texto da UI que mostra a munição
    private void AtualizarUI()
    {
        if (textoMunicao != null)
        {
            textoMunicao.text = municaoNoPente + " / " + municaoNaReserva;
        }
    }
}