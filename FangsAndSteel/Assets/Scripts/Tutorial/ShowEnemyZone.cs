using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShowEnemyZone : MonoBehaviour
{
    public static ShowEnemyZone TutorialUiInstance { get; private set; }

    private Transform cameraTransform;
    [SerializeField] private Vector3 CameraZonePosition;

    [SerializeField] private float maxCamSpeedToZone;
    private float curCamSpeedToZone;
    private const float MIN_CAM_SPEED_TO_ZONE = 25f;

    public Quaternion targetRotation;

    public GameObject PointerRing1;
    public GameObject PointerRing2;
    public GameObject PointerRing3;
    public Button ShowZoneButton;
    public GameObject ShowZoneHint;
    public GameObject WinPanel;

    private void Awake()
    {
        TutorialUiInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = Camera.main.transform;
        StartCoroutine(ShowZone(2f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator ShowZone(float latency)
    {
        yield return new WaitForSeconds(latency);
        yield return StartCoroutine(ShowZone());
    }

    public void ShowZoneFunc()
    {
        StartCoroutine(ShowZone());
    }

    public IEnumerator ShowZone()
    {
        PointerRing1.SetActive(true);
        PointerRing2.SetActive(true);
        PointerRing3.SetActive(true);
        ShowZoneHint.SetActive(true);
        ShowZoneButton.interactable = false;

        Quaternion initialRotation = cameraTransform.rotation;
        Vector3 initialLocation = cameraTransform.position;
        yield return StartCoroutine(MoveCameraToPoint(CameraZonePosition, targetRotation));
        yield return new WaitForSeconds(2.5f);
        yield return StartCoroutine(MoveCameraToPoint(initialLocation, initialRotation));

        PointerRing1.SetActive(false);
        PointerRing2.SetActive(false);
        PointerRing3.SetActive(false);
        ShowZoneHint.SetActive(false);
        ShowZoneButton.interactable = true;

        yield return null;
    }

    public IEnumerator MoveCameraToPoint(Vector3 destination, Quaternion trgRot)
    {
        float initialDist = (cameraTransform.position - destination).sqrMagnitude;
        float curDist = initialDist;
        while (curDist > initialDist * 0.3)
        {
            curCamSpeedToZone = Mathf.Lerp(MIN_CAM_SPEED_TO_ZONE, maxCamSpeedToZone, (initialDist * 0.5f) / curDist);
            Vector3 dir = (destination - cameraTransform.position).normalized;
            cameraTransform.position = cameraTransform.position + dir * curCamSpeedToZone * Time.deltaTime;
            curDist = (cameraTransform.position - destination).sqrMagnitude;
            yield return new WaitForEndOfFrame();
        }
        initialDist = curDist;
        while (curDist > 5f)
        {
            curCamSpeedToZone = Mathf.Lerp(maxCamSpeedToZone, MIN_CAM_SPEED_TO_ZONE, (initialDist - curDist) / initialDist);
            Vector3 dir = (destination - cameraTransform.position).normalized;
            cameraTransform.position = cameraTransform.position + dir * curCamSpeedToZone * Time.deltaTime;
            curDist = (cameraTransform.position - destination).sqrMagnitude;
            cameraTransform.rotation = Quaternion.RotateTowards(cameraTransform.rotation, trgRot, 50 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }
}
