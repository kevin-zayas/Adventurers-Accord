using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float moveSpeedScaler;
    [SerializeField] private float dragPanSpeed;
    [SerializeField] private bool useEdgeScrolling = false;
    [SerializeField] private bool useDragPanning = true;
    [SerializeField] private bool isPanning;

    private Vector2 lastMousePosition;

    private Vector3 moveDir;

    [SerializeField] private float orthographicSize;
    [SerializeField] private float minOrthographicSize;
    [SerializeField] private float maxOrthographicSize;
    [SerializeField] private float zoomSpeed;

    private void Awake()
    {
        orthographicSize = virtualCamera.m_Lens.OrthographicSize;
    }

    void Update()
    {
        HandleCameraMovement();
        HandleCameraZoom();

        if (useEdgeScrolling) HandleEdgeScrolling();
        if (useDragPanning) HandleDragPanning();

        transform.position += moveSpeed * Time.deltaTime * moveDir;

        moveDir = Vector3.zero;
        moveSpeedScaler = orthographicSize / maxOrthographicSize;
    }

    private void HandleCameraMovement()
    {
        if (Input.GetKey(KeyCode.W)) moveDir += Vector3.up;
        if (Input.GetKey(KeyCode.S)) moveDir += Vector3.down;
        if (Input.GetKey(KeyCode.A)) moveDir += Vector3.left;
        if (Input.GetKey(KeyCode.D)) moveDir += Vector3.right;
    }

    private void HandleEdgeScrolling()
    {
        if (useEdgeScrolling)
        {
            int edgeScrollSize = 20;

            if (Input.mousePosition.y <= edgeScrollSize) moveDir += Vector3.down;
            if (Input.mousePosition.x <= edgeScrollSize) moveDir += Vector3.left;

            if (Input.mousePosition.x >= Screen.width - edgeScrollSize) moveDir += Vector3.right;
            if (Input.mousePosition.y >= Screen.height - edgeScrollSize) moveDir += Vector3.up;
        }
    }

    private void HandleDragPanning()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isPanning = true;
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(1))
        {
            isPanning = false;
        }

        if (isPanning)
        {
            Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - lastMousePosition;

            moveDir.x = -mouseMovementDelta.x * dragPanSpeed;
            moveDir.y = -mouseMovementDelta.y * dragPanSpeed;

            lastMousePosition = Input.mousePosition;
        }
    }

    private void HandleCameraZoom()
    {
        if (Input.mouseScrollDelta.y > 0) orthographicSize -= 50;
        if (Input.mouseScrollDelta.y < 0) orthographicSize += 50;

        orthographicSize = Mathf.Clamp(orthographicSize, minOrthographicSize, maxOrthographicSize);

        virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, orthographicSize, Time.deltaTime * zoomSpeed);
    }
}
