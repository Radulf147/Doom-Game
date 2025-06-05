using UnityEngine;

[ExecuteAlways] // Faz o script rodar no editor e no jogo
[RequireComponent(typeof(EnemyNavigation))]
public class RangeVisualizer : MonoBehaviour
{
    private EnemyNavigation enemyNavigation;

    [Header("Configurações Gerais dos Círculos")]
    [Tooltip("Número de segmentos para um círculo completo de 360 graus.")]
    public int segmentsForFullCircle = 36;
    public float circleLineWidth = 0.1f;
    public Material lineMaterial; // Material para todos os LineRenderers

    [Header("Visualização dos Ranges de Detecção")]
    [Tooltip("Marque para exibir os círculos de visualização no editor e no jogo.")]
    public bool displayVisualizers = true;
    // As cores são definidas em GetCurrentDetectionColor e para o loseChaseRange diretamente

    [Header("Visualização do Range de Ataque")]
    public Color attackRangeColor = Color.yellow;
    // O número de segmentos para o arco de ataque será proporcional a segmentsForFullCircle

    private LineRenderer detectionRangeLineRenderer;
    private LineRenderer loseChaseRangeLineRenderer;
    private LineRenderer attackArcLineRenderer; // Novo LineRenderer para o ataque

    // Nomes dos GameObjects filhos para os visualizadores
    private const string DetectionCircleGOName = "DetectionRange_Visualizer_Child";
    private const string LoseChaseCircleGOName = "LoseChaseRange_Visualizer_Child";
    private const string AttackArcGOName = "AttackArc_Visualizer_Child"; // Novo nome

    void OnEnable()
    {
        enemyNavigation = GetComponent<EnemyNavigation>();
        if (enemyNavigation == null)
        {
            Debug.LogError("EnemyNavigation não encontrado neste GameObject. RangeVisualizer não funcionará.", this);
            enabled = false;
            return;
        }
        SetupVisualizers();
    }

    void OnDisable()
    {
        CleanupVisualizer(ref detectionRangeLineRenderer, DetectionCircleGOName);
        CleanupVisualizer(ref loseChaseRangeLineRenderer, LoseChaseCircleGOName);
        CleanupVisualizer(ref attackArcLineRenderer, AttackArcGOName); // Limpa o novo visualizador
    }

    void SetupVisualizers()
    {
        // Os parâmetros de cor e raio são definidos em UpdateVisuals
        detectionRangeLineRenderer = FindOrCreateLineRenderer(DetectionCircleGOName, true); // true para loop (círculo)
        loseChaseRangeLineRenderer = FindOrCreateLineRenderer(LoseChaseCircleGOName, true); // true para loop (círculo)
        attackArcLineRenderer = FindOrCreateLineRenderer(AttackArcGOName, true); // true para loop (semicírculo fechado)
                                                                                 // ou false se for desenhar as linhas radiais manualmente sem loop.
                                                                                 // Com loop=true, o primeiro e último ponto (o centro) se conectam às pontas do arco.
    }

    LineRenderer FindOrCreateLineRenderer(string childName, bool loop)
    {
        Transform childTransform = transform.Find(childName);
        LineRenderer lr;

        if (childTransform == null)
        {
            GameObject childGO = new GameObject(childName);
            childGO.transform.SetParent(this.transform);
            childGO.transform.localPosition = Vector3.zero;
            childGO.transform.localRotation = Quaternion.identity; // Orientação local

            lr = childGO.AddComponent<LineRenderer>();

            if (lineMaterial != null) lr.material = lineMaterial;
            else lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            
            lr.startWidth = circleLineWidth;
            lr.endWidth = circleLineWidth;
            // lr.positionCount será definido ao atualizar
            lr.useWorldSpace = false; // Pontos são locais ao transform do LineRenderer
            lr.loop = loop; // Importante para fechar círculos ou arcos
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
        }
        else
        {
            lr = childTransform.GetComponent<LineRenderer>();
            if (lr == null)
            {
                DestroyImmediate(childTransform.gameObject);
                return FindOrCreateLineRenderer(childName, loop); // Tenta recriar
            }
            // Garante que a configuração de loop seja aplicada se o objeto já existir
            lr.loop = loop;
            lr.startWidth = circleLineWidth; // Garante que a largura seja atualizada se alterada no inspector
            lr.endWidth = circleLineWidth;
        }
        return lr;
    }

    void CleanupVisualizer(ref LineRenderer rendererInstance, string childName)
    {
        // Lógica de limpeza permanece a mesma
        if (rendererInstance != null && rendererInstance.gameObject.name == childName)
        {
            if (Application.isEditor && !Application.isPlaying) DestroyImmediate(rendererInstance.gameObject);
            else Destroy(rendererInstance.gameObject);
            rendererInstance = null;
        }
        else
        {
            Transform childTransform = transform.Find(childName);
            if (childTransform != null)
            {
                if (Application.isEditor && !Application.isPlaying) DestroyImmediate(childTransform.gameObject);
                else Destroy(childTransform.gameObject);
            }
        }
    }

