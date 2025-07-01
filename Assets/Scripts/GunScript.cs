using UnityEngine;
using System.Collections;
using TMPro;

public class GunScript : MonoBehaviour
{
    // --- Atributos da Arma (Carregados pela Ficha de Dados) ---
    private float dano;
    private float cadencia;
    private int tamanhoPente;
    private int municaoReservaMax;
    private int projeteisPorTiro;
    private float fatorDeDispersao;
    private float alcance;
    private float tempoDeRecarga;
    private int municaoPorColeta;
    private GameObject hitEffectPrefab;
    private AudioClip somDoTiro;
    private AudioClip somDaRecarga;
    private AnimatorOverrideController animadorDaArma;
    private float duracaoVisualTiro;
    private float duracaoBaseAnimTiro;
    private float duracaoVisualRecarga;
    private float duracaoBaseAnimRecarga;

    // --- Variáveis de estado ---
    private int municaoNoPente;
    private int municaoNaReserva;
    private float proximoTiroDisponivel = 0f;
    private bool isReloading = false;

    [Header("Referências da Cena (Arrastar no Inspector)")]
    public Transform pontoDeDisparo;
    public TextMeshProUGUI textoMunicao;

    private HUDManager hudManager;
    private Camera mainCamera;
    private AudioSource audioSource;

    void Awake()
    {
        hudManager = FindFirstObjectByType<HUDManager>();
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();
    }

    public void CarregarDadosDaArma(WeaponData data)
    {
        this.dano = data.dano;
        this.cadencia = data.cadenciaDeTiro;
        this.tamanhoPente = data.tamanhoDoPente;
        this.municaoReservaMax = data.municaoReservaMax;
        this.alcance = data.alcanceDaArma;
        this.projeteisPorTiro = data.projeteisPorTiro;
        this.fatorDeDispersao = data.fatorDeDispersao;
        this.tempoDeRecarga = data.tempoDeRecarga;
        this.hitEffectPrefab = data.hitEffectPrefab;
        this.municaoPorColeta = data.municaoPorColeta;
        this.somDoTiro = data.somDoTiro;
        this.somDaRecarga = data.somDaRecarga;
        this.animadorDaArma = data.animadorDaArma;
        this.duracaoBaseAnimTiro = data.duracaoBaseAnimTiro;
        this.duracaoVisualTiro = data.duracaoVisualTiro;
        this.duracaoBaseAnimRecarga = data.duracaoBaseAnimRecarga;
        this.duracaoVisualRecarga = data.duracaoVisualRecarga;
        
        if (hudManager != null)
        {
            hudManager.CarregarAnimadorDaArma(this.animadorDaArma);
            if (this.duracaoVisualTiro > 0)
            {
                hudManager.DefinirVelocidadeAnimacao("VelocidadeTiro", this.duracaoBaseAnimTiro / this.duracaoVisualTiro);
            }
            if (this.duracaoVisualRecarga > 0)
            {
                hudManager.DefinirVelocidadeAnimacao("VelocidadeRecarga", this.duracaoBaseAnimRecarga / this.duracaoVisualRecarga);
            }
        }

        this.municaoNoPente = data.tamanhoDoPente;
        this.municaoNaReserva = 0;
        AtualizarUI();
    }

    // --- MÉTODO ProcessarImpacto CORRIGIDO ---
    // A lógica de dano agora volta a diferenciar entre um membro (headshot) e o corpo.
    private void ProcessarImpacto(RaycastHit hitInfo, Vector3 direcaoDoTiro)
    {
        // Efeito de partícula de impacto sempre acontece.
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
        }

        // PRIMEIRO, tenta encontrar um receptor de dano no membro específico que foi atingido.
        EnemyLimbDamageReceiver limbReceiver = hitInfo.collider.GetComponent<EnemyLimbDamageReceiver>();

        if (limbReceiver != null)
        {
            // Se encontrou, o membro sabe como lidar com o dano (aplicar multiplicadores, etc.).
            // A direção do dano é o oposto da direção do tiro.
            limbReceiver.ReceiveHit(dano, hitInfo.point, -direcaoDoTiro.normalized);
        }
        else
        {
            // SE NÃO encontrou um receptor no membro, volta para a lógica antiga de procurar
            // um IDamageable geral no objeto (para o corpo do zumbi ou para caixas).
            IDamageable damageableObject = hitInfo.collider.GetComponentInParent<IDamageable>();
            if (damageableObject != null)
            {
                // Passa o tipo de acerto como "BodyShot" por padrão se não for um membro específico.
                damageableObject.TakeDamage(dano, hitInfo.point, -direcaoDoTiro.normalized, HitType.BodyShot);
            }
        }
    }

    // O resto do script (Update, FireHitscan, Reload, etc.) permanece o mesmo.
    // Incluído abaixo para que você possa copiar e colar tudo de uma vez.
    #region Métodos Inalterados
    void OnEnable()
    {
        isReloading = false;
        AtualizarUI();
    }
    void Update()
    {
        if (isReloading) return;
        if (Input.GetButton("Fire1") && Time.time >= proximoTiroDisponivel) AttemptToFire();
        if (Input.GetKeyDown(KeyCode.R) && municaoNoPente < tamanhoPente && municaoNaReserva > 0) StartCoroutine(Reload());
    }
    private void AttemptToFire()
    {
        if (municaoNoPente > 0)
        {
            proximoTiroDisponivel = Time.time + cadencia;
            FireHitscan();
        }
        else if (municaoNaReserva > 0)
        {
            StartCoroutine(Reload());
        }
    }
    private void FireHitscan()
    {
        municaoNoPente--;
        AtualizarUI();

        if (audioSource != null && somDoTiro != null) audioSource.PlayOneShot(somDoTiro);
        if (hudManager != null) hudManager.PlayAnimacaoTiro();
        
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        for (int i = 0; i < projeteisPorTiro; i++)
        {
            Vector3 direcaoDoTiro = ray.direction;
            if (fatorDeDispersao > 0)
            {
                Vector2 circuloDispersao = Random.insideUnitCircle * fatorDeDispersao;
                direcaoDoTiro += mainCamera.transform.up * circuloDispersao.y + mainCamera.transform.right * circuloDispersao.x;
            }
            RaycastHit hitInfo;
            if (Physics.Raycast(ray.origin, direcaoDoTiro, out hitInfo, alcance))
            {
                ProcessarImpacto(hitInfo, direcaoDoTiro);
            }
        }
    }
    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Recarregando...");
        if (audioSource != null && somDaRecarga != null) audioSource.PlayOneShot(somDaRecarga);
        if (hudManager != null) hudManager.PlayAnimacaoRecarga();
        yield return new WaitForSeconds(this.tempoDeRecarga);
        int ammoNeeded = tamanhoPente - municaoNoPente;
        int ammoToMove = Mathf.Min(ammoNeeded, municaoNaReserva);
        municaoNoPente += ammoToMove;
        municaoNaReserva -= ammoToMove;
        AtualizarUI();
        isReloading = false;
    }
    public void AddAmmo()
    {
        municaoNaReserva += this.municaoPorColeta;
        municaoNaReserva = Mathf.Min(municaoNaReserva, municaoReservaMax);
        AtualizarUI();
    }
    private void AtualizarUI()
    {
        if (textoMunicao != null)
        {
            textoMunicao.text = municaoNoPente + " / " + municaoNaReserva;
        }
    }
    #endregion
}