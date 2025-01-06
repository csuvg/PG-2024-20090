using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class AStarPrueba : MonoBehaviour
{
    public static AStarPrueba Instance;
    AStarPathfinding aStarPathfinding = AStarPathfinding.Instance;

    [SerializeField]
    public List<Route> routes; 

    [SerializeField]
    private Camera arCamera; 
    [SerializeField]
    private ARSession session;
    [SerializeField]
    private ArrowImageIndicator arrowIndicator; 
    [SerializeField]
    private GameObject qrScanPanel;
    [SerializeField]
    private Button nextTargetButton;
    [SerializeField]
    private Text distanceText;
    [SerializeField]
    private Text logErrorText;
    [SerializeField]
    private Text logSuccessText;
    [SerializeField]
    private Text targetCounterText;
    [SerializeField]
    private Text currentTargetText;
    [SerializeField]
    private Text levelCounterText; 
    [SerializeField]
    private Text routeCounterText; 

    private NavMeshPath path;
    private LineRenderer line;
    private int currentRouteIndex = 0;
    private int currentLevelIndex = 0; 
    private int currentTargetIndex = 0; 
    public bool lineToggle = true; 
    public bool completedLevel = false; 
    public bool completedTour = false; 
    public bool isFirstTarget = true; 
    private bool isTourActive = false;

    private const string TourCompletedMessage = "Felicidades, Tour Completado!";
    private const string StartTourMessage = "Tour iniciado en el nivel {0}. Para comenzar escanee el Codigo QR!";
    private const string LevelCompleteMessage = "Nivel {0} Completado! Vaya al nivel {1}. Luego escanee el Codigo QR para continuar.";
    private const string NoPathFoundMessage = "No se encontró un camino hacia el objetivo.";
    private const string PathCalculationErrorMessage = "Error al calcular el camino.";

    // Singleton para asegurar que solo haya una instancia de SetNavigationTarget
    private void Awake()
    {
        // Assegurar que solo haya una instancia de este script
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Destruir el objeto si ya existe una instancia
        }
    }

    // Función para inicializar el tour
    public void InitializeTour(int routeIndex)
    {
        if (routes == null || routes.Count == 0)
        {
            Debug.LogError("Rutas no están seteadas.");
            return;
        }

 
        if (arCamera != null)
        {
            arCamera.enabled = true;
        }

        currentRouteIndex = routeIndex; 
        currentLevelIndex = 0; 
        currentTargetIndex = 0; 

        Debug.Log($"Ruta seleccionada: {routes[currentRouteIndex].RouteName}");

        path = new NavMeshPath();
        line = GetComponent<LineRenderer>();
        aStarPathfinding = GetComponent<AStarPathfinding>();

        if (line == null)
        {
            Debug.LogError("LineRenderer no encontrado.");
            return;
        }

        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.material = new Material(Shader.Find("Sprites/Default")) { color = Color.green };

        nextTargetButton.gameObject.SetActive(false);
        nextTargetButton.onClick.AddListener(OnNextTargetButtonClicked);

        ClearLogText();
        ClearAllTexts();

        UpdateRouteCounterText();

        
        qrScanPanel.SetActive(true);

        Debug.Log(string.Format(StartTourMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex));
        UpdateSuccessLogText(string.Format(StartTourMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex));

        Debug.Log("Se inicia el tour");

    }

    private void StartPathFinding()
    {
        Debug.Log("Iniciando el camino.");


        isTourActive = true;
        arrowIndicator.SetArrowEnabled(true); 

        UpdateTargetCounterText();
        UpdateLevelCounterText();

       
        CalculateAndDrawPath();
    }

    void Start()
    {
        qrScanPanel.SetActive(false); 
        arrowIndicator.SetArrowEnabled(false); 

      
        if (arCamera != null)
        {
            arCamera.enabled = false;
        }
    }

    void Update()
    {
       
        // Si el tour no está activo, no hacer nada
        if (!isTourActive)
        {
            return;
        }
        /*line.enabled = false;*/ // comentar esta linea para que se muestre la linea

       // revisar si el tour ha sido completado
        if (currentTargetIndex >= routes[currentRouteIndex].Levels[currentLevelIndex].Targets.Count)
        {
            // revisar si el nivel ha sido completado
            if (currentLevelIndex >= routes[currentRouteIndex].Levels.Count - 1)
            {
                line.enabled = false;
                arrowIndicator.SetArrowEnabled(false);
                completedTour = true;
                Debug.Log(TourCompletedMessage);
                UpdateSuccessLogText(TourCompletedMessage);
                return; 
            }

            // si no es el último nivel, pasar al siguiente nivel
            qrScanPanel.SetActive(true); 
            Debug.Log(string.Format(LevelCompleteMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex, routes[currentRouteIndex].Levels[currentLevelIndex + 1].LevelIndex));
            UpdateSuccessLogText(string.Format(LevelCompleteMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex, routes[currentRouteIndex].Levels[currentLevelIndex + 1].LevelIndex));
            line.enabled = false;
            arrowIndicator.SetArrowEnabled(false);
            nextTargetButton.gameObject.SetActive(false); 
            return; 
        }

        GameObject currentTarget = routes[currentRouteIndex].Levels[currentLevelIndex].Targets[currentTargetIndex].PositionObject;
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
        /*Debug.Log($"Distancia a objetivo: {distanceToTarget:F2} m");*/
        distanceText.text = $"Distancia a objetivo: {distanceToTarget:F2} m";
        currentTargetText.text = $"Objetivo actual: {routes[currentRouteIndex].Levels[currentLevelIndex].Targets[currentTargetIndex].Name}";

        // revisar si el objetivo ha sido alcanzado
        if (distanceToTarget <= 1.5f)
        {
            if (currentTargetIndex < routes[currentRouteIndex].Levels[currentLevelIndex].Targets.Count - 1)
            {
                nextTargetButton.gameObject.SetActive(true); 
                Debug.Log("Objetivo alcanzado. Presione el botón 'Siguiente objetivo'.");
                arrowIndicator.SetArrowEnabled(false);
            }
            else
            {
                nextTargetButton.gameObject.SetActive(false); 
                // Asegurarse de no activar la flecha si ya se alcanzó el último objetivo
                if (arrowIndicator != null && currentTargetIndex < routes[currentRouteIndex].Levels[currentLevelIndex].Targets.Count - 1)
                {
                    arrowIndicator.SetArrowEnabled(true);
                }
                currentTargetIndex++; 
            }
        }
        else
        {
            nextTargetButton.gameObject.SetActive(false);
            arrowIndicator.SetArrowEnabled(true);
            ClearLogText(); 
        }

        if (lineToggle)
        {
            CalculateAndDrawPath();
        }

        /*if (arrowIndicator.GetArrowEnabled())
        {
            CalculateAndDrawPath();
        }*/
    }

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

        Debug.Log("Start position: " + transform.position);
        Debug.Log("End position: " + currentTarget.transform.position);
        // Utilizar A* para encontrar el camino
        List<Node> pathNodes = aStarPathfinding.FindPath(transform.position, currentTarget.transform.position);

        if (pathNodes != null && pathNodes.Count > 0)
        {
            // Crear un arreglo de Vector3 para almacenar las esquinas del camino
            Vector3[] corners = new Vector3[pathNodes.Count];
            for (int i = 0; i < pathNodes.Count; i++)
            {
                corners[i] = pathNodes[i].position; 
            }

            // Detectar los puntos de giro en el camino
            List<Vector3> turningPoints = GetTurningPoints(corners);

            // enviar el primer punto de giro al indicador de flecha
            if (arrowIndicator != null && turningPoints.Count > 0)
            {
                Debug.Log("Enviando punto de giro al indicador de flecha.");
                arrowIndicator.SetTarget(turningPoints[0], arCamera); 
            }

            line.positionCount = corners.Length;
            line.SetPositions(corners);
            line.enabled = true; // Descomentar esta linea para que se muestre la linea
        }
        else
        {
            Debug.LogWarning(NoPathFoundMessage);
            UpdateErrorLogText(NoPathFoundMessage);
            line.enabled = false;
            // Desactivar la flecha si no se encuentra un camino
            arrowIndicator.SetArrowEnabled(false);
        }
    }


    // Funcion para obtener los puntos de giro en el camino
    List<Vector3> GetTurningPoints(Vector3[] corners)
    {
        List<Vector3> turningPoints = new List<Vector3>();

        for (int i = 1; i < corners.Length - 1; i++)
        {
            Vector3 previousSegment = (corners[i] - corners[i - 1]).normalized;
            Vector3 nextSegment = (corners[i + 1] - corners[i]).normalized;

            // Calcular el ángulo entre los segmentos
            float angle = Vector3.Angle(previousSegment, nextSegment);

            // si el ángulo es mayor a 30 grados, agregar el punto de giro
            if (angle > 20f)
            {
                turningPoints.Add(corners[i]);
            }
        }

        // asegurarse de que el último punto sea el último punto del camino
        if (corners.Length > 0)
        {
            turningPoints.Add(corners[corners.Length - 1]);
        }

        return turningPoints;
    }

    // función para manejar el botón de "Siguiente objetivo"
    public void OnNextTargetButtonClicked()
    {
        // esconder el panel de escaneo QR
        qrScanPanel.SetActive(false);

        if (isFirstTarget)
        {
            Debug.Log("Es el primer objetivo.");
            isFirstTarget = false;
            nextTargetButton.gameObject.SetActive(false);
            StartPathFinding();
            return;
        }

        if (currentTargetIndex < routes[currentRouteIndex].Levels[currentLevelIndex].Targets.Count - 1)
        {
            currentTargetIndex++;
            nextTargetButton.gameObject.SetActive(false);
            CalculateAndDrawPath();
            UpdateTargetCounterText();
        }
        else
        {
            // si se alcanza el último objetivo, pasar al siguiente nivel
            Debug.Log(string.Format(LevelCompleteMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex, routes[currentRouteIndex].Levels[currentLevelIndex + 1].LevelIndex));
            UpdateSuccessLogText(string.Format(LevelCompleteMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex, routes[currentRouteIndex].Levels[currentLevelIndex + 1].LevelIndex));
            line.enabled = false;
            arrowIndicator.SetArrowEnabled(false);
            nextTargetButton.gameObject.SetActive(false);

            // Movers al siguiente nivel
            currentLevelIndex++;
            currentTargetIndex = 0; 
            UpdateTargetCounterText(); 
            CalculateAndDrawPath();
            UpdateLevelCounterText(); 
        }
    }

    // función para actualizar el texto de éxito en el log
    private void UpdateSuccessLogText(string message)
    {
        logSuccessText.text += message + "\n"; 
    }

    // función para actualizar el texto de error en el log
    private void UpdateErrorLogText(string message)
    {
        logErrorText.text += message + "\n"; 
    }

    // función para limpiar el texto del log
    private void ClearLogText()
    {
        logSuccessText.text = ""; 
        logErrorText.text = ""; 
    }

    // función para limpiar todos los textos
    private void ClearAllTexts()
    {
        distanceText.text = "";
        currentTargetText.text = "";
        targetCounterText.text = "";
        levelCounterText.text = "";
    }

    // función para actualizar el contador de objetivos
    private void UpdateTargetCounterText()
    {
        targetCounterText.text = $"Objetivo {currentTargetIndex + 1} de {routes[currentRouteIndex].Levels[currentLevelIndex].Targets.Count}"; // Update target counter
    }

    // función para actualizar el contador de niveles
    private void UpdateLevelCounterText()
    {
        // actualizar el contador de nivel
        levelCounterText.text = $"Nivel {routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex}"; 
    }

    // función para actualizar el contador de rutas
    private void UpdateRouteCounterText()
    {
        routeCounterText.text = $"Ruta: {routes[currentRouteIndex].RouteName}";
    }

    // función para reiniciar el tour
    public void ResetTour()
    {
        // Reiniciar el tour
        path.ClearCorners(); 
        line.positionCount = 0; 
        line.enabled = false;
        arrowIndicator.SetArrowEnabled(false);

        // Reiniciar la sesión AR
        if (session != null)
        {
            session.Reset(); 
        }

        // Desactivar la cámara AR
        if (arCamera != null)
        {
            arCamera.enabled = false;
        }

        // reiniciar los índices
        currentRouteIndex = 0;
        currentLevelIndex = 0;
        currentTargetIndex = 0;

        // reiniciar las banderas
        isTourActive = false;
        completedLevel = false;
        completedTour = false;
        isFirstTarget = true; 

        // limpiar el log
        ClearLogText();

        // Reiniciar UI elementos
        distanceText.text = "";
        currentTargetText.text = "";
        targetCounterText.text = "";
        levelCounterText.text = "";
        routeCounterText.text = "";

        nextTargetButton.gameObject.SetActive(false);

        // Remover los listeners
        nextTargetButton.onClick.RemoveAllListeners();

        // escanear el panel QR
        qrScanPanel.SetActive(false);

        Debug.Log("Tour reset. Please select a route to start again.");
    }

    // función para dibujar el camino en el editor
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
