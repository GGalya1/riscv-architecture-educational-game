using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class Blinker : MonoBehaviour
{
    [FormerlySerializedAs("_targetRenderer")] [SerializeField] private Renderer targetRenderer;
    [FormerlySerializedAs("_blinkColor")] [SerializeField] private Color blinkColor = Color.yellow;
    [FormerlySerializedAs("_duration")] [SerializeField] private float duration = 1.0f;

    private Color _originalColor;
    private Coroutine _coroutine;
    private MaterialPropertyBlock _propBlock;
    private static readonly int ColorPropertyID = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();

        _propBlock = new MaterialPropertyBlock();
        
        if (targetRenderer != null)
            _originalColor = targetRenderer.sharedMaterial.color;
    }

    public void Trigger()
    {
        if (targetRenderer == null) return;
        if (_coroutine != null) StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(BlinkSequence());
    }

    private IEnumerator BlinkSequence()
    {
        SetRendererColor(blinkColor);

        yield return new WaitForSeconds(duration);

        SetRendererColor(_originalColor);
        _coroutine = null;
    }

    private void SetRendererColor(Color color)
    {
        targetRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(ColorPropertyID, color);
        targetRenderer.SetPropertyBlock(_propBlock);
    }
}