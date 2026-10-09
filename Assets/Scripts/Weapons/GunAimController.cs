using UnityEngine;

public class GunAimController : MonoBehaviour
{
    [SerializeField] private Camera aimCamera;
    [SerializeField] private float angleOffset;
    [SerializeField] private bool instantRotation = true;
    [SerializeField, Min(0f)] private float rotationSpeed = 720f;

    void Update()
    {
        if (aimCamera == null)
        {
            aimCamera = Camera.main;
            if (aimCamera == null)
            {
                return;
            }
        }

        Ray mouseRay = aimCamera.ScreenPointToRay(Input.mousePosition);
        Plane aimPlane = new Plane(Vector3.forward, new Vector3(0f, 0f, transform.position.z));
        if (!aimPlane.Raycast(mouseRay, out float distance))
        {
            return;
        }

        Vector3 mouseWorldPosition = mouseRay.GetPoint(distance);
        Vector2 aimDirection = mouseWorldPosition - transform.position;
        if (aimDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float targetAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg + angleOffset;
        float currentAngle = transform.eulerAngles.z;
        float angle = instantRotation
            ? targetAngle
            : Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}