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

    // Prefabs de Efeitos
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
        // CORREÇÃO DO AVISO: Usando o método mais novo para encontrar o HUDManager
        hudManager = FindFirstObjectByType<HUDManager>();
        mainCamera = Camera.main;
    }

    // O restante dos seus métodos (CarregarDadosDaArma, OnEnable, Update, Reload, etc.)
    // permanecem os mesmos que na versão anterior. O problema principal está
    // na forma como FireHitscan e ProcessarImpacto se comunicam.
    // ... (CarregarDadosDaArma, OnEnable, Update, AttemptToFire, Reload) ...

    private void FireHitscan()
    {
        municaoNoPente--;
        AtualizarUI();

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
                // CORREÇÃO: Passamos a 'direcaoDoTiro' para o método ProcessarImpacto
                ProcessarImpacto(hitInfo, direcaoDoTiro);
            }
        }
    }

    // --- MÉTODO ProcessarImpacto CORRIGIDO ---
    // Agora ele também recebe a 'direcaoDoTiro' como parâmetro
    private void ProcessarImpacto(RaycastHit hitInfo, Vector3 direcaoDoTiro)
    {
        Vector3 hitPoint = hitInfo.point;
        Vector3 hitNormal = hitInfo.normal;

        // Efeito de partícula de impacto
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
        }

        // Lógica de dano universal usando a interface IDamageable
        IDamageable damageableObject = hitInfo.collider.GetComponentInParent<IDamageable>();
        if (damageableObject != null)
        {
            // CORREÇÃO: Agora passamos todos os 3 argumentos exigidos pela interface
            damageableObject.TakeDamage(dano, hitPoint, direcaoDoTiro);
        }
    }

    // ... (O resto dos seus métodos como AddAmmo, AtualizarUI, etc. continuam aqui) ...
    #region Métodos Inalterados
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
    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Recarregando...");
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
    #endregion
}