    void UpdateLineRendererCircle(LineRenderer lr, float radius, Color color)
    {
        if (lr == null || !lr.gameObject.activeSelf) return; // Verifica se o GO está ativo

        lr.startColor = color;
        lr.endColor = color;
        // A largura já é definida em FindOrCreateLineRenderer e pode ser atualizada lá se necessário
        
        if (lr.positionCount != segmentsForFullCircle) lr.positionCount = segmentsForFullCircle;

        for (int i = 0; i < segmentsForFullCircle; i++)
        {
            float angle = i * (360f / segmentsForFullCircle) * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            lr.SetPosition(i, new Vector3(x, 0.01f, z)); // Pequeno offset Y
        }
    }

    void UpdateLineRendererAttackArc(LineRenderer lr, float radius, float attackAngleDegrees, Color color)
    {
        if (lr == null || !lr.gameObject.activeSelf || attackAngleDegrees <= 0) return;

        lr.startColor = color;
        lr.endColor = color;

        // Calcula quantos segmentos são necessários para este arco, proporcionalmente
        // Garante pelo menos 2 segmentos para formar um arco visível.
        int arcSegments = Mathf.Max(2, Mathf.CeilToInt(segmentsForFullCircle * (attackAngleDegrees / 360f)));
        
        // Pontos: 1 para o centro + (arcSegments + 1) para os pontos do arco.
        // O LineRenderer com loop=true conectará o último ponto do arco ao centro (ponto 0).
        lr.positionCount = arcSegments + 2; // Ponto central (0) + pontos do arco (1 a arcSegments+1)

        // Posição 0 é o centro do arco (origem da IA)
        lr.SetPosition(0, Vector3.zero + Vector3.up * 0.01f); // Ponto central, com pequeno offset Y

        float halfAngleRad = (attackAngleDegrees / 2f) * Mathf.Deg2Rad;
        float angleIncrementRad = (attackAngleDegrees * Mathf.Deg2Rad) / arcSegments;

        for (int i = 0; i <= arcSegments; i++)
        {
            float currentAngleRad = -halfAngleRad + (i * angleIncrementRad);
            
            // Os pontos do arco são calculados em relação ao forward do LineRenderer (que é o forward da IA)
            float x = Mathf.Sin(currentAngleRad) * radius; // Sin para x por causa da rotação
            float z = Mathf.Cos(currentAngleRad) * radius; // Cos para z (profundidade/forward)
            
            lr.SetPosition(i + 1, new Vector3(x, 0.01f, z));
        }
        // Com lr.loop = true, o LineRenderer conectará o último ponto (lr.GetPosition(arcSegments+1))
        // de volta ao primeiro ponto (lr.GetPosition(0)), formando a "fatia de pizza".
    }


    Color GetCurrentDetectionColor()
    {
        // Lógica permanece a mesma
        if (enemyNavigation == null) return Color.magenta;
        if (!Application.isPlaying && enemyNavigation.CurrentState == EnemyNavigation.DetectionState.Idle) return Color.gray;
        switch (enemyNavigation.CurrentState)
        {
            case EnemyNavigation.DetectionState.Detected: return Color.green;
            case EnemyNavigation.DetectionState.Idle: default: return Color.blue;
        }
    }

    void UpdateVisuals()
    {
        if (enemyNavigation == null)
        {
            enemyNavigation = GetComponent<EnemyNavigation>();
            if (enemyNavigation == null) {
                // Desabilita todos os renderers se não houver referência de navegação
                if(detectionRangeLineRenderer != null) detectionRangeLineRenderer.enabled = false;
                if(loseChaseRangeLineRenderer != null) loseChaseRangeLineRenderer.enabled = false;
                if(attackArcLineRenderer != null) attackArcLineRenderer.enabled = false;
                return;
            }
        }

        // Controla a visibilidade geral baseado em displayVisualizers e se a IA está morta
        bool shouldBeActive = displayVisualizers && !enemyNavigation.IsDead;

        // Atualiza visualizador de alcance de detecção
        if (detectionRangeLineRenderer != null)
        {
            detectionRangeLineRenderer.gameObject.SetActive(shouldBeActive); // Ativa/desativa o GameObject filho
            if (shouldBeActive)
            {
                UpdateLineRendererCircle(detectionRangeLineRenderer, enemyNavigation.DetectionRadius, Color.blue);
            }
        }

        // Atualiza visualizador de alcance de perda de perseguição
        if (loseChaseRangeLineRenderer != null)
        {
            loseChaseRangeLineRenderer.gameObject.SetActive(shouldBeActive);
            if (shouldBeActive)
            {
                UpdateLineRendererCircle(loseChaseRangeLineRenderer, enemyNavigation.LoseChaseRadius, Color.red);
            }
        }

        // Atualiza visualizador do arco de ataque
        if (attackArcLineRenderer != null)
        {
            attackArcLineRenderer.gameObject.SetActive(shouldBeActive && enemyNavigation.AttackAngle > 0 && enemyNavigation.AttackRadius > 0); // Só ativa se tiver ângulo e raio
            if (attackArcLineRenderer.gameObject.activeSelf) // Verifica se está realmente ativo para desenhar
            {
                UpdateLineRendererAttackArc(attackArcLineRenderer, enemyNavigation.AttackRadius, enemyNavigation.AttackAngle, attackRangeColor);
            }
        }
    }

    void Update()
    {
        UpdateVisuals();
    }
}