using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowImageIndicator : MonoBehaviour
{
    [SerializeField]
    private RawImage arrowImage; // Elemento de UI que representa la flecha

    private Vector3 targetPosition;
    private bool hasTarget = false;
    private Camera arCamera; // Referencia a la cámara AR
    [SerializeField]
    private float predictiveDistance = 1.5f;

    // Función para establecer el objetivo de la flecha
    public void SetTarget(Vector3 newTargetPosition, Camera camera)
    {
        targetPosition = newTargetPosition;
        arCamera = camera;
        hasTarget = true;
    }

    // Función para desactivar/activar la flecha
    public void SetArrowEnabled(bool enabled)
    {
        arrowImage.gameObject.SetActive(enabled);

    }

    // Función para obtener el estado de la flecha
    public bool GetArrowEnabled()
    {
        Debug.Log("Arrow is enabled: " + arrowImage.gameObject.activeSelf);
        return arrowImage.gameObject.activeSelf;
    }

    // Update se llama una vez por frame
    void Update()
    {
        // Asegurarse de que hay un objetivo y la flecha está activa
        if (!hasTarget || !arrowImage.gameObject.activeSelf)
        {
            return; 
        }

        // Calcular la dirección hacia el objetivo
        Vector3 directionToTarget = (targetPosition - arCamera.transform.position).normalized;

        // Predecir la posición del objetivo en el futuro
        Vector3 predictedTargetPosition = targetPosition + directionToTarget * predictiveDistance;

        // Proyectar la dirección de la cámara en el plano horizontal
        Vector3 forward = arCamera.transform.forward;
        forward.y = 0; // Mantener el vector horizontal
        Vector3 cameraToTargetDirection = Vector3.ProjectOnPlane(predictedTargetPosition - arCamera.transform.position, Vector3.up).normalized;

        // Calcular el ángulo entre la dirección de la cámara y la dirección al objetivo
        float angle = Vector3.SignedAngle(forward, cameraToTargetDirection, Vector3.up);

        // Aplicar la rotación a la flecha
        arrowImage.rectTransform.rotation = Quaternion.Euler(0, 0, -angle);
    }

    // Función para visualizar la posición del objetivo y la predicción
    private void OnDrawGizmos()
    {
        if (hasTarget)
        {
            // Dibujar una línea desde la cámara al objetivo
            Gizmos.color = Color.black;
            Gizmos.DrawLine(arCamera.transform.position, targetPosition);
            Gizmos.DrawSphere(targetPosition, 0.1f); // Visualizar el punto objetivo

            // Visualizar la posición predicha del objetivo
            Vector3 predictedTargetPosition = targetPosition + (targetPosition - arCamera.transform.position).normalized * predictiveDistance;
            Gizmos.DrawSphere(predictedTargetPosition, 0.1f); // Visualizar el punto predicho
            Gizmos.DrawLine(targetPosition, predictedTargetPosition); // Linea que conecta el objetivo con el punto predicho
        }
    }
}
