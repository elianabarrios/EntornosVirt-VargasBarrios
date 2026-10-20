using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Agregar al Button. El evento Al Completar reemplaza al On Click para saltar.
[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public class SaltarVideo : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField, Min(0.1f)] float segundos = 3f;
    [SerializeField] TMP_Text texto;
    [Tooltip("Opcional: Image de tipo Filled, independiente del fondo del botón.")]
    [SerializeField] Image progreso;
    [Tooltip("Conectar al método que termina la intro y continúa el juego.")]
    [SerializeField] UnityEvent alCompletar = new UnityEvent();

    Button boton;
    bool presionado;
    bool completado;
    int dedo;
    float transcurrido;
    int ultimoSegundo = -1;

    void Awake()
    {
        boton = GetComponent<Button>();
        if (texto == null) texto = GetComponentInChildren<TMP_Text>(true);
        // El indicador no debe interceptar el toque del botón.
        if (progreso != null) progreso.raycastTarget = false;
    }

    void OnEnable()
    {
        completado = false;
        Cancelar();
    }

    public void OnPointerDown(PointerEventData evento)
    {
        if (evento.button != PointerEventData.InputButton.Left || presionado
            || completado || !isActiveAndEnabled || !boton.IsInteractable()) return;
        presionado = true;
        dedo = evento.pointerId;
        transcurrido = 0f;
        ActualizarVisual();
    }

    public void OnPointerUp(PointerEventData evento)
    {
        if (presionado && evento.pointerId == dedo) Cancelar();
    }

    public void OnPointerExit(PointerEventData evento)
    {
        if (presionado && evento.pointerId == dedo) Cancelar();
    }

    void Update()
    {
        if (!presionado || completado) return;
        if (!boton.IsActive() || !boton.IsInteractable())
        {
            Cancelar();
            return;
        }

        // Sigue contando aunque el video se muestre con Time.timeScale en cero.
        transcurrido += Time.unscaledDeltaTime;
        ActualizarVisual();
        if (transcurrido < Mathf.Max(0.1f, segundos)) return;

        // Se marca antes de invocar: la continuación puede ocultar este objeto.
        completado = true;
        presionado = false;
        if (texto != null) texto.text = "Omitiendo…";
        alCompletar.Invoke();
    }

    void ActualizarVisual()
    {
        float duracion = Mathf.Max(0.1f, segundos);
        if (progreso != null) progreso.fillAmount = Mathf.Clamp01(transcurrido / duracion);
        int restante = Mathf.CeilToInt(Mathf.Max(0f, duracion - transcurrido));
        if (texto != null && restante != ultimoSegundo)
            texto.text = "Mantené presionado… " + restante;
        ultimoSegundo = restante;
    }

    void Cancelar()
    {
        presionado = false;
        transcurrido = 0f;
        ultimoSegundo = -1;
        if (progreso != null) progreso.fillAmount = 0f;
        if (texto != null && !completado)
            texto.text = "Mantené presionado " + Mathf.Max(0.1f, segundos).ToString("0.#")
                + " segundos para omitir";
    }

    void OnDisable() { Cancelar(); }
    void OnApplicationFocus(bool tieneFoco) { if (!tieneFoco) Cancelar(); }
    void OnApplicationPause(bool enPausa) { if (enPausa) Cancelar(); }
}
