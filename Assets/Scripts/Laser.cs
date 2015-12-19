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
    [SerializeField]
    [Range(0f, 1f)]
    float bufferDistance = 0.2f;

    Animator animator;
    LineRenderer renderer;
    Ray laserRay;
    RaycastHit info;
    Vector3 defaultPosition;

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
            PositionEndPoint(transform.InverseTransformPoint(info.point));

            // Indicate we hit the player if tagged as such
            animator.SetBool(SpottedBool, info.collider.CompareTag("Player"));
        }
        else
        {
            // Set end to default position
            PositionEndPoint(defaultPosition);

            // Indicate nothing was hit
            animator.SetBool(SpottedBool, false);
        }
    }

    void PositionEndPoint(Vector3 position)
    {
        // Position the end of the laser
        position.x = 0;
        position.y = 0;
        renderer.SetPosition(1, position);

        // Position the spot
        position.z -= bufferDistance;
        spot.localPosition = position;
    }
}
