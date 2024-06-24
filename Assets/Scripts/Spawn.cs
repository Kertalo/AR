using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;

public class Spawn : MonoBehaviour
{
    [SerializeField] GameObject levelPrefab;
    [SerializeField] Interpreter interpreter;

    List<ARRaycastHit> hits = new List<ARRaycastHit>();
    ARRaycastManager raycastManager;
    ARPlaneManager planeManager;
    [SerializeField] Camera cameraAR;
    GameObject level;

    void Start()
    {
        level = null;
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
    }

    public void DeleteLevel()
    {
        Destroy(level);
        planeManager.enabled = true;
    }

    void Update()
    {
        if (Input.touchCount == 0 || level != null || Input.GetTouch(0).phase != TouchPhase.Began)
            return;

        Vector2 position = Input.GetTouch(0).position;

        if (raycastManager.Raycast(position, hits) &&
            Physics.Raycast(cameraAR.ScreenPointToRay(position), out var hit))
        {
            planeManager.enabled = false;
            interpreter.DisableSurfaces();
            level = Instantiate(levelPrefab, hits[0].pose.position, Quaternion.identity);
        }   
    }
}
