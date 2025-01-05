using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Assets.Minijuegos.Scripts
{
    public class Scene : MonoBehaviour
    {
        public static Scene Instance { get; private set; }
        public string gameMenuName;
        public string menuName;

        [SerializeField]
        public GameObject[] disableObjects;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoadScene(string sceneName)
        {

            if (ControladorVistas.Instance != null)
                ControladorVistas.Instance.GuardarEstadoVistas();

            // Hide UI and disable input for the current scene's UI
            foreach (GameObject obj in disableObjects)
            {
                UIDocument uiDocument = obj.GetComponent<UIDocument>();

                if (uiDocument != null)
                {
                    uiDocument.rootVisualElement.style.display = DisplayStyle.None;
                    uiDocument.rootVisualElement.pickingMode = PickingMode.Ignore;
                }
                else
                {
                    obj.SetActive(false);
                }
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        public void UnloadScene(string sceneName)
        {
            // Show UI and re-enable input for the main scene's UI
            foreach (GameObject obj in disableObjects)
            {
                UIDocument uiDocument = obj.GetComponent<UIDocument>();
                Console.Error.WriteLine(obj.name + " " + uiDocument);

                if (uiDocument != null)
                {
                    uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
                    uiDocument.rootVisualElement.pickingMode = PickingMode.Position;
                }
                else
                {
                    obj.SetActive(true);
                }
            }

            SceneManager.UnloadSceneAsync(sceneName);

            if (ControladorVistas.Instance != null)
                ControladorVistas.Instance.RestaurarEstadoVistas();
        }

        public void LoadMenu()
        {
            Destroy(gameObject);
        }
    }
}
