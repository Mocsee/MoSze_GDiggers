using UnityEngine;

public class CameraSprintZoom : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private float normalSize = 5f;
    [SerializeField] private float sprintSize = 6.5f;
    [SerializeField] private float zoomSpeed = 3f;

    private void Awake()
    {
        if (cam == null)
            cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (cam == null || playerMovement == null) return;

        float targetSize = playerMovement.IsSprinting ? sprintSize : normalSize;
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, zoomSpeed * Time.deltaTime);
    }
}
