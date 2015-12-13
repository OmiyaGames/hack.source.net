using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CapsuleCollider))]
public class Bullet : NetworkBehaviour
{
    [SerializeField]
    Vector3 moveVelocity = new Vector3(0, 1, 0);

    [SyncVar]
    string ignorePlayer = null;

    Collider lastCollider = null;
    Rigidbody body = null;
    Vector3 localVelocity;

    public string IgnoredPlayer
    {
        get
        {
            return ignorePlayer;
        }
        set
        {
            if((value != null) && (ignorePlayer != value))
            {
                CmdSetIgnoredPlayer(value);
            }
        }
    }

    void Start()
    {
        body = GetComponent<Rigidbody>();
        localVelocity = body.rotation * moveVelocity;
    }

    void FixedUpdate()
    {
        body.velocity = localVelocity;
    }

    public void FlipDirection(string playerId = "")
    {
        // FIXME: do something about this!
    }

    [Command]
    void CmdSetIgnoredPlayer(string newPlayer)
    {
        ignorePlayer = newPlayer;
    }

    void OnCollisionEnter(Collision info)
    {
        if((IgnoredPlayer != null) && (info.collider != lastCollider) && (info.collider.name != IgnoredPlayer))
        {
            //Debug.Log("Collide Into: " + info.collider.name);
            // FIXME: for now, just destroy yourself
            Destroy(gameObject);
            lastCollider = info.collider;
        }
    }
}
