using UnityEngine;
using TMPro;

public class GunScript : MonoBehaviour
{
    // --- Atributos da Arma (Carregados pela Ficha de Dados) ---
    private AmmoPickup.AmmoType weaponAmmoType;
    private GameObject projetilPrefab;
    private float danoDoProjetil;
    private float cadencia;
    private int tamanhoPente;
    private int municaoNaReserva;
    private int municaoReservaMax;
    private int municaoNoPente;

    // NOVOS ATRIBUTOS
    private float alcanceDoProjetil;
    private int projeteisPorTiro;
    private float fatorDeDispersao;

    // ... resto das suas variáveis de controle ...
    private float proximoTiroDisponivel = 0f;
    
    [Header("Referências da Cena (Arrastar no Inspector)")]
    public Transform pontoDeDisparo;
    public TextMeshProUGUI textoMunicao;

    private HUDManager hudManager;

    void Start()
    {
        hudManager = FindObjectOfType<HUDManager>();
    }

    // ATUALIZADO: Carregar as novas propriedades
    public void CarregarDadosDaArma(WeaponData data)
    {
        this.weaponAmmoType = data.weaponAmmoType;
        this.projetilPrefab = data.projetilPrefab;
        this.danoDoProjetil = data.danoDoProjetil;
        this.cadencia = data.cadenciaDeTiro;
        this.tamanhoPente = data.tamanhoDoPente;
        this.municaoReservaMax = data.municaoReservaMax;

        // CARREGANDO OS NOVOS DADOS
        this.alcanceDoProjetil = data.alcanceDoProjetil;
        this.projeteisPorTiro = data.projeteisPorTiro;
        this.fatorDeDispersao = data.fatorDeDispersao;

        this.municaoNoPente = this.tamanhoPente;
        this.municaoNaReserva = this.municaoReservaMax;
        AtualizarUI();
    }

    // ... seu método Update() continua igual ...
    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= proximoTiroDisponivel)
        {
            Atirar();
        }
    }


    // MÉTODO ATIRAR COMPLETAMENTE REFEITO
    private void Atirar()
    {
        if (municaoNoPente <= 0 || projetilPrefab == null)
        {
            return;
        }

        proximoTiroDisponivel = Time.time + cadencia;
        municaoNoPente--;

        // Toca a animação e o som uma vez por clique
        if(hudManager != null) hudManager.PlayAnimacaoTiro();
        // Adicione aqui a lógica do som do tiro

        // Loop para criar múltiplos projéteis (se for uma espingarda)
        for (int i = 0; i < projeteisPorTiro; i++)
        {
            // Calcula a dispersão (spread) para cada projétil
            Vector3 direcaoDoTiro = pontoDeDisparo.forward;
            Vector3 dispersao = new Vector3(
                Random.Range(-fatorDeDispersao, fatorDeDispersao),
                Random.Range(-fatorDeDispersao, fatorDeDispersao),
                0
            );
            // Rotaciona a dispersão para alinhar com a mira do jogador
            dispersao = pontoDeDisparo.TransformDirection(dispersao);
            
            // Instancia o projétil
            GameObject projetil = Instantiate(
                projetilPrefab,
                pontoDeDisparo.position,
                Quaternion.LookRotation(direcaoDoTiro + dispersao) // Aplica a dispersão na rotação
            );
            
            // Passa a informação de dano E ALCANCE para o projétil
            if (projetil.GetComponent<ProjectileController>() != null)
            {
                projetil.GetComponent<ProjectileController>().Inicializar(danoDoProjetil, alcanceDoProjetil);
            }
        }
        
        AtualizarUI();
    }

    // ... seu método AddAmmo() e AtualizarUI() continuam iguais ...
    public void AddAmmo(int quantidade, AmmoPickup.AmmoType tipoDaMunicaoRecebida)
    {
        if (tipoDaMunicaoRecebida == this.weaponAmmoType)
        {
            municaoNaReserva += quantidade;
            if (municaoNaReserva > municaoReservaMax)
                municaoNaReserva = municaoReservaMax;
            AtualizarUI();
        }
    }

    private void AtualizarUI()
    {
        if (textoMunicao != null)
            textoMunicao.text = municaoNoPente + " / " + municaoNaReserva;
    }
}