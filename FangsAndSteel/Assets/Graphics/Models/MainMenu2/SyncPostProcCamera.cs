using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncPostProcCamera : MonoBehaviour
{
    private Camera camera;
    public Camera cameraToSync;

    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        camera.fieldOfView = cameraToSync.fieldOfView;
        camera.transform.position = cameraToSync.transform.position;
        camera.transform.rotation = cameraToSync.transform.rotation;
    }
}
