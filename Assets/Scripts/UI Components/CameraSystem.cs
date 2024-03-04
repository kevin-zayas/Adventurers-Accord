using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float orthographicRatio;
    [SerializeField] private float dragPanSpeed;
    [SerializeField] private bool useEdgeScrolling = false;
    [SerializeField] private bool useDragPanning = true;
    [SerializeField] private bool isPanning;

    [SerializeField] private float orthographicSize;
    [SerializeField] private float minOrthographicSize;
    [SerializeField] private float maxOrthographicSize;
    [SerializeField] private float zoomSpeed;

    private Vector2 lastMousePosition;
    private Vector3 moveDir;
    private Vector2 cameraColliderSize;

    [SerializeField] private BoxCollider2D cameraCollider;
    [SerializeField] private Rigidbody2D rb;

    private void Awake()
    {
        cameraColliderSize = cameraCollider.size;
    }
    private void Update()
    {
        HandleCameraZoom();

        if (useEdgeScrolling) HandleEdgeScrolling();
        if (useDragPanning) HandleDragPanning();
        
    }
    void FixedUpdate()
    {
        HandleCameraMovement();

        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * (Vector2)moveDir);
        
        moveDir = Vector3.zero;
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

            moveDir.x = -mouseMovementDelta.x * dragPanSpeed * orthographicRatio;
            moveDir.y = -mouseMovementDelta.y * dragPanSpeed * orthographicRatio;

            lastMousePosition = Input.mousePosition;
        }
    }

    private void HandleCameraZoom()
    {
        if (Input.mouseScrollDelta.y > 0) orthographicSize -= 50;
        if (Input.mouseScrollDelta.y < 0) orthographicSize += 50;

        orthographicSize = Mathf.Clamp(orthographicSize, minOrthographicSize, maxOrthographicSize);
        virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, orthographicSize, Time.deltaTime * zoomSpeed);

        orthographicRatio = orthographicSize / maxOrthographicSize;
        cameraCollider.size = cameraColliderSize * orthographicRatio;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        //if the camera hitbox is past the collision bounds, move it back to the center
        if (cameraCollider.bounds.min.x < collision.collider.bounds.min.x)
        {
            transform.position = new Vector3(transform.position.x + (collision.collider.bounds.min.x - cameraCollider.bounds.min.x), transform.position.y, transform.position.z);
        }
        if (cameraCollider.bounds.max.x > collision.collider.bounds.max.x)
        {
            transform.position = new Vector3(transform.position.x - (cameraCollider.bounds.max.x - collision.collider.bounds.max.x), transform.position.y, transform.position.z);
        }
        if (cameraCollider.bounds.min.y < collision.collider.bounds.min.y)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + (collision.collider.bounds.min.y - cameraCollider.bounds.min.y), transform.position.z);
        }
        if (cameraCollider.bounds.max.y > collision.collider.bounds.max.y)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - (cameraCollider.bounds.max.y - collision.collider.bounds.max.y), transform.position.z);
        }
    }
    
}
