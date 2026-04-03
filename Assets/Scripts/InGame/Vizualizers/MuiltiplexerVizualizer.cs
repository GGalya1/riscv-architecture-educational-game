using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.XR;

public class MuiltiplexerVizualizer: BaseVizualizer
{
    [Header("Bus Line Renderers")]
    [SerializeField] private LineRenderer[] _inputBuses;

    [SerializeField] private LineRenderer _outputBus;
    [SerializeField] private LineRenderer _controlBus;

    [Header("Visual Bits Renderers")]
    [SerializeField] private Renderer[] _bitRenderers;

    [Header("Colors")]
    [SerializeField] private Color _disabledColor = Color.gray;
    [SerializeField] private Color _activeColor = Color.red;

    private MultiplexerControlPanel _uiController;
    public MultiplexerControlPanel UIController => _uiController;

    private int _currentChoosenPath = -1;
    public int CurrentChoosenMuxPath => _currentChoosenPath;

    private MaterialPropertyBlock _propBlock;
    private static readonly int ColorPropertyID = Shader.PropertyToID("_BaseColor");


    protected override void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        base.Awake();

 
        if (_uiController != null)
        {
            if (_bitRenderers.Length > 2) 
            {
                _uiController.Setup(true, true, true, "Multiplexer 3");
            }
            else
            {
                _uiController.Setup(true, true, false, "Multiplexer 2");
            }
        }

        _uiController.FirstWayButton.onClick.AddListener(() => SelectPath(0));
        _uiController.SecondWayButton.onClick.AddListener(() => SelectPath(1));
        _uiController.ThirdWayButton.onClick.AddListener(() => SelectPath(2));

        ResetVizualization();
    }
    protected override void InitializePanelController()
    {
        // Специфичная для этого класса инициализация контроллера
        _uiController = _panelInstance.GetComponent<MultiplexerControlPanel>();
        if (_uiController == null)
        {
            Debug.LogError($"MultiplexerControlPanel component not found on the prefab for {gameObject.name}!");
        }
    }

    public override void ResetVizualization() {
        _currentChoosenPath = -1;
        UpdateVisuals(-1);
    }
    public void SelectPath(int index)
    {
        if (index < 0 || index >= _inputBuses.Length) return;

        // Debug.Log($"Path {index + 1} chosen");
        _currentChoosenPath = index;
        UpdateVisuals(index);
        HideData();
    }

    #region helpers
    private void UpdateVisuals(int activeIndex)
    {
        for (int i = 0; i < _inputBuses.Length; i++)
        {
            bool isActive = (i == activeIndex);
            SetColor(_inputBuses[i], isActive ? _activeColor : _disabledColor);

            if (i < _bitRenderers.Length)
                SetColor(_bitRenderers[i], isActive ? _activeColor : _disabledColor);
        }

        SetColor(_outputBus, activeIndex != -1 ? _activeColor : _disabledColor);
    }

    private void SetColor(Renderer renderer, Color color)
    {
        if (renderer == null) return;

        renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(ColorPropertyID, color);
        renderer.SetPropertyBlock(_propBlock);
    }
    #endregion
}
