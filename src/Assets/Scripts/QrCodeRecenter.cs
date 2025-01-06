using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;

public class QrCodeRecenter : MonoBehaviour
{
    [SerializeField]
    private ARSession session;
    [SerializeField]
    private XROrigin sessionOrigin;
    [SerializeField]
    private ARCameraManager cameraManager;
    [SerializeField]
    private Text logText;
    [SerializeField]
    private List<Target> navigationTargetObjects = new List<Target>();


    public static QrCodeRecenter Instance;

    private Texture2D cameraImageTexture;
    private IBarcodeReader reader = new BarcodeReader();

    private bool positionSet = false; // Variable para saber si la posici�n ha sido reorientada


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


    // Suscribirse al evento frameReceived
    private void OnEnable()
    {

        Debug.Log("OnEnable QRCodeRecenter");

        // if (!SetNavigationTarget.Instance.IsSearchingForQRCode) return; // Evitar reorientar la posici�n si no se est� buscando un QR

        Debug.Log("SetNavigationTarget TRUE >>>>> Buscando QR...");

        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    // Suscribirse al evento frameReceived
    private void OnDisable()
    {
        // cameraManager.frameReceived -= OnCameraFrameReceived;

        cameraManager.frameReceived -= OnCameraFrameReceived;

        // Destruir la textura para liberar memoria
        if (cameraImageTexture != null)
        {
            Destroy(cameraImageTexture);
            cameraImageTexture = null;
        }
    }

    // Funci�n para detectar el QR y reorientar la posici�n
    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {

        Debug.Log("OnCameraFrameReceived Revisando >>>>>");

        if (!SetNavigationTarget.Instance.IsSearchingForQRCode) return; // Evitar reorientar la posici�n si no se est� buscando un QR

        Debug.Log("SetNavigationTarget TRUE >>>>> Buscando QR...");

        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            return;
        }

        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(image.width / 2, image.height / 2),
            outputFormat = TextureFormat.RGBA32,
            transformation = XRCpuImage.Transformation.MirrorY
        };

        int size = image.GetConvertedDataSize(conversionParams);
        var buffer = new NativeArray<byte>(size, Allocator.Temp);
        image.Convert(conversionParams, buffer);
        image.Dispose();

        // cameraImageTexture = new Texture2D(
        //     conversionParams.outputDimensions.x,
        //     conversionParams.outputDimensions.y,
        //     conversionParams.outputFormat,
        //     false);

        // cameraImageTexture.LoadRawTextureData(buffer);
        // cameraImageTexture.Apply();
        // buffer.Dispose();

        // Verificar si la textura existe y es del tamaño correcto
        if (cameraImageTexture == null ||
            cameraImageTexture.width != conversionParams.outputDimensions.x ||
            cameraImageTexture.height != conversionParams.outputDimensions.y ||
            cameraImageTexture.format != conversionParams.outputFormat)
        {
            // Destruir la textura anterior si existe
            if (cameraImageTexture != null)
            {
                Destroy(cameraImageTexture);
            }

            // Crear una nueva instancia de la textura con los parámetros correctos
            cameraImageTexture = new Texture2D(
                conversionParams.outputDimensions.x,
                conversionParams.outputDimensions.y,
                conversionParams.outputFormat,
                false);
        }

        // Actualizar los datos de la textura
        cameraImageTexture.LoadRawTextureData(buffer);
        cameraImageTexture.Apply();
        buffer.Dispose();









        var result = reader.Decode(cameraImageTexture.GetPixels32(), cameraImageTexture.width, cameraImageTexture.height);
        if (result != null)
        {
            Debug.Log($"QR detectado: {result.Text}");
            SetQrCodeRecenterTarget(result.Text);
        }
        else
        {
            Debug.LogWarning("Ningun QR detectado.");
            UpdateLog("Ningun QR detectado.");
        }
    }

    // Funci�n para reorientar la posici�n del usuario
    private void SetQrCodeRecenterTarget(string targetText)
    {
        if (positionSet) return; // Evitar reorientar la posici�n si ya ha sido reorientada

        Debug.Log("positionSet: " + positionSet);

        if (!SetNavigationTarget.Instance.IsSearchingForQRCode) return; // Evitar reorientar la posici�n si no se est� buscando un QR

        Debug.Log($"[ SetQrCodeRecenterTarget ] >>>>>>>> Buscando objetivo: {targetText}");

        Target currentTarget = navigationTargetObjects.Find(x => x.Name.ToLower().Equals(targetText.ToLower()));
        if (currentTarget != null)
        {
            session.Reset();
            Vector3 newPosition = currentTarget.PositionObject.transform.position;
            newPosition.y += 1.00f;
            sessionOrigin.transform.position = newPosition;
            sessionOrigin.transform.rotation = currentTarget.PositionObject.transform.rotation;

            Debug.Log($"Reorientado a objetivo: {currentTarget.Name}");
            UpdateLog($"Exitosamente reorientado a: {currentTarget.Name}");

            positionSet = true; //Marcar la posici�n como reorientada
            SetNavigationTarget.Instance.OnNextTargetButtonClicked(); // Llamar al m�todo OnNextTargetButtonClicked en el script SetNavigationTarget
            SetLineToggleTrue(); // Set lineToggle to true
            StartCoroutine(ResetPositionSet()); // Empezar la corrutina para reiniciar la posici�n
        }
        else
        {
            Debug.LogWarning($"No se encontr� un objetivo para: {targetText}");
            UpdateLog($"No se encontr� un objetivo para: {targetText}");
        }
    }

    // Funci�n para cambiar el nivel activo
    public void ChangeActiveFloor(int floorEntrance)
    {
        string targetText = "";

        if (floorEntrance == 0)
        {
            targetText = "ElevadoresSec2";
            Debug.Log($"Cambiando nivel activo al: {targetText}");
            SetQrCodeRecenterTarget(targetText);

        } else if (floorEntrance == 8) {

            targetText = "ElevadoresTer1";
            Debug.Log($"Cambiando nivel activo al: {targetText}");
            SetQrCodeRecenterTarget(targetText);

        } else if (floorEntrance == 9) {

            targetText = "ElevadoresSec3";
            Debug.Log($"Cambiando nivel activo al: {targetText}");
            SetQrCodeRecenterTarget(targetText);

        } else if (floorEntrance == 10) {

            targetText = "ElevadoresPrin4";
            Debug.Log($"Cambiando nivel activo al: {targetText}");
            SetQrCodeRecenterTarget(targetText);

        } else if (floorEntrance == 11) {

            targetText = "ElevadoresSec6";
            Debug.Log($"Cambiando nivel activo al: {targetText}");
            SetQrCodeRecenterTarget(targetText);

        } else
        {
            targetText = $"Elevadores{floorEntrance}";
            Debug.Log($"Cambiando nivel activo al: {targetText}");
            SetQrCodeRecenterTarget(targetText);
        }

    }

    // Corrutina para reiniciar la posici�n
    private IEnumerator ResetPositionSet()
    {
        yield return new WaitForSeconds(10); // Esperar 10 segundos
        positionSet = false; // Reiniciar la variable positionSet
        Debug.Log("Posicion reiniciada despues del delay.");
    }

    // Funcion para activar la variable lineToggle en SetNavigationTarget
    private void SetLineToggleTrue()
    {
        SetNavigationTarget.Instance.lineToggle = true; // Setear lineToggle a true
    }

    // Funci�n para actualizar el texto en el objeto logText
    private void UpdateLog(string message)
    {
        logText.text = message; // Actualizar el texto en el objeto logText
    }
}