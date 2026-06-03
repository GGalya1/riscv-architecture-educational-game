using UnityEngine;
using DG.Tweening;

// An abstract base class for all component rendering
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

    // Cached main camera, for UI positioning
    protected Camera _staticCamera;

    // Cached renderer, if you need to change the model's color (for a simple color change)
    [SerializeField] protected Renderer _bigModelRenderer;

    // A flag indicating whether the visualization is currently active
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

        // 2. Camera caching
        _staticCamera = Camera.main;
        if (_staticCamera == null)
        {
            Debug.LogError($"Main camera not found by {gameObject.name}!");
        }

        // 3. Creating and initializing the UI panel
        _targetLocalPos = _panelInstance.transform.localPosition;
        PrepareHiddenState();

        // An abstract method that must be implemented by a subclass
        InitializePanelController();
    }

    // --- PUBLIC INTERFACE (IVizualizer) ---

    // ShowData and HideData share the same basic logic, but ShowData has some specific features
    public virtual void ShowData()
    {
        if (isVisualizationActive)
        {
            HideData();
            return;
        }

        // General logic
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

    // Abstract methods that must be implemented in subclasses
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
