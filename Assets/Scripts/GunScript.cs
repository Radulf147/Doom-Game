using UnityEngine;
using System.Collections;
using TMPro;

public class GunScript : MonoBehaviour
{
    // --- Atributos da Arma ---
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
    private float espacamentoVertical;
    private bool characterHasMultiKillAbility = false;

    // --- Variáveis de estado ---
    private int municaoNoPente;
    private int municaoNaReserva;
    private float proximoTiroDisponivel = 0f;
    private bool isReloading = false;

    [Header("Referências da Cena")]
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
        this.espacamentoVertical = data.espacamentoVertical;
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
            if (this.duracaoVisualTiro > 0) hudManager.DefinirVelocidadeAnimacao("VelocidadeTiro", this.duracaoBaseAnimTiro / this.duracaoVisualTiro);
            if (this.duracaoVisualRecarga > 0) hudManager.DefinirVelocidadeAnimacao("VelocidadeRecarga", this.duracaoBaseAnimRecarga / this.duracaoVisualRecarga);
        }

        this.municaoNoPente = data.tamanhoDoPente;
        this.municaoNaReserva = data.municaoReservaMax;
        AtualizarUI();
    }

    private void FireHitscan()
    {
        Debug.Log("<color=#FFFFFF><b>-- CHECKPOINT 1: FireHitscan INICIADO --</b></color>"); // BRANCO
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
            else if (espacamentoVertical > 0)
            {
                float centroDoArco = (projeteisPorTiro - 1) / 2.0f;
                float offsetVertical = (i - centroDoArco) * espacamentoVertical;
                direcaoDoTiro += mainCamera.transform.up * offsetVertical;
            }

            RaycastHit hitInfo;
            if (Physics.Raycast(ray.origin, direcaoDoTiro, out hitInfo, alcance))
            {
                bool targetDied = ProcessarImpacto(hitInfo, direcaoDoTiro);
                Debug.Log($"<color=#666666><i><-- CHECKPOINT 6: FireHitscan recebeu de volta '{targetDied}'.</i></color>"); // CINZA
                if (targetDied)
                {
                    killsThisShot++;
                }
            }
        }
        
        Debug.Log($"<color=#00FF00><b>-- CHECKPOINT 7: CONTAGEM FINAL. Kills: {killsThisShot} | Habilidade Ativa: {characterHasMultiKillAbility} --</b></color>"); // VERDE

        if (characterHasMultiKillAbility && killsThisShot > 1)
        {
            Debug.Log("<color=#00FF00><b>--> SUCESSO! Adicionando Shield!</b></color>");
            PlayerHealth playerHealth = GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.AddShield(10);
            }
            else
            {
                Debug.LogError("Falha Crítica: Não foi possível encontrar PlayerHealth para adicionar o shield!");
            }
        }
    }
    
    private bool ProcessarImpacto(RaycastHit hitInfo, Vector3 direcaoDoTiro)
    {
        Debug.Log($"<color=#00FFFF><b>-- CHECKPOINT 2: ProcessarImpacto CHAMADO para '{hitInfo.collider.name}' --</b></color>"); // CIANO
        
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
        }

        bool morreu = false;
        EnemyLimbDamageReceiver limbReceiver = hitInfo.collider.GetComponent<EnemyLimbDamageReceiver>();
        if (limbReceiver != null)
        {
            morreu = limbReceiver.ReceiveHit(dano, hitInfo.point, -direcaoDoTiro.normalized);
            if (morreu && ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScoreForEnemyKill(limbReceiver.limbHitType);
            }
        }
        else
        {
            IDamageable damageableObject = hitInfo.collider.GetComponentInParent<IDamageable>();
            if (damageableObject != null)
            {
                morreu = damageableObject.TakeDamage(dano, hitInfo.point, -direcaoDoTiro.normalized, HitType.BodyShot);
                if (morreu && ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScoreForEnemyKill(HitType.BodyShot);
                }
            }
            else
            {
                 Debug.LogWarning($"Objeto '{hitInfo.collider.name}' foi atingido mas não tem nenhum script de dano (LimbReceiver ou IDamageable).");
            }
        }
        return morreu;
    }

    public void SetCharacterAbilities(CharacterData charData)
    {
        this.characterHasMultiKillAbility = charData.hasMultiKillShieldAbility;
    }

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
}