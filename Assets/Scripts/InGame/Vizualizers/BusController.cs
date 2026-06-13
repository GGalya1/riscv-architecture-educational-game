using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BusController : MonoBehaviour
{
    [Header("UI Elements")] public Slider speedSlider;

    [Header("Motion settings")] public GameObject spherePrefab; // Sphere prefab (aka signal)

    public float movementSpeed = 7f; // Velocity of sphere

    [Header("Pool settings")] [SerializeField]
    private int poolPreWarmCount = 5;

    //[Header("All bus segments")]
    // This is where we store references to all LineRenderers in the scene.
    // These are assigned via the Inspector.
    //public LineRenderer[] busSegments;

    private readonly List<BusSignal> _activeSignals = new();


    // A dictionary to store the path for each bus so it doesn't have to be calculated every time
    private readonly Dictionary<LineRenderer, Vector3[]> _busPaths = new();

    private readonly Stack<BusSignal> _pool = new();
    private readonly Dictionary<LineRenderer, Vector3[]> _reverseBusPaths = new();

    private BusSignal _spherePrefabSignal;
    public bool NoActiveSignals => _activeSignals.Count == 0;

    private void Awake()
    {
        _spherePrefabSignal = spherePrefab.GetComponent<BusSignal>();
        //InitializePaths();
    }

    private void Start()
    {
        PrewarmPool();
        spherePrefab.SetActive(false);

        if (speedSlider != null)
        {
            speedSlider.onValueChanged.AddListener(OnSliderValueChanged);
            movementSpeed = speedSlider.value;
        }
        else
        {
            Debug.LogError("Slider is null !");
        }

        Debug.Log($"The controller has loaded {_busPaths.Count} bus segments.");
    }

    private void OnDestroy()
    {
        // all objects in pool need to be removed manual
        foreach (var signal in _activeSignals)
            if (signal != null)
                Destroy(signal.gameObject);

        foreach (var signal in _pool)
            if (signal != null)
                Destroy(signal.gameObject);
    }


    public void StartBusSignal(LineRenderer targetBus, int value, bool reversedPath = false)
    {
        StartBusSignalInternal(targetBus, value, reversedPath);
    }

    public void RegisterSegment(LineRenderer lr)
    {
        if (lr == null)
        {
            Debug.LogWarning("null LineRenderer passed - skipping.");
            return;
        }

        // Idempotent - skip if already registered (e.g. present in busSegments[])
        if (_busPaths.ContainsKey(lr))
            return;

        var t = lr.transform;
        var count = lr.positionCount;

        if (count < 2)
        {
            Debug.LogWarning($" '{lr.gameObject.name}' has fewer than 2 vertices - skipping.");
            return;
        }

        // Retrieve local positions and convert to world space
        var localPoints = new Vector3[count];
        lr.GetPositions(localPoints);

        var worldPoints = new Vector3[count];
        var reversedPts = new Vector3[count];

        for (var i = 0; i < count; i++)
            worldPoints[i] = t.TransformPoint(localPoints[i]);

        Array.Copy(worldPoints, reversedPts, count);
        Array.Reverse(reversedPts);

        _busPaths[lr] = worldPoints;
        _reverseBusPaths[lr] = reversedPts;
    }

    #region Internal

    /// <summary>
    ///     A public method for triggering a signal on a specific bus.
    ///     You will call this method from other scripts (for example, when clicking on the CPU block).
    /// </summary>
    /// <param name="targetBus">LineRenderer, through which the signal should be sent.</param>
    /// <param name="reversedPath">bool, says if the path must be animated in reverse direction</param>
    /// <param name="value">int, which signal carry</param>
    private void StartBusSignalInternal(LineRenderer targetBus, int value, bool reversedPath = false)
    {
        var pathPoints = GetPathPoints(targetBus, reversedPath);
        if (pathPoints == null || pathPoints.Length < 2) return;

        // Create a sphere
        var signal = GetSignal(pathPoints[0]);

        signal.UIRegisterPanel.Display("Signal", $"{value}");

        // 2. Important: Adjust the speed and add it to the monitoring list
        _activeSignals.Add(signal);
        StartCoroutine(MoveSphereAlongPath(signal, pathPoints));
    }

    // A coroutine to move a ball along a given array of points
    private IEnumerator MoveSphereAlongPath(BusSignal signal, Vector3[] pathPoints)
    {
        // if (signal == null) yield break;

        for (var i = 1; i < pathPoints.Length; i++)
        {
            var targetPoint = pathPoints[i];

            while (signal.transform.position != targetPoint)
            {
                // Take the current speed (normal or fast-forward)
                var currentSpeed = movementSpeed;

                signal.transform.position = Vector3.MoveTowards(
                    signal.transform.position,
                    targetPoint,
                    currentSpeed * Time.deltaTime
                );
                yield return null;
            }
        }

        // 4. Remove from the list before destruction
        ReturnSignal(signal);
    }

    private void OnSliderValueChanged(float value)
    {
        movementSpeed = value;
    }

    /*private void InitializePaths()
    {
        foreach (var lr in busSegments)
        {
            // Check for null (if there are empty slots in the inspector)
            if (lr == null)
            {
                Debug.LogWarning("An empty LineRenderer slot was found in the busSegments array. Skipping it.");
                continue;
            }

            // The transform relative to which the LineRenderer stores its points
            var busTransform = lr.transform;

            var pointCount = lr.positionCount;
            if (pointCount < 2)
            {
                Debug.LogWarning($"The '{lr.gameObject.name}' bus has fewer than 2 vertices and will not be loaded.");
                continue;
            }

            // Retrieve local coordinates from the LineRenderer
            var localPoints = new Vector3[pointCount];
            lr.GetPositions(localPoints);

            // Arrays for storing paths in world space
            var worldPoints = new Vector3[pointCount];
            var reversedWorldPoints = new Vector3[pointCount];

            // --- 2. CONVERSION OF LOCAL POINTS TO WORLD COORDINATES ---

            for (var i = 0; i < pointCount; i++)
                // We use TransformPoint to convert the local position (localPoints[i])
                // to a global (world) position, taking into account the position and rotation of the bus's parent (busTransform).
                worldPoints[i] = busTransform.TransformPoint(localPoints[i]);

            // --- 3. SAVING THE STRAIGHT PATH ---

            // We store an array of world coordinates for forward motion
            _busPaths.Add(lr, worldPoints);

            // --- 4. SAVING THE RETURN PATH (REVERSE) ---

            // Copy the array of world coordinates
            Array.Copy(worldPoints, reversedWorldPoints, pointCount);

            // Reverse the order of the elements for backward movement
            Array.Reverse(reversedWorldPoints);

            // We store an array of inverse global coordinates
            _reverseBusPaths.Add(lr, reversedWorldPoints);
        }
    }*/

    private Vector3[] GetPathPoints(LineRenderer targetBus, bool reversed)
    {
        // if (targetBus == null) return null;
        return (reversed ? _reverseBusPaths : _busPaths).GetValueOrDefault(targetBus);
    }

    #endregion

    #region Pool

    private void PrewarmPool()
    {
        for (var i = 0; i < poolPreWarmCount; i++)
        {
            var signal = Instantiate(_spherePrefabSignal);
            signal.gameObject.SetActive(false);
            _pool.Push(signal);
        }
    }

    private BusSignal GetSignal(Vector3 startPosition)
    {
        var signal = _pool.Count > 0
            ? _pool.Pop()
            : Instantiate(_spherePrefabSignal);

        signal.transform.position = startPosition;
        signal.gameObject.SetActive(true);
        return signal;
    }

    private void ReturnSignal(BusSignal signal)
    {
        _activeSignals.Remove(signal);
        signal.ResetVisualisation(); // kill DOTWeen + ui panel
        signal.gameObject.SetActive(false);
        _pool.Push(signal);
    }

    #endregion
}