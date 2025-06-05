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
    public Material lineMaterial;

    [Header("Visualização dos Ranges de Detecção")]
    [Tooltip("Marque para exibir os círculos de visualização no editor e no jogo.")]
    public bool displayVisualizers = true;

    [Header("Visualização do Range de Ataque")]
    private Color attackRangeColor = Color.yellow;

    private LineRenderer detectionRangeLineRenderer;
    private LineRenderer loseChaseRangeLineRenderer;
    private LineRenderer attackArcLineRenderer; // Novo LineRenderer para o ataque

    // Nomes dos GameObjects filhos para os visualizadores
    private const string DetectionCircleGOName = "DetectionRange_Visualizer_Child";
    private const string LoseChaseCircleGOName = "LoseChaseRange_Visualizer_Child";
    private const string AttackArcGOName = "AttackArc_Visualizer_Child";

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
        detectionRangeLineRenderer = FindOrCreateLineRenderer(DetectionCircleGOName, true); 
        loseChaseRangeLineRenderer = FindOrCreateLineRenderer(LoseChaseCircleGOName, true); 
        attackArcLineRenderer = FindOrCreateLineRenderer(AttackArcGOName, true); 
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
                return FindOrCreateLineRenderer(childName, loop);
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
        if (lr == null || !lr.gameObject.activeSelf) return;

        lr.startColor = color;
        lr.endColor = color;
        
        if (lr.positionCount != segmentsForFullCircle) lr.positionCount = segmentsForFullCircle;

        for (int i = 0; i < segmentsForFullCircle; i++)
        {
            float angle = i * (360f / segmentsForFullCircle) * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            lr.SetPosition(i, new Vector3(x, 0.01f, z));
        }
    }

    void UpdateLineRendererAttackArc(LineRenderer lr, float radius, float attackAngleDegrees, Color color)
    {
        if (lr == null || !lr.gameObject.activeSelf || attackAngleDegrees <= 0) return;

        lr.startColor = color;
        lr.endColor = color;

        // Calcula quantos segmentos são necessários para este arco, proporcionalmente
        int arcSegments = Mathf.Max(2, Mathf.CeilToInt(segmentsForFullCircle * (attackAngleDegrees / 360f)));
        
        // Pontos: 1 para o centro + (arcSegments + 1) para os pontos do arco.
        lr.positionCount = arcSegments + 2;

        lr.SetPosition(0, Vector3.zero + Vector3.up * 0.01f);

        float halfAngleRad = (attackAngleDegrees / 2f) * Mathf.Deg2Rad;
        float angleIncrementRad = (attackAngleDegrees * Mathf.Deg2Rad) / arcSegments;

        for (int i = 0; i <= arcSegments; i++)
        {
            float currentAngleRad = -halfAngleRad + (i * angleIncrementRad);
            
            float x = Mathf.Sin(currentAngleRad) * radius; // Sin para x por causa da rotação
            float z = Mathf.Cos(currentAngleRad) * radius; // Cos para z (profundidade/forward)
            
            lr.SetPosition(i + 1, new Vector3(x, 0.01f, z));
        }
    }


    Color GetCurrentDetectionColor()
    {
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

        if (detectionRangeLineRenderer != null)
        {
            detectionRangeLineRenderer.gameObject.SetActive(shouldBeActive); // Ativa/desativa o GameObject filho
            if (shouldBeActive)
            {
                UpdateLineRendererCircle(detectionRangeLineRenderer, enemyNavigation.DetectionRadius, Color.blue);
            }
        }

        if (loseChaseRangeLineRenderer != null)
        {
            loseChaseRangeLineRenderer.gameObject.SetActive(shouldBeActive);
            if (shouldBeActive)
            {
                UpdateLineRendererCircle(loseChaseRangeLineRenderer, enemyNavigation.LoseChaseRadius, Color.red);
            }
        }

        if (attackArcLineRenderer != null)
        {
            attackArcLineRenderer.gameObject.SetActive(shouldBeActive && enemyNavigation.AttackAngle > 0 && enemyNavigation.AttackRadius > 0); // Só ativa se tiver ângulo e raio
            if (attackArcLineRenderer.gameObject.activeSelf)
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