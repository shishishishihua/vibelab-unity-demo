using UnityEngine;

public sealed class VibeLabOrbitCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 7.5f;
    [SerializeField] private float minDistance = 4.5f;
    [SerializeField] private float maxDistance = 10.5f;
    [SerializeField] private float yaw = 12f;
    [SerializeField] private float pitch = 18f;
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private float dragSpeed = 0.25f;
    [SerializeField] private float zoomSpeed = 2.5f;
    [SerializeField] private float lookHeight = 1.2f;

    public void Configure(Transform orbitTarget)
    {
        target = orbitTarget;
        RefreshImmediate();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        HandleKeyboard();
        HandleMouse();
        HandleZoom();
        RefreshImmediate();
    }

    private void HandleKeyboard()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        yaw += horizontal * rotateSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            pitch -= rotateSpeed * 0.35f * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            pitch += rotateSpeed * 0.35f * Time.deltaTime;
        }

        pitch = Mathf.Clamp(pitch, 8f, 55f);
    }

    private void HandleMouse()
    {
        if (!Input.GetMouseButton(1))
        {
            return;
        }

        yaw += Input.GetAxis("Mouse X") * rotateSpeed * dragSpeed;
        pitch -= Input.GetAxis("Mouse Y") * rotateSpeed * dragSpeed;
        pitch = Mathf.Clamp(pitch, 8f, 55f);
    }

    private void HandleZoom()
    {
        var scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) < 0.001f)
        {
            return;
        }

        distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
    }

    private void RefreshImmediate()
    {
        var focus = target.position + Vector3.up * lookHeight;
        var rotation = Quaternion.Euler(pitch, yaw, 0f);
        var offset = rotation * (Vector3.back * distance);

        transform.position = focus + offset;
        transform.rotation = rotation;
    }
}
