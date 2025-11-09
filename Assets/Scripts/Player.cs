using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidadMovimiento = 5f;
    public float fuerzaSalto = 10f;
    
    [Header("Configuración de Salto")]
    public Transform verificadorSuelo;
    public float radioVerificacion = 0.2f;
    public LayerMask capaSuelo = 1; // Capa "Default" por defecto
    
    [Header("Configuración Avanzada de Suelo")]
    public float distanciaVerificacion = 0.1f;
    
    [Header("Debug")]
    public bool mostrarDebug = true;
    
    [Header("Monedas")]
    public int monedasRecolectadas = 0;
    public TextMeshProUGUI textCoins; // Referencia al texto de UI (TextMeshPro)
    
    private Rigidbody2D rb;
    private bool enElSuelo;
    private bool mirandoDerecha = true; // Para saber hacia dónde está mirando
    private Animator animator;
    
    void Start()
    {
        // Obtener el componente Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        
        // Obtener el componente Animator
        animator = GetComponent<Animator>();
        
        // Verificar que tenemos Rigidbody2D
        if (rb == null)
        {
            Debug.LogError("¡El personaje necesita un componente Rigidbody2D!");
            return;
        }
        
        // Verificar que tenemos Animator (opcional)
        if (animator == null)
        {
            Debug.LogWarning("No se encontró componente Animator. Las animaciones no funcionarán.");
        }
        
        // Si no hay un verificador de suelo asignado, buscar GroundCheck
        if (verificadorSuelo == null)
        {
            // Buscar el GroundCheck que ya existe como hijo
            Transform groundCheck = transform.Find("GroundCheck");
            if (groundCheck != null)
            {
                verificadorSuelo = groundCheck;
                if (mostrarDebug) Debug.Log("GroundCheck encontrado y asignado automáticamente");
            }
            else
            {
                // Si no existe, crear uno
                GameObject verificador = new GameObject("GroundCheck");
                verificador.transform.SetParent(transform);
                verificador.transform.localPosition = new Vector3(0, -0.5f, 0);
                verificadorSuelo = verificador.transform;
                if (mostrarDebug) Debug.Log("GroundCheck creado automáticamente");
            }
        }
        
        if (mostrarDebug)
        {
            Debug.Log("Player inicializado correctamente");
        }
        
        // Actualizar el texto de monedas al inicio
        ActualizarTextoMonedas();
    }

    void Update()
    {
        if (rb == null) return;
        
        // Verificar si está en el suelo usando múltiples métodos
        enElSuelo = VerificarSuelo();
        
        // Debug visual del estado del suelo
        if (mostrarDebug)
        {
            Debug.Log($"En el suelo: {enElSuelo}");
        }
        
        // Movimiento horizontal usando el nuevo Input System
        float movimientoHorizontal = 0f;
        
        // Usar el nuevo Input System
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed)
            {
                movimientoHorizontal = -1f;
                if (mostrarDebug) Debug.Log("Moviendo a la izquierda (Nuevo Input System)");
            }
            
            if (Keyboard.current.dKey.isPressed)
            {
                movimientoHorizontal = 1f;
                if (mostrarDebug) Debug.Log("Moviendo a la derecha (Nuevo Input System)");
            }
        }
        
        // Fallback al sistema antiguo si el nuevo no está disponible
        try
        {
            if (movimientoHorizontal == 0f)
            {
                if (Input.GetKey(KeyCode.A))
                {
                    movimientoHorizontal = -1f;
                    if (mostrarDebug) Debug.Log("Moviendo a la izquierda (Sistema antiguo)");
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    movimientoHorizontal = 1f;
                    if (mostrarDebug) Debug.Log("Moviendo a la derecha (Sistema antiguo)");
                }
            }
        }
        catch (System.Exception)
        {
            // Si falla el sistema antiguo, no importa, usamos el nuevo
        }
        
        // Aplicar movimiento horizontal
        Vector2 velocidadActual = rb.linearVelocity;
        rb.linearVelocity = new Vector2(movimientoHorizontal * velocidadMovimiento, velocidadActual.y);
        
        // Actualizar animaciones
        if (animator != null)
        {
            // Configurar la velocidad de movimiento para la animación
            animator.SetFloat("Speed", Mathf.Abs(movimientoHorizontal));
            
            // Configurar velocidad vertical para animación de salto/caída
            animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
            
            // Configurar si está en el suelo
            animator.SetBool("IsGrounded", enElSuelo);
        }
        
        // Voltear el personaje según la dirección
        if (movimientoHorizontal > 0 && !mirandoDerecha)
        {
            Voltear();
        }
        else if (movimientoHorizontal < 0 && mirandoDerecha)
        {
            Voltear();
        }
        
        // Salto con tecla W
        bool saltar = false;
        if (Keyboard.current != null && Keyboard.current.wKey.wasPressedThisFrame)
        {
            saltar = true;
        }
        else
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.W))
                {
                    saltar = true;
                }
            }
            catch (System.Exception)
            {
                // Ignorar errores del sistema antiguo
            }
        }
        
        // Solo saltar si está en el suelo
        if (saltar && enElSuelo)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            if (mostrarDebug) Debug.Log("¡Saltando desde el suelo!");
        }
        else if (saltar && !enElSuelo)
        {
            if (mostrarDebug) Debug.Log("No puedo saltar: no estoy en el suelo");
        }
        
        // Debug adicional
        if (mostrarDebug && movimientoHorizontal != 0f)
        {
            Debug.Log($"Movimiento: {movimientoHorizontal}, Velocidad: {rb.linearVelocity}");
        }
    }
    
    // Función para verificar si está en el suelo de manera más precisa
    bool VerificarSuelo()
    {
        // Método 1: OverlapCircle (método principal)
        bool circulo = Physics2D.OverlapCircle(verificadorSuelo.position, radioVerificacion, capaSuelo);
        
        // Método 2: Raycast hacia abajo para mayor precisión
        RaycastHit2D raycast = Physics2D.Raycast(transform.position, Vector2.down, distanciaVerificacion, capaSuelo);
        
        // Método 3: Verificar la velocidad vertical (si está cayendo muy rápido, no está en el suelo)
        bool velocidadOK = rb.linearVelocity.y <= 0.1f; // Solo si no está subiendo rápido
        
        // Combinar todos los métodos para mayor precisión
        bool resultado = (circulo || raycast.collider != null) && velocidadOK;
        
        if (mostrarDebug && resultado != enElSuelo)
        {
            Debug.Log($"Cambio de estado del suelo: {resultado} (Círculo: {circulo}, Raycast: {raycast.collider != null}, Velocidad: {velocidadOK})");
        }
        
        return resultado;
    }
    
    // Función para voltear el personaje
    void Voltear()
    {
        // Cambiar la dirección
        mirandoDerecha = !mirandoDerecha;
        
        // Voltear el sprite multiplicando la escala X por -1
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
        
        if (mostrarDebug)
        {
            Debug.Log($"Personaje volteado. Ahora mira hacia: {(mirandoDerecha ? "derecha" : "izquierda")}");
        }
    }
    
    // Detectar colisiones con monedas y pinchos
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Coin"))
        {
            // Incrementar contador de monedas
            monedasRecolectadas++;
            
            // Actualizar el texto de la UI
            ActualizarTextoMonedas();
            
            if (mostrarDebug)
            {
                Debug.Log($"¡Moneda recolectada! Total: {monedasRecolectadas}");
            }
            
            // Destruir la moneda
            Destroy(collision.gameObject);
        }
    }
    
    // Detectar colisiones físicas con pinchos (Box Collider 2D)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Spikes"))
        {
            // El jugador tocó los pinchos
            if (mostrarDebug)
            {
                Debug.Log("¡Tocaste los pinchos! Reiniciando nivel...");
            }
            
            // Reiniciar el nivel actual
            ReiniciarNivel();
        }
    }
    
    // Función para reiniciar el nivel
    void ReiniciarNivel()
    {
        // Obtener el nombre de la escena actual y recargarla
        string escenaActual = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(escenaActual);
    }
    
    // Función para actualizar el texto de monedas
    void ActualizarTextoMonedas()
    {
        if (textCoins != null)
        {
            textCoins.text = monedasRecolectadas.ToString();
        }
    }
    
    // Dibujar el círculo de verificación del suelo en el editor
    void OnDrawGizmosSelected()
    {
        if (verificadorSuelo != null)
        {
            // Círculo de detección
            Gizmos.color = enElSuelo ? Color.green : Color.red;
            Gizmos.DrawWireSphere(verificadorSuelo.position, radioVerificacion);
            
            // Raycast de detección
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * distanciaVerificacion);
            
            // Texto informativo
            if (Application.isPlaying)
            {
                UnityEditor.Handles.Label(transform.position + Vector3.up, 
                    $"Suelo: {(enElSuelo ? "SÍ" : "NO")}\nVel.Y: {rb.linearVelocity.y:F2}");
            }
        }
    }
}
