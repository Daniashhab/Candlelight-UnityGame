using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TowerCamera : MonoBehaviour
{
    public Canvas itemCanvas;
    public float cameraDelay = 0.5f;
    public CinemachineVirtualCamera towerCamera;
    private bool cameraSwitch = false;

    void Start()
    {
        
    }

    void Update()
    {
       
    }
    IEnumerator SwitchCameraView() 
    {
        yield return new WaitForSeconds(cameraDelay);
        towerCamera.Priority = 20;
        yield return new WaitForSeconds(3f);
        towerCamera.Priority = 1;

    }
    public void OnCanvasClosed()
    {
        itemCanvas.gameObject.SetActive(false);

        if (!cameraSwitch)
        {
            cameraSwitch = true;
            StartCoroutine(SwitchCameraView());
        }
    }
}
