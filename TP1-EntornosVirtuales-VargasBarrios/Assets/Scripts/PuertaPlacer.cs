using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PuertaPlacer : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] ARRaycastManager raycastManager;
    [SerializeField] ARPlaneManager planeManager;

    [Header("Prefabs")]
    [SerializeField] GameObject reticulaPrefab;
    [SerializeField] GameObject puertaPrefab;

    [Header("UI")]
    [SerializeField] Button botonColocar;
    [SerializeField] GameObject panelColocar;
    [SerializeField] GameObject panelSimon;
    [SerializeField] TMP_Text textoEstado;

    [Header("Ajustes")]
    [SerializeField] float alturaCamaraSobrePiso = 1.4f;
    [SerializeField] float tamanoMinimoPlano = 0.4f;
    [SerializeField] bool permitirSinPared = true;
    [SerializeField] float distanciaPlanB = 2.5f;

    [Header("Diagnóstico")]
    [SerializeField] bool ocultarHabitacion = true;   // desactiva el hijo "Room" al colocar (para probar)

    static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    GameObject reticula;
    GameObject puertaInstancia;
    Pose poseValida;
    bool hayPared;
    bool colocada;
    string modoColocacion = "-";

    ARSessionState ultimoEstado;
    int perdidasTracking;

    void Start()
    {
        reticula = Instantiate(reticulaPrefab);
        reticula.SetActive(false);
        panelSimon.SetActive(false);
    }

    void Update()
    {
        // Contar cuántas veces se pierde el seguimiento
        if (ultimoEstado == ARSessionState.SessionTracking &&
            ARSession.state != ARSessionState.SessionTracking)
            perdidasTracking++;
        ultimoEstado = ARSession.state;

        ActualizarTexto();
        if (colocada) return;

        hayPared = false;
        var cam = Camera.main;
        var centro = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(centro, hits, TrackableType.PlaneWithinBounds))
        {
            foreach (var h in hits)
            {
                var plano = planeManager.GetPlane(h.trackableId);
                if (plano == null || plano.alignment != PlaneAlignment.Vertical) continue;
                if (plano.size.x < tamanoMinimoPlano || plano.size.y < tamanoMinimoPlano) continue;

                Vector3 normal = plano.normal;
                if (Vector3.Dot(normal, cam.transform.position - h.pose.position) < 0f)
                    normal = -normal;
                normal.y = 0f;
                if (normal.sqrMagnitude < 0.01f) continue;
                normal.Normalize();

                poseValida = new Pose(h.pose.position, Quaternion.LookRotation(normal, Vector3.up));
                hayPared = true;
                break;
            }
        }

        if (reticula != null)
        {
            reticula.SetActive(hayPared);
            if (hayPared)
                reticula.transform.SetPositionAndRotation(poseValida.position, poseValida.rotation);
        }

        // El botón solo se habilita con seguimiento estable
        bool trackingOk = ARSession.state == ARSessionState.SessionTracking;
        botonColocar.interactable = trackingOk && (hayPared || permitirSinPared);
    }

    void ActualizarTexto()
    {
        if (textoEstado == null) return;
        var cam = Camera.main.transform;

        int verticales = 0;
        foreach (var p in planeManager.trackables)
            if (p.alignment == PlaneAlignment.Vertical) verticales++;

        string puerta = "sin colocar";
        if (puertaInstancia != null)
        {
            Vector3 rel = cam.InverseTransformPoint(puertaInstancia.transform.position);
            puerta = $"x {rel.x:F1}  y {rel.y:F1}  z {rel.z:F1}  (dist {rel.magnitude:F1} m)";
        }

        textoEstado.text =
            $"Estado: {ARSession.state}\n" +
            $"Perdidas de tracking: {perdidasTracking}\n" +
            $"Planos: {planeManager.trackables.count} (vert: {verticales})\n" +
            $"Pared: {hayPared}  Modo: {modoColocacion}\n" +
            $"Cam: {cam.position.x:F1}, {cam.position.y:F1}, {cam.position.z:F1}\n" +
            $"Puerta (rel. cámara): {puerta}";
    }

    // Lo llama el botón "Colocar"
    public void Colocar()
    {
        if (colocada) return;
        var cam = Camera.main;

        Vector3 pos;
        Quaternion rot;

        if (hayPared)
        {
            modoColocacion = "PARED";
            pos = new Vector3(poseValida.position.x,
                              cam.transform.position.y - alturaCamaraSobrePiso,
                              poseValida.position.z);
            rot = poseValida.rotation;
        }
        else if (permitirSinPared)
        {
            modoColocacion = "PLAN B";
            Vector3 adelante = cam.transform.forward;
            adelante.y = 0f;
            adelante.Normalize();
            pos = cam.transform.position + adelante * distanciaPlanB;
            pos.y = cam.transform.position.y - alturaCamaraSobrePiso;
            rot = Quaternion.LookRotation(-adelante, Vector3.up);
        }
        else return;

        puertaInstancia = Instantiate(puertaPrefab, pos, rot);
        colocada = true;

        if (ocultarHabitacion)
        {
            var room = puertaInstancia.transform.Find("ObjectsScene");
            if (room != null) room.gameObject.SetActive(false);
        }

        Destroy(reticula);
        reticula = null;
        planeManager.enabled = false;
        foreach (var p in planeManager.trackables) p.gameObject.SetActive(false);
        panelColocar.SetActive(false);
        panelSimon.SetActive(true);
    }

    public void AbrirPuerta()
    {
        if (puertaInstancia == null)
        {
            Debug.LogWarning("Se tiene que instanciar la puerta con AR", this);
            return;
        }

        var puertaPuzzle =
            puertaInstancia.GetComponentInChildren<PuertaPuzzle>(true);

        if (puertaPuzzle == null)
        {
            Debug.LogError(
                "nO HAY PUERTAPUZZLE", this);
            return;
        }

        puertaPuzzle.Abrir();
    }

    // volver a colocar sin reabrir la app
    public void Reiniciar()
    {
        if (puertaInstancia != null) Destroy(puertaInstancia);
        if (reticula == null)
        {
            reticula = Instantiate(reticulaPrefab);
            reticula.SetActive(false);
        }
        colocada = false;
        modoColocacion = "-";
        planeManager.enabled = true;
        foreach (var p in planeManager.trackables) p.gameObject.SetActive(true);
        panelColocar.SetActive(true);
        panelSimon.SetActive(false);
    }
}