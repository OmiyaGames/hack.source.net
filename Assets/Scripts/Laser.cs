using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Animator))]
public class Laser : MonoBehaviour
{
    public const string SpottedBool = "Spotted";

    [SerializeField]
    LayerMask mask;
    [SerializeField]
    [Range(1f, 500f)]
    float maxDistance = 100f;
    [SerializeField]
    Transform spot;

    Animator animator;
    LineRenderer renderer;
    Ray laserRay;
    RaycastHit info;
    Vector3 defaultPosition;
    Vector3 raycastPosition;

    void Start()
    {
        animator = GetComponent<Animator>();
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
            raycastPosition = transform.InverseTransformPoint(info.point);
            PositionEndPoint(ref raycastPosition);

            // Indicate we hit the player if tagged as such
            animator.SetBool(SpottedBool, info.collider.CompareTag("Player"));
        }
        else
        {
            // Set end to default position
            PositionEndPoint(ref defaultPosition);

            // Indicate nothing was hit
            animator.SetBool(SpottedBool, false);
        }
    }

    void PositionEndPoint(ref Vector3 position)
    {
        raycastPosition.x = 0;
        raycastPosition.y = 0;
        renderer.SetPosition(1, raycastPosition);
        spot.localPosition = raycastPosition;
    }
}
