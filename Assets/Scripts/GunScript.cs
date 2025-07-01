using UnityEngine;
using System.Collections;
using TMPro;

public class GunScript : MonoBehaviour
{
    // --- Atributos da Arma (Carregados pela Ficha de Dados) ---
    private AmmoPickup.AmmoType weaponAmmoType;
    private float dano;
    private float cadencia;
    private int tamanhoPente;
    private int municaoReservaMax;
    private int projeteisPorTiro;
    private float fatorDeDispersao;
    private float alcance;
    private float tempoDeRecarga;
    private GameObject hitEffectPrefab;
    private AudioClip somDoTiro;
    private AudioClip somDaRecarga;

    // Variáveis de estado
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

    // Este método está correto e não precisa de alterações.
    public void CarregarDadosDaArma(WeaponData data)
    {
        this.weaponAmmoType = data.weaponAmmoType;
        this.dano = data.dano;
        this.cadencia = data.cadenciaDeTiro;
        this.tamanhoPente = data.tamanhoDoPente;
        this.municaoReservaMax = data.municaoReservaMax;
        this.alcance = data.alcanceDaArma;
        this.projeteisPorTiro = data.projeteisPorTiro;
        this.fatorDeDispersao = data.fatorDeDispersao;
        this.tempoDeRecarga = data.tempoDeRecarga;
        this.hitEffectPrefab = data.hitEffectPrefab;

        this.somDoTiro = data.somDoTiro;
        this.somDaRecarga = data.somDaRecarga;
        
        if (hudManager != null)
        {
            hudManager.CarregarAnimadorDaArma(data.animadorDaArma);

            if (data.duracaoVisualTiro > 0)
            {
                float velocidadeTiro = data.duracaoBaseAnimTiro / data.duracaoVisualTiro;
                hudManager.DefinirVelocidadeAnimacao("VelocidadeTiro", velocidadeTiro);
            }
            if (data.duracaoVisualRecarga > 0)
            {
                float velocidadeRecarga = data.duracaoBaseAnimRecarga / data.duracaoVisualRecarga;
                hudManager.DefinirVelocidadeAnimacao("VelocidadeRecarga", velocidadeRecarga);
            }
        }

        this.municaoNoPente = data.tamanhoDoPente;
        this.municaoNaReserva = data.municaoReservaMax;
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

        if (audioSource != null && somDoTiro != null)
        {
            audioSource.PlayOneShot(somDoTiro);
        }

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
    
    private void ProcessarImpacto(RaycastHit hitInfo, Vector3 direcaoDoTiro)
    {
        Vector3 hitPoint = hitInfo.point;
        Vector3 incomingHitDirection = -direcaoDoTiro.normalized;
        EnemyLimbDamageReceiver limbDamageReceiver = hitInfo.collider.GetComponent<EnemyLimbDamageReceiver>();
        if (limbDamageReceiver != null)
        {
            limbDamageReceiver.ReceiveHit(dano, hitPoint, incomingHitDirection);
        }
        else
        {
            IDamageable damageableObject = hitInfo.collider.GetComponentInParent<IDamageable>();
            if (damageableObject != null)
            {
                damageableObject.TakeDamage(dano, hitPoint, incomingHitDirection, HitType.BodyShot);
            }
        }
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hitPoint, Quaternion.LookRotation(hitInfo.normal));
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Recarregando...");
        
        // CORREÇÃO 4: Lógica para tocar o som de recarga está de volta
        if (audioSource != null && somDaRecarga != null)
        {
            audioSource.PlayOneShot(somDaRecarga);
        }

        if (hudManager != null) 
        {
            hudManager.PlayAnimacaoRecarga();
        }
        
        yield return new WaitForSeconds(this.tempoDeRecarga);

        int ammoNeeded = tamanhoPente - municaoNoPente;
        int ammoToMove = Mathf.Min(ammoNeeded, municaoNaReserva);

        municaoNoPente += ammoToMove;
        municaoNaReserva -= ammoToMove;
        AtualizarUI();
        isReloading = false;
    }
    public void AddAmmo(int quantidade, AmmoPickup.AmmoType tipoDaMunicaoRecebida)
    {
        if (tipoDaMunicaoRecebida == this.weaponAmmoType)
        {
            municaoNaReserva += quantidade;
            municaoNaReserva = Mathf.Min(municaoNaReserva, municaoReservaMax);
            AtualizarUI();
        }
    }

    private void AtualizarUI()
    {
        if (textoMunicao != null)
        {
            textoMunicao.text = municaoNoPente + " / " + municaoNaReserva;
        }
    }
}