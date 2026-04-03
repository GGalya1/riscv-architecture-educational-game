using UnityEngine;
using System.Collections;

public class Blinker : MonoBehaviour
{
    [SerializeField] private Renderer _targetRenderer;
    [SerializeField] private Color _blinkColor = Color.yellow;
    [SerializeField] private float _duration = 1.0f;

    private Color _originalColor;
    private Coroutine _coroutine;
    private MaterialPropertyBlock _propBlock;
    private static readonly int ColorPropertyID = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        if (_targetRenderer == null) _targetRenderer = GetComponent<Renderer>();

        _propBlock = new MaterialPropertyBlock();
        
        if (_targetRenderer != null)
            _originalColor = _targetRenderer.sharedMaterial.color;
    }

    public void Trigger()
    {
        if (_targetRenderer == null) return;
        if (_coroutine != null) StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(BlinkSequence());
    }

    private IEnumerator BlinkSequence()
    {
        SetRendererColor(_blinkColor);

        yield return new WaitForSeconds(_duration);

        SetRendererColor(_originalColor);
        _coroutine = null;
    }

    private void SetRendererColor(Color color)
    {
        _targetRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(ColorPropertyID, color);
        _targetRenderer.SetPropertyBlock(_propBlock);
    }
}