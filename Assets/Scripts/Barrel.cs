using UnityEngine;

public class Barrel : MonoBehaviour
{
    [Header("Configuración de Rebote")]
    public float fuerzaRebote = 15f; // Fuerza del rebote hacia arriba
    public bool mostrarDebug = true;
    public bool destruirDespuesDeUsar = true; // Si se destruye después del rebote
    public float tiempoAntesDeDestruir = 0.5f; // Tiempo antes de destruirse
    
    [Header("Efectos (Opcional)")]
    public AudioClip sonidoRebote; // Sonido cuando rebota
    public ParticleSystem efectoRebote; // Efecto visual
    
    private AudioSource audioSource;
    private bool yaUsado = false; // Para evitar múltiples usos
    private Animator animator;
    
    void Start()
    {
        // Obtener componentes
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        
        if (mostrarDebug)
        {
            Debug.Log("Barril inicializado correctamente");
        }
    }
    
    // Detectar cuando el jugador toca la parte superior (trigger)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (mostrarDebug)
        {
            Debug.Log($"=== TRIGGER DETECTADO ===");
            Debug.Log($"Objeto: {collision.name}");
            Debug.Log($"Tag del objeto: '{collision.tag}'");
            Debug.Log($"Ya usado: {yaUsado}");
        }
        
