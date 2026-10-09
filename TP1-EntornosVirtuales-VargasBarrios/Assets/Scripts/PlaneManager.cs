using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaneManager : MonoBehaviour
{
    [SerializeField] private ARPlaneManager aRPlaneManager;
    [SerializeField] private GameObject PuzzlePrefab;


    private List<ARPlane> planes = new List<ARPlane>();
    private void OnEnabled()
    {
        aRPlaneManager.planesChanged +- PlanesFound;
    }

    private void OnDisable()
    {
        aRPlaneManager.planesChanged -= PlanesFound;
    }

    private void PlanesFound(ARPlanesChangedEventArgs PlaneData)
    {
        if (PlaneData.added != null && PlaneData.added.Count > 0)
        {
            planes.AddRange(PlaneData.added);
        }
    
        foreach (var plane in planes)
        {
            if (plane.extents.x * plane.extents.y >= 0.4 && PuzzlePrefab == null)
            {
                PuzzlePrefabs = Instantiate(PuzzlePrefab);
                float yOffset = PuzzlePrefabs.transform.localScale.y / 2;
                PuzzlePrefabs.transform.position = new Vector3(plane.center.x , plane.center.y + yOffset , plane.center.z);
                PuzzlePrefabs.transform.forward = plane.normal;
                StopPlaneDetection();

            }
        }
    }

    public void StopPlaneDetection()
    {
        aRPlaneManager.requestedDetectionMode = PlaneDetectionMode.None;
        foreach (var plane in planes)
        {
            plane.gameObject.SetActive(false);
        }
    }




}
