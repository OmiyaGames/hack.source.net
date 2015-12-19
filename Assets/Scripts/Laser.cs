using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Laser : MonoBehaviour
{
    [SerializeField]
    LayerMask mask;
    [SerializeField]
    [Range(1f, 500f)]
    float maxDistance = 100f;
    [SerializeField]
    Transform spot;

    LineRenderer renderer;
    Ray laserRay;
    RaycastHit info;
    Vector3 defaultPosition;
    Vector3 raycastPosition;

    void Start()
    {
        renderer = GetComponent<LineRenderer>();
        defaultPosition = new Vector3(0, 0, maxDistance);
    }

    void Update ()
    {
        // Raycast something
        laserRay.origin = transform.position;
        laserRay.direction = transform.forward;
        if(Physics.Raycast(laserRay, out info, maxDistance, mask) == true)
        {
            // Convert hit position to local position
            raycastPosition = transform.worldToLocalMatrix * info.point;
            raycastPosition.x = 0;
            raycastPosition.y = 0;
            renderer.SetPosition(1, raycastPosition);
            spot.localPosition = raycastPosition;
        }
        else
        {
            renderer.SetPosition(1, defaultPosition);
            spot.localPosition = defaultPosition;
        }
    }
}
