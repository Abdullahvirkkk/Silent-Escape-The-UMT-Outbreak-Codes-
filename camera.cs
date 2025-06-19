using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public float cameraHeight = 1.5f;
    public float smoothSpeed = 10f;
    public LayerMask collisionMask;
    public float cameraRadius = 0.3f;
    private Vector3 desiredPosition;

    void LateUpdate()
    {
        Vector3 targetPosition = player.position + player.forward * cameraRadius + Vector3.up * cameraHeight;
        desiredPosition = targetPosition;

        RaycastHit hit;
        if (Physics.SphereCast(player.position + Vector3.up * cameraHeight, cameraRadius,
            (desiredPosition - (player.position + Vector3.up * cameraHeight)).normalized, out hit, cameraRadius, collisionMask))
            {
                desiredPosition = hit.point + hit.normal * cameraRadius;
            }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(player.position + Vector3.up * cameraHeight);
    }
}