        // Verificar que sea el jugador y no haya sido usado
        if ((collision.CompareTag("Player") || collision.name.Contains("character")) && !yaUsado)
        {
            if (mostrarDebug)
            {
                Debug.Log("✅ Es el jugador y no ha sido usado!");
            }
            
            // Obtener el Rigidbody2D del jugador
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            
            if (playerRb != null)
            {
                if (mostrarDebug)
                {
                    Debug.Log($"Velocidad Y del jugador: {playerRb.linearVelocity.y}");
                }
                
                // Solo aplicar rebote si el jugador está cayendo o con poca velocidad
                if (playerRb.linearVelocity.y <= 3f)
                {
                    yaUsado = true;
                    
                    if (mostrarDebug)
                    {
                        Debug.Log("🚀 APLICANDO REBOTE!!!");
                    }
                    
                    // Aplicar fuerza de rebote hacia arriba - MÉTODO MEJORADO
                    playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f); // Resetear velocidad Y
                    playerRb.AddForce(Vector2.up * fuerzaRebote, ForceMode2D.Impulse);
                    
                    if (mostrarDebug)
                    {
                        Debug.Log($"Nueva velocidad después del rebote: {playerRb.linearVelocity}");
                    }
                    
                    // Reproducir sonido si existe
                    if (audioSource != null && sonidoRebote != null)
                    {
                        audioSource.PlayOneShot(sonidoRebote);
                    }
                    
                    // Activar efecto visual si existe
                    if (efectoRebote != null)
                    {
                        efectoRebote.Play();
                    }
                    
                    // Activar la animación del barril
                    if (animator != null)
                    {
                        // Activar el Animator que ya tiene el barril configurado
                        animator.enabled = true;
                        
                        if (mostrarDebug)
                        {
                            Debug.Log("✅ Animator del barril activado!");
                        }
                        
                        // Si tiene el trigger Explode, también lo activamos
                        if (HasParameter(animator, "Explode"))
                        {
                            animator.SetTrigger("Explode");
                            if (mostrarDebug)
                            {
                                Debug.Log("✅ Trigger 'Explode' activado");
                            }
                        }
                    }
                    else
                    {
                        if (mostrarDebug)
                        {
                            Debug.LogWarning("⚠️ No se encontró Animator en el barril");
                        }
                    }
                    
                    // También mantener la animación por código como respaldo
                    AnimarBarril();
                    
                    // Destruir después del tiempo especificado
                    if (destruirDespuesDeUsar)
                    {
                        if (mostrarDebug)
                        {
                            Debug.Log($"💣 Programando destrucción en {tiempoAntesDeDestruir} segundos...");
                        }
                        Invoke("DestruirBarril", tiempoAntesDeDestruir);
                    }
                }
                else
                {
                    if (mostrarDebug)
                    {
                        Debug.Log("❌ No se aplicó rebote: jugador subiendo muy rápido");
                    }
                }
            }
            else
            {
                if (mostrarDebug)
                {
                    Debug.LogError("❌ No se encontró Rigidbody2D en el jugador!");
                }
            }
        }
        else
        {
            if (mostrarDebug)
            {
                if (yaUsado)
                {
                    Debug.Log("❌ Barril ya fue usado");
                }
                else
                {
                    Debug.Log($"❌ No es el jugador. Tag esperado: 'Player', Tag recibido: '{collision.tag}'");
                }
            }
        }
    }
    
    // Función para destruir el barril
    void DestruirBarril()
    {
        if (mostrarDebug)
        {
            Debug.Log("💥 ¡DESTRUYENDO BARRIL AHORA!");
        }
        
        // Cancelar cualquier animación en curso
        StopAllCoroutines();
        
        // Destruir el objeto inmediatamente
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
    
    // Función para verificar si un parámetro existe en el Animator
    private bool HasParameter(Animator anim, string paramName)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
    
    // Función para animar el barril cuando es tocado
    void AnimarBarril()
    {
        // Animación más dramática y visible
        StartCoroutine(AnimacionRebote());
    }
    
    // Corrutina para la animación mejorada
    System.Collections.IEnumerator AnimacionRebote()
    {
        Vector3 escalaOriginal = transform.localScale;
        
        if (mostrarDebug)
        {
            Debug.Log("🎬 Iniciando animación del barril...");
        }
        
        // FASE 1: Compresión dramática
        Vector3 escalaComprimida = new Vector3(escalaOriginal.x * 1.3f, escalaOriginal.y * 0.6f, escalaOriginal.z);
        float tiempo = 0f;
        float duracionCompresion = 0.15f;
        
        while (tiempo < duracionCompresion)
        {
            transform.localScale = Vector3.Lerp(escalaOriginal, escalaComprimida, tiempo / duracionCompresion);
            tiempo += Time.deltaTime;
            yield return null;
        }
        
        if (mostrarDebug)
        {
            Debug.Log("🔥 Compresión completada");
        }
        
        // FASE 2: Explosión/Expansión
        Vector3 escalaExpandida = new Vector3(escalaOriginal.x * 1.4f, escalaOriginal.y * 1.3f, escalaOriginal.z);
        tiempo = 0f;
        float duracionExpansion = 0.25f;
        
        while (tiempo < duracionExpansion)
        {
            transform.localScale = Vector3.Lerp(escalaComprimida, escalaExpandida, tiempo / duracionExpansion);
            // Añadir un poco de rotación para más dramatismo
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(tiempo * 20f) * 5f);
            tiempo += Time.deltaTime;
            yield return null;
        }
        
        if (mostrarDebug)
        {
            Debug.Log("💥 Expansión completada - preparando para destrucción");
        }
        
        // FASE 3: Oscilación antes de destruir (si no se destruye automáticamente)
        if (!destruirDespuesDeUsar)
        {
            tiempo = 0f;
            float duracionOscilacion = 0.2f;
            
            while (tiempo < duracionOscilacion)
            {
                float factor = Mathf.Sin(tiempo * 30f) * 0.1f;
                transform.localScale = escalaOriginal + Vector3.one * factor;
                tiempo += Time.deltaTime;
                yield return null;
            }
            
            // Volver a normal
            transform.localScale = escalaOriginal;
            transform.rotation = Quaternion.identity;
            yaUsado = false; // Permitir usar de nuevo
        }
        else
        {
            // Mantener la escala expandida hasta destruirse
            transform.localScale = escalaExpandida;
        }
    }
    
    // Dibujar gizmos para debug
    void OnDrawGizmosSelected()
    {
        // Dibujar área de rebote
        Gizmos.color = yaUsado ? Color.red : Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, new Vector3(1f, 0.2f, 1f));
    }
}