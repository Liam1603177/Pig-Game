using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    public Transform target; // El objetivo a seguir (el jugador)
    public float smoothSpeed = 0.125f; // Velocidad de suavizado
    public Vector3 offset; // Offset de la cámara respecto al objetivo
    
    [Header("Límites de Cámara (Opcional)")]
    public bool useLimits = false;
    public float minX, maxX, minY, maxY;
    
    void Start()
    {
        // Si no se asigna un target, buscar el jugador automáticamente
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("Target encontrado automáticamente: " + player.name);
            }
            else
            {
                Debug.LogWarning("No se encontró un objeto con tag 'Player'. Asigna el target manualmente.");
            }
        }
        
        // Si no hay offset definido, usar uno por defecto
        if (offset == Vector3.zero)
        {
            offset = new Vector3(0, 0, -10);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        
        // Calcular la posición deseada
        Vector3 desiredPosition = target.position + offset;
        
        // Aplicar límites si están activados
        if (useLimits)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }
        
        // Interpolar suavemente hacia la posición deseada
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // Asegurar que la Z se mantenga (para cámaras 2D)
        smoothedPosition.z = transform.position.z;
        
        // Aplicar la nueva posición
        transform.position = smoothedPosition;
    }
}
