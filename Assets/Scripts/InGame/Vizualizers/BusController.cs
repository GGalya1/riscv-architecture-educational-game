using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BusController : MonoBehaviour
{
    [Header("UI Элементы")]
    public Slider speedSlider;

    [Header("Настройки движения")]
    public GameObject spherePrefab;      // Префаб шара (сигнала)
    public float movementSpeed = 7f;     // Скорость движения шара

    [Header("Все сегменты шин")]
    // Здесь мы храним ссылки на все LineRenderer в сцене.
    // Назначаются через инспектор.
    public LineRenderer[] busSegments;

    // Словарь для хранения пути каждой шины, чтобы не вычислять его каждый раз
    private Dictionary<LineRenderer, Vector3[]> busPaths = new Dictionary<LineRenderer, Vector3[]>();
    private Dictionary<LineRenderer, Vector3[]> reverseBusPaths = new Dictionary<LineRenderer, Vector3[]>();

    private List<BusSignal> activeSignals = new List<BusSignal>();
    public bool NoActiveSignals => activeSignals.Count == 0;

    void Start()
    {
        InitializePaths();

        if (spherePrefab != null) spherePrefab.SetActive(false);

        if (speedSlider != null)
        {
            speedSlider.onValueChanged.AddListener(OnSliderValueChanged);
            movementSpeed = speedSlider.value;
        }
        else {
            Debug.LogError("Slider is null !");
        }

        Debug.Log($"Контроллер загрузил {busPaths.Count} сегментов шин.");
    }

    /// <summary>
    /// Публичный метод для запуска сигнала по конкретной шине.
    /// Этот метод ты будешь вызывать из других скриптов (например, при клике на блок CPU).
    /// </summary>
    /// <param name="targetBus">LineRenderer, по которому должен пойти сигнал.</param>
    public void StartBusSignal(LineRenderer targetBus, bool reversedPath = false)
    {
        Vector3[] pathPoints = GetPathPoints(targetBus, reversedPath);
        if (pathPoints == null || pathPoints.Length < 2) return;

        // Создаем шар
        GameObject currentSphere = Instantiate(spherePrefab);
        currentSphere.SetActive(true);
        
        currentSphere.transform.position = pathPoints[0];

        // Получаем компонент BusSignal
        BusSignal signal = currentSphere.GetComponent<BusSignal>();
        
        // 2. Важно: Настраиваем скорость и добавляем в список мониторинга
        if (signal != null)
        {
            activeSignals.Add(signal);
        }

        StartCoroutine(MoveSphereAlongPath(signal, pathPoints));
    }

    public void StartBusSignal(LineRenderer targetBus, int value, bool reversedPath = false)
    {
        Vector3[] pathPoints = GetPathPoints(targetBus, reversedPath);
        if (pathPoints == null || pathPoints.Length < 2) return;

        // Создаем шар
        GameObject currentSphere = Instantiate(spherePrefab);
        currentSphere.SetActive(true);

        currentSphere.transform.position = pathPoints[0];

        // Получаем компонент BusSignal
        BusSignal signal = currentSphere.GetComponent<BusSignal>();
        signal.UIRegisterPanel.Display("Signal", $"{value}");

        // 2. Важно: Настраиваем скорость и добавляем в список мониторинга
        if (signal != null)
        {
            activeSignals.Add(signal);
        }

        StartCoroutine(MoveSphereAlongPath(signal, pathPoints));
    }

    // Coroutine для движения шара по заданному массиву точек
    IEnumerator MoveSphereAlongPath(BusSignal signal, Vector3[] pathPoints)
    {
        if (signal == null) yield break;

        for (int i = 1; i < pathPoints.Length; i++)
        {
            Vector3 startPoint = pathPoints[i - 1];
            Vector3 targetPoint = pathPoints[i];

            while (signal.transform.position != targetPoint)
            {
                // Берем актуальную скорость (обычную или ускоренную)
                float currentSpeed = movementSpeed;

                signal.transform.position = Vector3.MoveTowards(
                    signal.transform.position,
                    targetPoint,
                    currentSpeed * Time.deltaTime
                );
                yield return null;
            }
        }

        // 4. Удаляем из списка перед уничтожением
        activeSignals.Remove(signal);
        Destroy(signal.UIRegisterPanel.gameObject);
        Destroy(signal.gameObject);
    }

    private void OnSliderValueChanged(float value)
    {
        movementSpeed = value;
        
    }

    private void InitializePaths() {
        foreach (LineRenderer lr in busSegments)
        {
            // Проверка на null (если в инспекторе есть пустые слоты)
            if (lr == null)
            {
                Debug.LogWarning("Обнаружен пустой слот LineRenderer в массиве busSegments. Пропускаю.");
                continue;
            }

            // Трансформ, относительно которого LineRenderer хранит свои точки
            Transform busTransform = lr.transform;

            int pointCount = lr.positionCount;
            if (pointCount < 2)
            {
                Debug.LogWarning($"Шина '{lr.gameObject.name}' имеет менее 2 точек и не будет загружена.");
                continue;
            }

            // Получаем локальные координаты из LineRenderer
            Vector3[] localPoints = new Vector3[pointCount];
            lr.GetPositions(localPoints);

            // Массивы для хранения путей в мировом пространстве
            Vector3[] worldPoints = new Vector3[pointCount];
            Vector3[] reversedWorldPoints = new Vector3[pointCount];

            // --- 2. ПРЕОБРАЗОВАНИЕ ЛОКАЛЬНЫХ ТОЧЕК В МИРОВЫЕ ---

            for (int i = 0; i < pointCount; i++)
            {
                // Используем TransformPoint, чтобы перевести локальную позицию (localPoints[i])
                // в глобальную (мировую) позицию, учитывая положение и вращение родителя шины (busTransform).
                worldPoints[i] = busTransform.TransformPoint(localPoints[i]);
            }

            // --- 3. СОХРАНЕНИЕ ПРЯМОГО ПУТИ ---

            // Сохраняем массив мировых координат для прямого движения
            busPaths.Add(lr, worldPoints);

            // --- 4. СОХРАНЕНИЕ ОБРАТНОГО ПУТИ (РЕВЕРС) ---

            // Копируем массив мировых координат
            System.Array.Copy(worldPoints, reversedWorldPoints, pointCount);

            // Реверсируем порядок элементов для обратного движения
            System.Array.Reverse(reversedWorldPoints);

            // Сохраняем массив реверсивных мировых координат
            reverseBusPaths.Add(lr, reversedWorldPoints);
        }
    }

    private Vector3[] GetPathPoints(LineRenderer targetBus, bool reversed)
    {
        if (targetBus == null) return null;
        var dict = reversed ? reverseBusPaths : busPaths;
        return dict.ContainsKey(targetBus) ? dict[targetBus] : null;
    }
}
