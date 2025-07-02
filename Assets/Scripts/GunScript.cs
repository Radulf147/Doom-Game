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

    private bool characterHasMultiKillAbility = false;

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

        // >>>>> A CORREÇÃO ESTÁ AQUI <<<<<
        this.municaoNoPente = data.tamanhoDoPente;
        this.municaoNaReserva = data.municaoReservaMax; // Agora lê o valor máximo da reserva da ficha.
        AtualizarUI();
    }


    public void SetCharacterAbilities(CharacterData charData)
    {
        this.characterHasMultiKillAbility = charData.hasMultiKillShieldAbility;
    }

    // --- MÉTODO AddAmmo CORRIGIDO ---
    // Ele não precisa mais saber o tipo ou a quantidade, pois esses dados agora estão na própria arma.
    public void AddAmmo()
    {
        municaoNaReserva += this.municaoPorColeta;
        municaoNaReserva = Mathf.Min(municaoNaReserva, municaoReservaMax);
        AtualizarUI();
        Debug.Log("Pegou " + this.municaoPorColeta + " de munição. Reserva atual: " + municaoNaReserva);
    }

    // O resto do script permanece o mesmo

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
        int killsThisShot = 0;
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
                bool targetDied = ProcessarImpacto(hitInfo, direcaoDoTiro);
                if (targetDied)
                {
                    killsThisShot++;
                }
            }
        }
        if (characterHasMultiKillAbility && killsThisShot > 1)
        {
            PlayerHealth playerHealth = GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.AddShield(10);
            }
        }
    }
    private bool ProcessarImpacto(RaycastHit hitInfo, Vector3 direcaoDoTiro)
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
        }
        EnemyLimbDamageReceiver limbReceiver = hitInfo.collider.GetComponent<EnemyLimbDamageReceiver>();
        if (limbReceiver != null)
        {
            return limbReceiver.ReceiveHit(dano, hitInfo.point, -direcaoDoTiro.normalized);
        }
        else
        {
            IDamageable damageableObject = hitInfo.collider.GetComponentInParent<IDamageable>();
            if (damageableObject != null)
            {
                return damageableObject.TakeDamage(dano, hitInfo.point, -direcaoDoTiro.normalized, HitType.BodyShot);
            }
        }
        return false;
    }


    IEnumerator Reload()
    {
        isReloading = true;
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
    private void AtualizarUI()
    {
        if (textoMunicao != null)
        {
            textoMunicao.text = municaoNoPente + " / " + municaoNaReserva;
        }
    }
    #endregion
}