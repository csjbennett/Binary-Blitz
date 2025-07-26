using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    public Camera mainCamera;
    public Transform mainCameraTransform;
    public Transform player;
    private float defaultCameraSize;
    public float mouseBias = 1f;
    public float playerBias = 4f;
    public float cameraLerpSpeed = 5f;

    public enum FollowType { lerp, snappy };
    public FollowType followType;
    public enum CameraType { follow, fixedPos };
    public CameraType cameraType;

    // Start is called before the first frame update
    void Start()
    {
        defaultCameraSize = mainCamera.orthographicSize;

        mainCameraTransform.position = player.position + new Vector3(0, 0, -10);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (cameraType == CameraType.follow)
        {
            Vector2 mousePos = Input.mousePosition;
            Resolution resolution = Screen.currentResolution;

            mousePos.x = Mathf.Clamp(mousePos.x, 0, resolution.width);
            mousePos.y = Mathf.Clamp(mousePos.y, 0, resolution.height);

            Vector3 mousePosWorldspace = mainCamera.ScreenToWorldPoint(mousePos);
            mousePosWorldspace.z = -10;

            Vector3 playerPos = player.position;
            playerPos.z = -10;

            Vector3 targetPosition = ((playerPos * playerBias) + (mousePosWorldspace * mouseBias)) / (playerBias + mouseBias);

            if (followType == FollowType.lerp)
                mainCameraTransform.position = Vector3.Lerp(mainCameraTransform.position, targetPosition, Time.deltaTime * cameraLerpSpeed);
            else
                mainCameraTransform.position = targetPosition;
        }
    }

    // Camera type adjusters
    public void MakeFollow()
    {
        cameraType = CameraType.follow;
    }
    public void MakeFixed()
    {
        cameraType = CameraType.fixedPos;
    }

    // Camera focus adjuster
    public void SetCameraFocus(Transform target)
    {
        if (cameraType != CameraType.fixedPos)
            cameraType = CameraType.fixedPos;

        Vector3 targetPos = target.position + (Vector3.forward * -10);
        mainCameraTransform.DOMove(targetPos, 1f);
    }

    // Camera size adjusters
    public void SetCameraSize(float targetSize)
    {
        mainCamera.DOOrthoSize(targetSize, 1f);
    }
    public void ResetCameraSize()
    {
        mainCamera.DOOrthoSize(defaultCameraSize, 1f);
    }
}
