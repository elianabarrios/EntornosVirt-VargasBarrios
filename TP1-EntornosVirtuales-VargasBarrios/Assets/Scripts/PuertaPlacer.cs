using UnityEngine;
using Vuforia;

public class MarcadorPuerta : MonoBehaviour
{
    [Header("Qué se instancia")]
    [SerializeField] GameObject puertaPrefab;        // el prefab ObjectsScene (de Assets/Prefabs)
    [SerializeField] GameObject panelSimon;

    [Header("Ubicación respecto del marcador")]
    [SerializeField] float distanciaDetras = 1.5f;           // metros hacia adentro de la pared
    [SerializeField] float alturaMarcadorSobrePiso = 1.4f;   // altura del centro del marcador sobre el piso
    [SerializeField] float segundosEstable = 1f;             // tiempo con el marcador bien detectado
    [SerializeField, Range(0f, 1f)] float toleranciaVertical = 0.3f;  // 0 = exactamente vertical

    ObserverBehaviour observer;
    float tiempoTracked;
    bool colocado;
    string aviso = "";

    void Awake()
    {
        observer = GetComponent<ObserverBehaviour>();
        panelSimon.SetActive(false);
    }

    void Update()
    {
        if (colocado) return;

        bool tracked = observer.TargetStatus.Status == Status.TRACKED;
        bool vertical = Mathf.Abs(transform.up.y) <= toleranciaVertical;

        aviso = (tracked && !vertical) ? "Pegá el marcador en una pared (vertical)" : "";

        if (tracked && vertical) tiempoTracked += Time.deltaTime;
        else tiempoTracked = 0f;

        if (tiempoTracked >= segundosEstable) Colocar();
    }

    void Colocar()
    {
        colocado = true;

        // Normal de la pared, horizontal, apuntando hacia el jugador
        Vector3 normal = transform.up;
        normal.y = 0f;
        normal.Normalize();

        Vector3 pos = transform.position - normal * distanciaDetras;   // hacia adentro de la pared
        pos.y = transform.position.y - alturaMarcadorSobrePiso;        // apoyada en el piso
        Quaternion rot = Quaternion.LookRotation(normal, Vector3.up);  // +Z hacia el jugador

        Instantiate(puertaPrefab, pos, rot);
        panelSimon.SetActive(true);
    }

    void OnGUI()
    {
        if (aviso == "") return;
        GUI.skin.label.fontSize = 44;
        GUI.Label(new Rect(30, 80, Screen.width - 60, 200), aviso);
    }
}