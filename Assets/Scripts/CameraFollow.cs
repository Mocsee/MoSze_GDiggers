using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Follow")]
    [SerializeField] private float smoothSpeed = 3f;
    [SerializeField] private float yOffset = 2f;

    [Header("Zoom")]
    [SerializeField] private float normalSize = 50f; 
    [SerializeField] private float sprintSize = 60f;
    [SerializeField] private float zoomSpeed = 5f;

    private Camera cam;
    private float minY;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        minY = transform.position.y;

        if (cam != null)
            cam.orthographicSize = normalSize;
    }

    private void LateUpdate()
    {
        if (target == null || cam == null) return;

        FollowOnlyUp();
        HandleZoom();
    }

    private void FollowOnlyUp()
    {
        float targetY = target.position.y + yOffset;

        float newY = Mathf.Lerp(transform.position.y, targetY, smoothSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (transform.position.y < minY)
        {
            transform.position = new Vector3(transform.position.x, minY, transform.position.z);
        }
    }

    private void HandleZoom()
    {
        if (playerMovement == null) return;

        float targetSize = playerMovement.IsSprinting ? sprintSize : normalSize;
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, zoomSpeed * Time.deltaTime);
    }
}
