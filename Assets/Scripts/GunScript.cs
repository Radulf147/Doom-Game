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
    private float reloadTime;
    private GameObject hitEffectPrefab;

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

    void Awake()
    {
        hudManager = FindFirstObjectByType<HUDManager>();
        mainCamera = Camera.main;
    }

    public void CarregarDadosDaArma(WeaponData data)
    {
        this.weaponAmmoType = data.weaponAmmoType;
        this.dano = data.danoDoProjetil;
        this.cadencia = data.cadenciaDeTiro;
        this.tamanhoPente = data.tamanhoDoPente;
        this.municaoReservaMax = data.municaoReservaMax;
        this.alcance = data.alcanceDaArma;
        this.projeteisPorTiro = data.projeteisPorTiro;
        this.fatorDeDispersao = data.fatorDeDispersao;
        this.reloadTime = data.reloadTime;
        this.hitEffectPrefab = data.hitEffectPrefab;

        // MODIFICAÇÃO 1: Passa o controlador de animação da ficha para a HUD.
        if (hudManager != null)
        {
            hudManager.CarregarAnimadorDaArma(data.animadorDaArma);
        }

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

        if (Input.GetButton("Fire1") && Time.time >= proximoTiroDisponivel)
        {
            AttemptToFire();
        }

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
        else
        {
            if (Time.time >= proximoTiroDisponivel)
            {
                proximoTiroDisponivel = Time.time + cadencia;
                if (municaoNaReserva > 0)
                {
                    StartCoroutine(Reload());
                }
            }
        }
    }

    private void FireHitscan()
    {
        municaoNoPente--;
        AtualizarUI();
        Debug.Log("GUNSCRIPT: Pedindo para o HUDManager tocar a animação de tiro...");
        if (hudManager != null) hudManager.PlayAnimacaoTiro(); // Esta chamada já estava correta

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
        
        // MODIFICAÇÃO 2: Pede para a HUD tocar a animação de recarga.
        if (hudManager != null) hudManager.PlayAnimacaoRecarga();

        yield return new WaitForSeconds(reloadTime);

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