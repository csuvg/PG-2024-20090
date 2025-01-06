using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class SetNavigationTarget2 : MonoBehaviour
{
    public static SetNavigationTarget2 Instance; // Singleton instance

    [SerializeField]
    public List<Route> routes; // List of routes (previously levels)

    [SerializeField]
    private Camera arCamera; // Reference to the AR Camera or Main Camera
    [SerializeField]
    private ARSession session;
    /*[SerializeField]
    private ArrowIndicator arrowIndicator; // Reference to the ArrowIndicator script*/
    [SerializeField]
    private ArrowImageIndicator arrowIndicator; // Reference to the ArrowIndicator script
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
    private Text levelCounterText; // Reference to the level counter text
    [SerializeField]
    private Text routeCounterText; // Reference to the route counter text

    private NavMeshPath path;
    private LineRenderer line;
    private int currentRouteIndex = 0; // Index for the current route
    private int currentLevelIndex = 0; // Index for the current level within the route
    private int currentTargetIndex = 0; // Index to track the current target
    public bool lineToggle = true; // Make lineToggle public
    public bool completedLevel = false; // Make completedLevel public
    public bool completedTour = false; // Make completedTour public
    public bool isFirstTarget = true; // Make isFirstTarget public
    private bool isTourActive = false; // Flag to control when the tour starts

    private const string TourCompletedMessage = "Felicidades, Tour Completado!";
    private const string StartTourMessage = "Tour iniciado en el nivel {0}. Para comenzar escanee el Codigo QR!";
    private const string LevelCompleteMessage = "Nivel {0} Completado! Vaya al nivel {1}. Luego escanee el Codigo QR para continuar.";
    private const string NoPathFoundMessage = "No se encontró un camino hacia el objetivo.";
    private const string PathCalculationErrorMessage = "Error al calcular el camino.";

    private void Awake()
    {
        // Ensure that there's only one instance of SetNavigationTarget
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instance
        }
    }

    public void InitializeTour(int routeIndex)
    {
        if (routes == null || routes.Count == 0)
        {
            Debug.LogError("Rutas no están seteadas.");
            return;
        }

        // Activate the camera when the tour starts
        if (arCamera != null)
        {
            arCamera.enabled = true;
        }

        currentRouteIndex = routeIndex; // Set the current route index based on the user's selection
        currentLevelIndex = 0; // Reset to the first level
        currentTargetIndex = 0; // Reset to the first target

        Debug.Log($"Ruta seleccionada: {routes[currentRouteIndex].RouteName}");

        path = new NavMeshPath();
        line = GetComponent<LineRenderer>();

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

        // Clear any log messages
        ClearLogText();
        ClearAllTexts();

        // display correct texts
        UpdateRouteCounterText();

        // show qr scan panel
        qrScanPanel.SetActive(true);

        // display message to scan qr code 
        Debug.Log(string.Format(StartTourMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex));
        UpdateSuccessLogText(string.Format(StartTourMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex));

        Debug.Log("Se inicia el tour");

    }

    private void StartPathFinding()
    {
        Debug.Log("Iniciando el camino.");

        // Set the tour active and initialize the UI
        isTourActive = true;
        arrowIndicator.SetArrowEnabled(true); // Enable the arrow indicator

        UpdateTargetCounterText();
        UpdateLevelCounterText();

        // Start path calculation for the first target
        CalculateAndDrawPath();
    }

    void Start()
    {
        qrScanPanel.SetActive(false); // hide the qr scan panel
        arrowIndicator.SetArrowEnabled(false); // Disable the arrow indicator initially

        // Deactivate the camera initially
        if (arCamera != null)
        {
            arCamera.enabled = false;
        }
    }

    void Update()
    {
        // Only update if the tour is active
        if (!isTourActive)
        {
            return;
        }
        line.enabled = false; // comentar esta linea para que se muestre la linea

        // Check if all targets in the current level have been completed
        if (currentTargetIndex >= routes[currentRouteIndex].Levels[currentLevelIndex].Targets.Count)
        {
            // Check if it's the last level in the current route
            if (currentLevelIndex >= routes[currentRouteIndex].Levels.Count - 1)
            {
                line.enabled = false;
                arrowIndicator.SetArrowEnabled(false);
                completedTour = true;
                Debug.Log(TourCompletedMessage);
                UpdateSuccessLogText(TourCompletedMessage);
                return; // No more levels to navigate to
            }

            // If not the last level, display level complete message
            qrScanPanel.SetActive(true); // show the qr scan panel
            Debug.Log(string.Format(LevelCompleteMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex, routes[currentRouteIndex].Levels[currentLevelIndex + 1].LevelIndex));
            UpdateSuccessLogText(string.Format(LevelCompleteMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex, routes[currentRouteIndex].Levels[currentLevelIndex + 1].LevelIndex));
            line.enabled = false;
            arrowIndicator.SetArrowEnabled(false);
            nextTargetButton.gameObject.SetActive(false); // Hide the button
            return; // No more targets to navigate to
        }

        GameObject currentTarget = routes[currentRouteIndex].Levels[currentLevelIndex].Targets[currentTargetIndex].PositionObject;
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
        /*Debug.Log($"Distancia a objetivo: {distanceToTarget:F2} m");*/
        distanceText.text = $"Distancia a objetivo: {distanceToTarget:F2} m";
        currentTargetText.text = $"Objetivo actual: {routes[currentRouteIndex].Levels[currentLevelIndex].Targets[currentTargetIndex].Name}";

        // Check if within 1.2 meters to show the button
        if (distanceToTarget <= 1.2f)
        {
            if (currentTargetIndex < routes[currentRouteIndex].Levels[currentLevelIndex].Targets.Count - 1)
            {
                nextTargetButton.gameObject.SetActive(true); // Show the button for intermediate targets
                Debug.Log("Objetivo alcanzado. Presione el botón 'Siguiente objetivo'.");
                arrowIndicator.SetArrowEnabled(false);
            }
            else
            {
                nextTargetButton.gameObject.SetActive(false); // Hide button for the last target
                // Asegurarse de no activar la flecha si ya se alcanzó el último objetivo
                if (arrowIndicator != null && currentTargetIndex < routes[currentRouteIndex].Levels[currentLevelIndex].Targets.Count - 1)
                {
                    arrowIndicator.SetArrowEnabled(true);
                }
                currentTargetIndex++; // Move to the next target
            }
        }
        else
        {
            nextTargetButton.gameObject.SetActive(false);
            arrowIndicator.SetArrowEnabled(true);
            ClearLogText(); // Clear log text when out of range
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

        if (NavMesh.CalculatePath(transform.position, currentTarget.transform.position, NavMesh.AllAreas, path))
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                if (path.corners.Length > 0)
                {
                    // Detect points of significant change in direction
                    List<Vector3> turningPoints = GetTurningPoints(path.corners);

                    // Send the next turning point to the arrow indicator
                    if (arrowIndicator != null && turningPoints.Count > 0)
                    {
                        Debug.Log("Enviando punto de giro al indicador de flecha.");
                        arrowIndicator.SetTarget(turningPoints[0], arCamera); // Pass the camera as well
                    }

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
                    arrowIndicator.SetArrowEnabled(false);
                }
            }
            else
            {
                Debug.LogError(PathCalculationErrorMessage);
                UpdateErrorLogText(PathCalculationErrorMessage);
                line.enabled = false;
                // Desactivar la flecha cuando no hay más objetivos en el nivel
                arrowIndicator.SetArrowEnabled(false);
            }
        }
        else
        {
            Debug.LogError(PathCalculationErrorMessage);
            UpdateErrorLogText(PathCalculationErrorMessage);
            line.enabled = false;
            // Desactivar la flecha cuando no hay más objetivos en el nivel
            arrowIndicator.SetArrowEnabled(false);
        }
    }

    // Function to get points where the direction changes significantly
    List<Vector3> GetTurningPoints(Vector3[] corners)
    {
        List<Vector3> turningPoints = new List<Vector3>();

        for (int i = 1; i < corners.Length - 1; i++)
        {
            Vector3 previousSegment = (corners[i] - corners[i - 1]).normalized;
            Vector3 nextSegment = (corners[i + 1] - corners[i]).normalized;

            // Calculate the angle between the two segments
            float angle = Vector3.Angle(previousSegment, nextSegment);

            // If the angle is significant (e.g., greater than 30 degrees), consider it a turn
            if (angle > 30f)
            {
                turningPoints.Add(corners[i]);
            }
        }

        // Ensure the final destination is also a turning point
        if (corners.Length > 0)
        {
            turningPoints.Add(corners[corners.Length - 1]);
        }

        return turningPoints;
    }

    public void OnNextTargetButtonClicked()
    {
        //hide qr scan panel
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
            // If all targets in the current level are completed
            Debug.Log(string.Format(LevelCompleteMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex, routes[currentRouteIndex].Levels[currentLevelIndex + 1].LevelIndex));
            UpdateSuccessLogText(string.Format(LevelCompleteMessage, routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex, routes[currentRouteIndex].Levels[currentLevelIndex + 1].LevelIndex));
            line.enabled = false;
            arrowIndicator.SetArrowEnabled(false);
            nextTargetButton.gameObject.SetActive(false);

            // Move to the next level
            currentLevelIndex++;
            currentTargetIndex = 0; // Reset the target index for the new level
            UpdateTargetCounterText(); // Update target counter when moving to the next level
            CalculateAndDrawPath(); // Calculate path for the first target of the new level
            UpdateLevelCounterText(); // Update level counter when moving to the next level
        }
    }

    private void UpdateSuccessLogText(string message)
    {
        logSuccessText.text += message + "\n"; // Append the message to the success log text
    }

    private void UpdateErrorLogText(string message)
    {
        logErrorText.text += message + "\n"; // Append the message to the error log text
    }

    private void ClearLogText()
    {
        logSuccessText.text = ""; // Clear the success log text
        logErrorText.text = ""; // Clear the error log text
    }

    private void ClearAllTexts()
    {
        distanceText.text = "";
        currentTargetText.text = "";
        targetCounterText.text = "";
        levelCounterText.text = "";
    }

    private void UpdateTargetCounterText()
    {
        targetCounterText.text = $"Objetivo {currentTargetIndex + 1} de {routes[currentRouteIndex].Levels[currentLevelIndex].Targets.Count}"; // Update target counter
    }

    private void UpdateLevelCounterText()
    {
        // Update the level counter text to show the current level
        levelCounterText.text = $"Nivel {routes[currentRouteIndex].Levels[currentLevelIndex].LevelIndex}"; // Update level counter
    }
    private void UpdateRouteCounterText()
    {
        routeCounterText.text = $"Ruta: {routes[currentRouteIndex].RouteName}";
    }

    public void ResetTour()
    {
        // Reset NavMeshPath and LineRenderer
        path.ClearCorners(); // Clear any existing corners in the path
        line.positionCount = 0; // Reset the LineRenderer
        line.enabled = false; // Disable the line until the next route starts
        arrowIndicator.SetArrowEnabled(false);

        // Reset ARSession (uncommented)
        if (session != null)
        {
            session.Reset(); // Reset the AR session to reinitialize tracking.
        }

        // Deactivate the camera initially
        if (arCamera != null)
        {
            arCamera.enabled = false;
        }

        // Reset indices to start from the beginning
        currentRouteIndex = 0;
        currentLevelIndex = 0;
        currentTargetIndex = 0;

        // Reset flags
        isTourActive = false;
        completedLevel = false;
        completedTour = false;
        isFirstTarget = true; // Reset the first target flag for a new tour.

        // Clear log messages
        ClearLogText();

        // Reset UI elements
        distanceText.text = "";
        currentTargetText.text = "";
        targetCounterText.text = "";
        levelCounterText.text = "";
        routeCounterText.text = "";

        // Hide the "Next Target" button
        nextTargetButton.gameObject.SetActive(false);

        // Remove the previous listeners to avoid any issues
        nextTargetButton.onClick.RemoveAllListeners();

        // Hide QR scan panel
        qrScanPanel.SetActive(false);

        Debug.Log("Tour reset. Please select a route to start again.");
    }


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
