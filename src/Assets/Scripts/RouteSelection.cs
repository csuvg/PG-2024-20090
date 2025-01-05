using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RouteSelection : MonoBehaviour
{
    [SerializeField]
    private GameObject routeSelectionPanel; // Panel que contiene la selecci�n de rutas
    [SerializeField]
    private Dropdown routeDropdown; // Dropdown para seleccionar la ruta
    [SerializeField]
    private Button startTourButton; // Boton para iniciar el tour
    [SerializeField]
    private Text selectedRouteText; // Texto que muestra la ruta seleccionada
    [SerializeField]
    private Button resetButton;
    [SerializeField]
    private List<Route> availableRoutes;

    private int selectedRouteIndex;

    // Start se llama antes de la primera actualizaci�n del frame
    void Start()
    {
        routeSelectionPanel.SetActive(false); //Mostar el panel de selecci�n de rutas al inicio

        selectedRouteText.text = ""; // Limpiar el texto de la ruta seleccionada
        availableRoutes = SetNavigationTarget.Instance.routes; // Obtener las rutas disponibles

        // Popular el dropdown con las rutas disponibles
        List<string> routeNames = new List<string>();
        foreach (var route in availableRoutes)
        {
            routeNames.Add(route.RouteName);
        }
        routeDropdown.AddOptions(routeNames);

        // Setear el listener para el dropdown
        routeDropdown.onValueChanged.AddListener(OnRouteSelected);

        startTourButton.gameObject.SetActive(false); // Setear el bot�n de inicio de tour como inactivo
        startTourButton.onClick.AddListener(StartTour);

        resetButton.gameObject.SetActive(false);
        resetButton.onClick.AddListener(ResetPanel);
    }

    // Update se llama una vez por frame
    void Update()
    {
        // revisar si el tour ha sido completado
        if (SetNavigationTarget.Instance.completedTour)
        {
            // si es verdadero, mostrar el bot�n de reset
            resetButton.gameObject.SetActive(true);
        }
        else
        {
            // si es falso, ocultar el bot�n de reset
            resetButton.gameObject.SetActive(false);
        }

    }

    // Funcion para manejar la selecci�n de rutas
    void OnRouteSelected(int index)
    {
        selectedRouteIndex = index;
        selectedRouteText.text = $"Ruta seleccionada: {availableRoutes[selectedRouteIndex].RouteName}";
        startTourButton.gameObject.SetActive(true); // Mostrar el bot�n de inicio de tour
    }

    // Funcion para Iniciar el tour
    void StartTour()
    {
        SetNavigationTarget.Instance.InitializeTour(selectedRouteIndex);
        routeSelectionPanel.SetActive(false); // Ocultar el panel de selecci�n de rutas
    }

    // Funci�n para reiniciar el panel
    void ResetPanel()
    {
        // Reiniciar el tour llamando al m�todo ResetTour en SetNavigationTarget
        SetNavigationTarget.Instance.ResetTour();

        // Mostrar el panel de selecci�n de rutas
        routeSelectionPanel.SetActive(true);
        selectedRouteText.text = "";
    }
}
