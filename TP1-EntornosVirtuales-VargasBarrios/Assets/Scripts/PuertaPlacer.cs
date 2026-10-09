using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PuertaPlacer : MonoBehaviour
{
    [SerializeField] ARRaycastManager raycastManager;
    [SerializeField] ARPlaneManager planeManager;
    [SerializeField] GameObject reticulaPrefab;
    [SerializeField] GameObject puertaPrefab;     // PuertaSimon
    [SerializeField] Button botonColocar;
    [SerializeField] GameObject panelColocar;
    [SerializeField] GameObject panelSimon;

    static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
    static readonly List<ARRaycastHit> hitsPiso = new List<ARRaycastHit>();

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

        if (raycastManager.Raycast(centro, hits, TrackableType.PlaneWithinPolygon))
        {
            foreach (var h in hits)
            {
                var plano = planeManager.GetPlane(h.trackableId);
                if (plano == null || plano.alignment != PlaneAlignment.Vertical) continue;

                // Normal de la pared, siempre apuntando hacia el jugador y sin inclinación
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

        botonColocar.interactable = hayPared;
    }

    // Lo llama el botón "Colocar"
    public void Colocar()
    {
        if (!hayPared || colocada) return;

        Vector3 pos = poseValida.position;
        Vector3 normal = poseValida.rotation * Vector3.forward;

        // Altura del piso: rayo hacia abajo desde un poco delante de la pared
        float alturaPiso = Camera.main.transform.position.y - 1.4f; // plan B
        var rayo = new Ray(pos + normal * 0.5f, Vector3.down);
        if (raycastManager.Raycast(rayo, hitsPiso, TrackableType.PlaneWithinPolygon))
        {
            foreach (var h in hitsPiso)
            {
                var p = planeManager.GetPlane(h.trackableId);
                if (p != null && p.alignment == PlaneAlignment.HorizontalUp)
                {
                    alturaPiso = h.pose.position.y;
                    break;
                }
            }
        }

        Instantiate(puertaPrefab, new Vector3(pos.x, alturaPiso, pos.z), poseValida.rotation);
        colocada = true;

        // Limpiar: ocultar retícula y planos, cambiar de UI
        Destroy(reticula);
        planeManager.enabled = false;
        foreach (var p in planeManager.trackables) p.gameObject.SetActive(false);
        panelColocar.SetActive(false);
        panelSimon.SetActive(true);
    }
}
