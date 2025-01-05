using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using static UnityEngine.GraphicsBuffer;
using ZXing.PDF417.Internal;

public class SetNavigationTarget3 : MonoBehaviour
{
    public static SetNavigationTarget3 Instance; // Instancia �nica de SetNavigationTarget

    [SerializeField]
    public List<Route> routes; // Lista de rutas
    [SerializeField]
    private Camera arCamera; // Referencia a la c�mara AR
    [SerializeField]
    private ARSession session; // Referencia a la sesi�n AR
    [SerializeField]
    private ArrowImageIndicator arrowIndicator; // Referencia al indicador de flecha

    // Variables UI
    [SerializeField]
    private GameObject qrScanPanel; // Panel para escanear el c�digo QR
    [SerializeField]
    private Button nextTargetButton; // Bot�n para el siguiente objetivo
    [SerializeField]
    private Text distanceText; // Texto para mostrar la distancia al objetivo
    [SerializeField]
    private Text logErrorText; // Texto para mostrar mensajes de error
    [SerializeField]
    private Text logSuccessText; // Texto para mostrar mensajes de �xito
    [SerializeField]
    private Text targetCounterText; // Texto para mostrar el contador de objetivos
    [SerializeField]
    private Text currentTargetText; // Texto para mostrar el objetivo actual
    [SerializeField]
    private Text levelCounterText; // Texto para mostrar el contador de niveles
    [SerializeField]
    private Text routeCounterText; // Texto para mostrar el contador de rutas
    [SerializeField]
    private Text completionText; // Texto para mostrar el porcentaje completado

    // Variables de navegaci�n
    private NavMeshPath path; // Camino de navegaci�n
    private LineRenderer line; // LineRenderer para dibujar el camino

    // Contadores
    private int currentRouteIndex = 0; // Indice de la ruta actual
    private int currentLevelIndex = 0; // Indice para rastrear el nivel actual
    private int currentTargetIndex = 0; // Indice para rastrear el objetivo actual
    private int completedTargetsCount = 0; // Trackear la cantidad de objetivos completados

    // Flags
    public bool lineToggle = true; // Make lineToggle public
    public bool completedLevel = false; // Make completedLevel public
    public bool completedTour = false; // Make completedTour public
    public bool isFirstTarget = true; // Make isFirstTarget public
    private bool isTourActive = false; // Flag to control when the tour starts

    // Mensajes
    private const string TourCompletedMessage = "Felicidades, Tour Completado!";
    private const string StartTourMessage = "Tour iniciado en el nivel {0}. Para comenzar escanee el Codigo QR!";
    private const string LevelCompleteMessage = "Nivel {0} Completado! Vaya al nivel {1}. Luego escanee el Codigo QR para continuar.";
    private const string NoPathFoundMessage = "No se encontr� un camino hacia el objetivo.";
    private const string PathCalculationErrorMessage = "Error al calcular el camino.";

