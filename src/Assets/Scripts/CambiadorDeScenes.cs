using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CambiadorDeScenes : MonoBehaviour
{
    [SerializeField]
    private UIDocument uiDocument;

    public GameObject[] disableObjects;

    [SerializeField]
    private string nombreEscenaDestino;

    #if UNITY_EDITOR
    [SerializeField]
    private SceneAsset escenaDestino;
    #endif

    public static CambiadorDeScenes Instance { get; private set; }

    private const string NOMBRE_BOTON = "boton_ir_a_videojuegos";


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
    void Start()
    {
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument no asignado en CambiadorDeScenes.");
            return;
        }

        Button botonIrAVideojuegos = uiDocument.rootVisualElement.Q<Button>(NOMBRE_BOTON);

        //if (botonIrAVideojuegos != null)
        //{
        //    botonIrAVideojuegos.clicked += CambiarAEscenaDestino;
        //}
        //else
        //{
        //    Debug.LogError($"No se encontró el botón con nombre '{NOMBRE_BOTON}' en el UIDocument.");
        //}

        #if UNITY_EDITOR
        // Actualiza nombreEscenaDestino cuando se cambia escenaDestino en el editor
        if (escenaDestino != null)
        {
            nombreEscenaDestino = escenaDestino.name;
        }
        #endif
    }

    private void CambiarAEscenaDestino()
    {
        if (!string.IsNullOrEmpty(nombreEscenaDestino))
        {
            SceneManager.LoadScene(nombreEscenaDestino);
        }
        else
        {
            Debug.LogError("No se ha asignado un nombre de escena de destino en CambiadorDeScenes.");
        }
    }
    public void LoadMinigame(string minigameSceneName)
    {
        if (!string.IsNullOrEmpty(minigameSceneName))
        {

            foreach (GameObject obj in disableObjects)
            {
                obj.SetActive(false);
            }

            SceneManager.LoadScene(minigameSceneName, LoadSceneMode.Additive);
        }
        else
        {
            Debug.LogError("No se ha proporcionado un nombre de escena válido para el minijuego.");
        }
    }

    public void UnloadMinigame(string minigameSceneName)
    {
        if (!string.IsNullOrEmpty(minigameSceneName))
        {
            foreach (GameObject obj in disableObjects)
            {
                obj.SetActive(true);
            }

            SceneManager.UnloadSceneAsync(minigameSceneName);
        }
        else
        {
            Debug.LogError("No se ha proporcionado un nombre de escena válido para el minijuego.");
        }
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (escenaDestino != null)
        {
            nombreEscenaDestino = escenaDestino.name;
        }
    }
    #endif
}
