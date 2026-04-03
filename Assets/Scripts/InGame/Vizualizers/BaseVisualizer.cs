using UnityEngine;
using DG.Tweening;

// Абстрактный базовый класс для всей визуализации компонентов
public abstract class BaseVizualizer : MonoBehaviour, IVizualizer
{
    // --- FIELDS FOR CONFIGURATION (Visible in Inspector) ---

    [SerializeField] protected GameObject _panelInstance;
    [SerializeField] private CanvasGroup _panelCanvasGroup;

    [Header("Visual Settings")]
    [SerializeField] protected Color _selectionColor = Color.blue;

    [Header("Animation Settings")]
    [SerializeField] private float _animDuration = 0.25f;
    [SerializeField] private float _moveDistance = 0.2f;

    // --- FIELDS FOR CACHED REFERENCES ---

    // Кэшированная основная камера, для позиционирования UI
    protected Camera _staticCamera;

    // Кэшированный рендерер, если нужно менять цвет модели (для простого изменения цвета)
    [SerializeField] protected Renderer _bigModelRenderer;

    // Флаг, показывающий, активна ли визуализация в данный момент
    protected bool isVisualizationActive = false;
    private Vector3 _targetLocalPos;

    // --- PUBLIC PROPERTIES (Read-Only access to configuration) ---

    public GameObject PanelPrefab => _panelInstance;

    // --- OPTIMIZATION FIELDS ---
    private Color _originalColor;
    private MaterialPropertyBlock _propBlock;
    private static readonly int ColorPropertyID = Shader.PropertyToID("_BaseColor");

    // --- UNITY LIFECYCLE ---

    protected virtual void Awake()
    {
        _propBlock = new MaterialPropertyBlock();

        if (_bigModelRenderer != null)
        {
            _originalColor = _bigModelRenderer.sharedMaterial.color;
        }

        // 2. Кэширование камерыa
        _staticCamera = Camera.main;
        if (_staticCamera == null)
        {
            Debug.LogError($"Main camera not found by {gameObject.name}!");
        }

        // 3. Создание и инициализация панели UI
        _targetLocalPos = _panelInstance.transform.localPosition;
        PrepareHiddenState();

        // Абстрактный метод, который должен быть реализован наследником
        InitializePanelController();
    }

    // --- PUBLIC INTERFACE (IVizualizer) ---

    // ShowData и HideData имеют общую логику, но ShowData имеет специфику
    public virtual void ShowData()
    {
        if (isVisualizationActive)
        {
            HideData();
            return;
        }

        // Общая логика
        isVisualizationActive = true;
        SetModelColor(_selectionColor);

        _panelInstance.transform.DOKill();
        _panelCanvasGroup.DOKill();

        _panelInstance.transform.DOLocalMove(_targetLocalPos, _animDuration).SetEase(Ease.OutCubic);
        _panelCanvasGroup.DOFade(1f, _animDuration).SetEase(Ease.OutCubic).OnComplete(() => {
            _panelCanvasGroup.interactable = true;
            _panelCanvasGroup.blocksRaycasts = true;
        });
    }

    public virtual void HideData()
    {
        isVisualizationActive = false;
        SetModelColor(_originalColor);

        _panelCanvasGroup.interactable = false;
        _panelCanvasGroup.blocksRaycasts = false;

        _panelInstance.transform.DOKill();
        _panelCanvasGroup.DOKill();

        _panelCanvasGroup.DOFade(0f, _animDuration).SetEase(Ease.InCubic);
        _panelInstance.transform.DOLocalMove(_targetLocalPos - Vector3.up * _moveDistance, _animDuration)
                .SetEase(Ease.InCubic);  
    }
    private void SetModelColor(Color color)
    {
        if (_bigModelRenderer == null) return;

        _bigModelRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(ColorPropertyID, color);
        _bigModelRenderer.SetPropertyBlock(_propBlock);
    }

    // Абстрактные методы, которые должны быть реализованы в дочерних классах
    public abstract void ResetVizualization();
    protected abstract void InitializePanelController();

    // --- PRIVATE/PROTECTED METHODS ---

    private void PrepareHiddenState()
    {
        _panelCanvasGroup.alpha = 0;
        _panelCanvasGroup.interactable = false;
        _panelCanvasGroup.blocksRaycasts = false;

        _panelInstance.transform.localPosition = _targetLocalPos - Vector3.up * _moveDistance;
    }
}