    // Awake se llama cuando el script es inicializado
    private void Awake()
    {
        // Asegurarse de que solo haya una instancia de SetNavigationTarget
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Destruir el objeto si ya existe una instancia
        }
    }

    // Funcion para inicializar el tour
    public void InitializeTour(int routeIndex)
    {
        if (routes == null || routes.Count == 0)
        {
            Debug.LogError("Rutas no est�n seteadas.");
            return;
        }

        // Activar la c�mara cuando se inicia el tour
        if (arCamera != null)
        {
            arCamera.enabled = true;
        }

        currentRouteIndex = routeIndex; // Setear el indice de la ruta actual
        currentLevelIndex = 0; // Reiniciar el indice del nivel
        currentTargetIndex = 0; // Reiniciar el indice del objetivo
        completedTargetsCount = 0; // Reiniciar el contador de objetivos completados

        Debug.Log($"Ruta seleccionada: {routes[currentRouteIndex].RouteName}");

        // Inicializar el camino y el LineRenderer
        path = new NavMeshPath();
        line = GetComponent<LineRenderer>();

        if (line == null)
        {
            Debug.LogError("LineRenderer no encontrado.");
            return;
        }

        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.material = new Material(Shader.Find("Sprites/Default")) { color = Color.green * 0.8f };

        // Mostrar el bot�n de siguiente objetivo
        nextTargetButton.gameObject.SetActive(false);
        nextTargetButton.onClick.AddListener(OnNextTargetButtonClicked);

        // Limpiar los textos
        ClearLogText();
        ClearAllTexts();

        // Desplegar el mensaje de inicio del tour
        UpdateRouteCounterText();

        // Mostar el panel de escaneo de QR
        qrScanPanel.SetActive(true);

        // Desplegar el mensaje para que el usuario escanee el c�digo QR
        Debug.Log(string.Format(StartTourMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex));
        UpdateSuccessLogText(string.Format(StartTourMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex));

        UpdateCompletionPercentage(); // Reiniciar el porcentaje completado
        Debug.Log("Se inicia el tour");

    }

    // Funci�n para iniciar el camino
    private void StartPathFinding()
    {
        Debug.Log("Iniciando el camino.");

        // Setear el flag para indicar que el tour est� activo
        isTourActive = true;
        arrowIndicator.SetArrowEnabled(true); // Activar la flecha indicadora

        UpdateTargetCounterText();
        UpdateLevelCounterText();

        // Iniciar el pathfinding
        CalculateAndDrawPath();
    }

    // Start se llama antes de la primera actualizaci�n del frame
    void Start()
    {
        qrScanPanel.SetActive(false); //esconder el panel de qr scan
        arrowIndicator.SetArrowEnabled(false); // Desactivar la flecha inicialmente

        // Desactivar la c�mara inicialmente
        if (arCamera != null)
        {
            arCamera.enabled = false;
        }
    }

    // Update se llama una vez por frame
    void Update()
    {
        // Solo continuar si el tour est� activo
        if (!isTourActive)
        {
            return;
        }

        line.enabled = false; // comentar esta linea para que se muestre la linea

        // Revisar si el usuario ha alcanzado el objetivo
        if (currentTargetIndex >= routes[currentRouteIndex].Levels[currentLevelIndex].Targets.Count)
        {
            // Revisar si el nivel actual es el �ltimo nivel
            if (currentLevelIndex >= routes[currentRouteIndex].Levels.Count - 1)
            {
                if (!completedTour)
                {
                    completedTargetsCount++;
                    UpdateCompletionPercentage();
                }

                line.enabled = false;
                arrowIndicator.SetArrowEnabled(false);
                completedTour = true;
                /*Debug.Log(TourCompletedMessage);*/
                UpdateSuccessLogText(TourCompletedMessage);
                return;
            }

            // Si no es el �ltimo nivel, pasar al siguiente nivel
            qrScanPanel.SetActive(true); // mostar el panel de qr scan
            /*Debug.Log(string.Format(LevelCompleteMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex, routes[currentRouteIndex].Levels[currentLevelIndex + 1].LevelIndex));*/
            UpdateSuccessLogText(string.Format(LevelCompleteMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex, routes[currentRouteIndex].Levels[currentLevelIndex + 1].LevelIndex));
            line.enabled = false;
            arrowIndicator.SetArrowEnabled(false);
            nextTargetButton.gameObject.SetActive(false); // Esconder el bot�n de siguiente objetivo
            return;
        }

        // Calcular la distancia al objetivo actual
        GameObject currentTarget = routes[currentRouteIndex].Levels[currentLevelIndex].Targets[currentTargetIndex].PositionObject;
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
        /*float distanceToTarget = CalculatePathLength(path.corners, currentTarget);*/
        /*Debug.Log($"Distancia a objetivo: {distanceToTarget:F2} m");*/
        distanceText.text = $"Distancia a objetivo: {distanceToTarget:F2} m";
        currentTargetText.text = $"Objetivo actual: {routes[currentRouteIndex].Levels[currentLevelIndex].Targets[currentTargetIndex].Name}";

        // Revisar si el usuario ha alcanzado el objetivo
        if (distanceToTarget <= 1.5f)
        {
            if (currentTargetIndex < routes[currentRouteIndex].Levels[currentLevelIndex].Targets.Count - 1)
            {
                nextTargetButton.gameObject.SetActive(true); // mostrar el bot�n de siguiente objetivo
                Debug.Log("Objetivo alcanzado. Presione el bot�n 'Siguiente objetivo'.");
                arrowIndicator.SetArrowEnabled(false);
            }
            else
            {
                nextTargetButton.gameObject.SetActive(false); // Esconder el bot�n de siguiente objetivo
                // Asegurarse de no activar la flecha si ya se alcanz� el �ltimo objetivo
                if (arrowIndicator != null && currentTargetIndex < routes[currentRouteIndex].Levels[currentLevelIndex].Targets.Count - 1)
                {
                    arrowIndicator.SetArrowEnabled(true);
                }
                currentTargetIndex++; // Mover al siguiente objetivo
            }
        }
        else
        {
            nextTargetButton.gameObject.SetActive(false);
            arrowIndicator.SetArrowEnabled(true);
            ClearLogText(); // Limpiar los mensajes de log
        }

        if (lineToggle)
        {
            CalculateAndDrawPath();
        }
    }

    // Funci�n para calcular la longitud del camino
    private float CalculatePathLength(Vector3[] corners, GameObject currentTarget)
    {
        float totalDistance = Vector3.Distance(transform.position, currentTarget.transform.position);

        for (int i = 0; i < corners.Length - 1; i++)
        {
            totalDistance += Vector3.Distance(corners[i], corners[i + 1]);
        }

        return totalDistance;
    }

    // Funcio para calcular y dibujar el camino
    void CalculateAndDrawPath()
    {
        if (currentTargetIndex >= routes[currentRouteIndex].Levels[currentLevelIndex].Targets.Count)
        {
            Debug.Log(string.Format(LevelCompleteMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex, routes[currentRouteIndex].Levels[currentLevelIndex + 1].LevelIndex));
            UpdateSuccessLogText(string.Format(LevelCompleteMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex, routes[currentRouteIndex].Levels[currentLevelIndex + 1].LevelIndex));
            line.enabled = false;
            arrowIndicator.SetArrowEnabled(false);
            return;
        }

        GameObject currentTarget = routes[currentRouteIndex].Levels[currentLevelIndex].Targets[currentTargetIndex].PositionObject;

        if (NavMesh.CalculatePath(transform.position, currentTarget.transform.position, NavMesh.AllAreas, path))
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                if (path.corners.Length > 0)
                {

                    // Detectar puntos de giro en el camino
                    List<Vector3> turningPoints = GetTurningPoints(path.corners);

                    // Revisar si el usuario est� cerca de un punto de giro
                    for (int i = 0; i < turningPoints.Count; i++)
                    {
                        if (IsNearSegment(turningPoints[i],
                                          (i < turningPoints.Count - 1) ? turningPoints[i + 1] : currentTarget.transform.position,
                                          transform.position,
                                          1.0f)) // Ajusar el umbral seg�n sea necesario
                        {
                            // Si esta cerca de un segmento, apuntar hacia el siguiente punto de giro
                            if (i + 1 < turningPoints.Count)
                            {
                                arrowIndicator.SetTarget(turningPoints[i + 1], arCamera);
                                return;
                            }
                        }
                    }

                    // Si no esta cerca de un segmento, apuntar hacia el primer punto de giro
                    arrowIndicator.SetTarget(turningPoints[0], arCamera);

                    line.positionCount = path.corners.Length;
                    line.SetPositions(path.corners);
                    /*line.enabled = true;*/ //descomentar esta linea para que se muestre la linea
                }
                else
                {
                    Debug.LogWarning(NoPathFoundMessage);
                    UpdateErrorLogText(NoPathFoundMessage);
                    line.enabled = false;
                    // Desactivar la flecha si no se encuentra un camino
                    /*arrowIndicator.SetArrowEnabled(false);*/
                }
            }
            else
            {
                Debug.LogError(PathCalculationErrorMessage);
                UpdateErrorLogText(PathCalculationErrorMessage);
                line.enabled = false;
                // Desactivar la flecha cuando no hay m�s objetivos en el nivel
                /*arrowIndicator.SetArrowEnabled(false);*/
            }
        }
        else
        {
            Debug.LogError(PathCalculationErrorMessage);
            UpdateErrorLogText(PathCalculationErrorMessage);
            line.enabled = false;
            // Desactivar la flecha cuando no hay m�s objetivos en el nivel
            /*arrowIndicator.SetArrowEnabled(false);*/
        }
    }

    // Funcion para detectar puntos de giro en el camino de navegaci�n
    List<Vector3> GetTurningPoints(Vector3[] corners)
    {
        List<Vector3> turningPoints = new List<Vector3>();

        for (int i = 1; i < corners.Length - 1; i++)
        {
            Vector3 previousSegment = (corners[i] - corners[i - 1]).normalized;
            Vector3 nextSegment = (corners[i + 1] - corners[i]).normalized;

            // Calcular el �ngulo entre los segmentos
            float angle = Vector3.Angle(previousSegment, nextSegment);

            // Si el �ngulo es mayor a 20 grados, considerar el punto como un punto de giro
            if (angle > 20f)
            {
                turningPoints.Add(corners[i]);
            }
        }

        // Asegurarse de agregar el �ltimo punto como un punto de giro
        if (corners.Length > 0)
        {
            turningPoints.Add(corners[corners.Length - 1]);
        }

        return turningPoints;
    }

    // Funci�n para detectar si un punto est� cerca de un segmento
    private bool IsNearSegment(Vector3 pointA, Vector3 pointB, Vector3 position, float threshold)
    {
        Vector3 closestPoint = Vector3.Project(position - pointA, (pointB - pointA).normalized) + pointA;
        float distanceToSegment = Vector3.Distance(closestPoint, position);

        // Revisar si el punto m�s cercano est� dentro del segmento
        if (Vector3.Dot(pointB - pointA, closestPoint - pointA) < 0 ||
            Vector3.Dot(pointA - pointB, closestPoint - pointB) < 0)
        {
            return distanceToSegment < threshold;
        }

        return distanceToSegment < threshold;
    }

    // Funci�n para manejar el bot�n de siguiente objetivo
    public void OnNextTargetButtonClicked()
    {
        // esconder el panel de qr scan
        qrScanPanel.SetActive(false);

        if (isFirstTarget)
        {
            Debug.Log("Es el primer objetivo.");
            isFirstTarget = false;
            nextTargetButton.gameObject.SetActive(false);
            StartPathFinding();
            return;
        }

        // revisar si hay m�s objetivos en el nivel actual
        if (currentTargetIndex < routes[currentRouteIndex].Levels[currentLevelIndex].Targets.Count - 1)
        {
            currentTargetIndex++;
            completedTargetsCount++;
            nextTargetButton.gameObject.SetActive(false);
            CalculateAndDrawPath();
            UpdateTargetCounterText();
        }
        else
        {
            // Si todos los objetivos del nivel actual han sido completados
            Debug.Log(string.Format(LevelCompleteMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex, routes[currentRouteIndex].Levels[currentLevelIndex + 1].LevelIndex));
            UpdateSuccessLogText(string.Format(LevelCompleteMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex, routes[currentRouteIndex].Levels[currentLevelIndex + 1].LevelIndex));
            line.enabled = false;
            arrowIndicator.SetArrowEnabled(false);
            nextTargetButton.gameObject.SetActive(false);

            // Mover al siguiente nivel
            currentLevelIndex++;
            completedTargetsCount++;
            currentTargetIndex = 0; // Reiniciar el indice del objetivo
            UpdateTargetCounterText(); // Actualizar el contador de objetivos
            CalculateAndDrawPath(); // Calcular el camino para el siguiente nivel
            UpdateLevelCounterText(); // Actualizar el contador de niveles
        }

        // Actualizar el porcentaje completado
        UpdateCompletionPercentage();
    }

    // Funci�n para obtener el total de objetivos en la ruta
    private int GetTotalTargets()
    {
        int totalTargets = 0;
        foreach (var level in routes[currentRouteIndex].Levels)
        {
            totalTargets += level.Targets.Count;
        }
        return totalTargets;
    }

    // Funci�n para actualizar el porcentaje completado
    private void UpdateCompletionPercentage()
    {
        int totalTargets = GetTotalTargets();
        float completionPercentage = (float)completedTargetsCount / totalTargets * 100f;
        // Castear a un entero para mostrar el porcentaje completado
        int completionPercentageInt = Mathf.FloorToInt(completionPercentage);

        Debug.Log("totalTargets: " + totalTargets);
        Debug.Log($"Porcentaje completado: {completionPercentage:F2}%");
        completionText.text = $"Completado: {completionPercentageInt}%";
    }

    // Funci�n para actualizar el texto de log de �xito
    private void UpdateSuccessLogText(string message)
    {
        logSuccessText.text += message + "\n"; // Agregar el mensaje al texto de log de �xito
    }

    // Funci�n para actualizar el texto de log de error
    private void UpdateErrorLogText(string message)
    {
        logErrorText.text += message + "\n"; // Agregar el mensaje al texto de log de error
    }

    // Funci�n para limpiar el texto de log
    private void ClearLogText()
    {
        logSuccessText.text = ""; // LImpiar el texto de log de �xito
        logErrorText.text = ""; // Limpiar el texto de log de error
    }

    // Funci�n para limpiar todos los textos
    private void ClearAllTexts()
    {
        distanceText.text = "";
        currentTargetText.text = "";
        targetCounterText.text = "";
        levelCounterText.text = "";
    }

    // Funci�n para actualizar el contador de objetivos
    private void UpdateTargetCounterText()
    {
        targetCounterText.text = $"Objetivo {currentTargetIndex + 1} de {routes[currentRouteIndex].Levels[currentLevelIndex].Targets.Count}"; // Update target counter
    }

    // Funci�n para actualizar el contador de niveles
    private void UpdateLevelCounterText()
    {
        // Actualizar el contador de niveles
        levelCounterText.text = $"Nivel {routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex}";
    }

    // Funci�n para actualizar el contador de rutas
    private void UpdateRouteCounterText()
    {
        routeCounterText.text = $"Ruta: {routes[currentRouteIndex].RouteName}";
    }

    // Funci�n para reiniciar el tour
    public void ResetTour()
    {
        // Reiniciar el camino y el LineRenderer
        path.ClearCorners(); // Limpiar los puntos del camino
        line.positionCount = 0; // Reiniciar la cantidad de posiciones
        line.enabled = false; // Desactivar el LineRenderer
        arrowIndicator.SetArrowEnabled(false);

        // Reiniciar la sesi�n AR
        if (session != null)
        {
            session.Reset();
        }

        // Desactivar la c�mara
        if (arCamera != null)
        {
            arCamera.enabled = false;
        }

        // Reiniciar los contadores
        currentRouteIndex = 0;
        currentLevelIndex = 0;
        currentTargetIndex = 0;

        // Reiniciar los flags
        isTourActive = false;
        completedLevel = false;
        completedTour = false;
        isFirstTarget = true;

        // LImpiar los textos
        ClearLogText();

        // Reiniciar los textos
        distanceText.text = "";
        currentTargetText.text = "";
        targetCounterText.text = "";
        levelCounterText.text = "";
        routeCounterText.text = "";

        // Escoder el bot�n de siguiente objetivo
        nextTargetButton.gameObject.SetActive(false);

        // Remover los listeners del bot�n de siguiente objetivo
        nextTargetButton.onClick.RemoveAllListeners();

        // Esconder el panel de qr scan
        qrScanPanel.SetActive(false);

        Debug.Log("Tour reset. Please select a route to start again.");
    }

    // Funci�n para dibujar el camino en el editor
    private void OnDrawGizmos()
    {
        if (path == null || path.corners.Length == 0)
        {
            return;
        }

        Gizmos.color = Color.red;

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
        }
    }
}
