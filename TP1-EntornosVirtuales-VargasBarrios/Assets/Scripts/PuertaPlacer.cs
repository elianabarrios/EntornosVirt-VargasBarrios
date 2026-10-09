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
    [SerializeField] float alturaCamaraSobrePiso = 1.4f;   // cuánto más abajo que el celular está el piso
    [SerializeField] float tamanoMinimoPlano = 0.4f;       // ignora planos más chicos (metros)
    [SerializeField] bool permitirSinPared = true;         // plan B: colocar delante de la cámara
    [SerializeField] float distanciaPlanB = 2.5f;

    static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    GameObject reticula;
    Pose poseValida;
    bool hayPared;
    bool colocada;

    void Start()
    {
        reticula = Instantiate(reticulaPrefab);
        reticula.SetActive(false);
        panelSimon.SetActive(false);
    }

    void Update()
    {
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

        reticula.SetActive(hayPared);
        if (hayPared)
            reticula.transform.SetPositionAndRotation(poseValida.position, poseValida.rotation);

        botonColocar.interactable = hayPared || permitirSinPared;

        if (textoEstado != null)
        {
            int verticales = 0;
            foreach (var p in planeManager.trackables)
                if (p.alignment == PlaneAlignment.Vertical) verticales++;

            textoEstado.text =
                $"Estado AR: {ARSession.state}\n" +
                $"Planos: {planeManager.trackables.count} (verticales: {verticales})\n" +
                $"Pared: {hayPared}";
        }
    }

    // Lo llama el botón
    public void Colocar()
    {
        if (colocada) return;
        var cam = Camera.main;

        Vector3 pos;
        Quaternion rot;

        if (hayPared)
        {
            pos = new Vector3(poseValida.position.x,
                              cam.transform.position.y - alturaCamaraSobrePiso,
                              poseValida.position.z);
            rot = poseValida.rotation;
        }
        else if (permitirSinPared)
        {
            // Plan B: 2,5 m delante de la cámara, mirando hacia el jugador
            Vector3 adelante = cam.transform.forward;
            adelante.y = 0f;
            adelante.Normalize();
            pos = cam.transform.position + adelante * distanciaPlanB;
            pos.y = cam.transform.position.y - alturaCamaraSobrePiso;
            rot = Quaternion.LookRotation(-adelante, Vector3.up);
        }
        else return;

        Instantiate(puertaPrefab, pos, rot);
        colocada = true;

        Destroy(reticula);
        planeManager.enabled = false;
        foreach (var p in planeManager.trackables) p.gameObject.SetActive(false);
        panelColocar.SetActive(false);
        panelSimon.SetActive(true);
    }
}