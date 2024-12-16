using UnityEngine;
using UnityEngine.Video;

public class PlayerCamera : MonoBehaviour
{
    #region Varaibles

    [Header("Camera Settings")]
    [SerializeField] private Camera cam;
    [SerializeField] private float height = 4f;
    [SerializeField] private float distance = 2.5f;
    [SerializeField] private float lookAtHeight = 2f;
    [SerializeField] private float angle = 0f;
    [SerializeField] private float smoothSpeed = 0.5f;
    [SerializeField] private float mouseSensitivity = 0.1f;

    private Vector3 refVelocity;
    [SerializeField] private Transform target;

    #endregion Variables

    #region Properties

    public Camera Cam => cam;
    public float Height
    {
        get => height;
        set => height = value;
    } 
    public float Distance
    {
        get => distance;
        set => distance = value;
    }
    public float LookAtHeight => lookAtHeight;
    public float Angle
    {
        get => angle;
        set => angle = value;
    }
    public float SmoothSpeed => smoothSpeed;
    public float MouseSensitivity => mouseSensitivity;
    public Transform Target => target;

    [SerializeField] private LayerMask playerLayerMask;

    #endregion Properties

    #region Reset Camera Position

    public void ResetCameraPosition()
    {
        distance = 2.5f;
        height = 5;
        lookAtHeight = 2;
        angle = 0;
        smoothSpeed = 0.5f;

        Vector3 worldPosition = Vector3.forward * -distance + Vector3.up * height;
        Vector3 rotatedVector = Quaternion.AngleAxis(angle, Vector3.up) * worldPosition;
        Vector3 targetPosition = target.position + rotatedVector;

        cam.transform.position = targetPosition;
        cam.transform.LookAt(target.position + Vector3.up * lookAtHeight);
    }

    #endregion Reset Camera Position

    private void LateUpdate()
{
    HandleCamera();

    RaycastHit hitInfo;

    // Raycast 방향 수정: target.position - cam.transform.position
    if (Physics.Raycast(cam.transform.position, target.position - cam.transform.position, out hitInfo, Vector3.Distance(cam.transform.position, target.position), playerLayerMask))
    {
        // 레이어 마스크 비교
        if (((1 << hitInfo.collider.gameObject.layer) & playerLayerMask) != 0)
        {
            Debug.Log("Player is in the way");
            height = 9.8f;
            distance = 7.5f;
        }
        else
        {
            Debug.Log("Player is not in the way");
            height -= 0.01f;
            distance -= 0.01f;
        }
    }
}

    public void HandleCamera()
    {
        Vector3 worldPosition = Vector3.forward * -distance + Vector3.up * height;
        Vector3 rotatedVector = Quaternion.AngleAxis(angle, Vector3.up) * worldPosition;
        Vector3 targetPosition = target.position + rotatedVector;
       
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, targetPosition, ref refVelocity, smoothSpeed);
        transform.LookAt(target.position + Vector3.up * lookAtHeight);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawCube(target.position + Vector3.up * lookAtHeight, Vector3.one * 0.25f);
        Gizmos.color = new Color(1, 0, 0, 0.75f);
        Gizmos.DrawWireCube(target.position + Vector3.up * lookAtHeight, Vector3.one * 0.25f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(cam.transform.position, target.position + Vector3.up * lookAtHeight);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(cam.transform.position, target.position);
    }
}
