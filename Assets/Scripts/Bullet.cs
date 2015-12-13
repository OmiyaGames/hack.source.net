using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

[RequireComponent(typeof(CapsuleCollider))]
public class Bullet : NetworkBehaviour
{
    [SerializeField]
    Vector3 moveVelocity = new Vector3(0, 1, 0);

    [SyncVar]
    string ignorePlayer = null;

    static readonly Dictionary<Collider, PlayerStatus> allPlayers = new Dictionary<Collider, PlayerStatus>();

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
        if(isServer == true)
        {
            body.velocity = localVelocity;
        }
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
        if((isServer == true) && (IgnoredPlayer != null) && (info.collider != lastCollider) && (info.collider.name != IgnoredPlayer) )
        {
            // Check if this is the player
            if(info.collider.CompareTag("Player") == true)
            {
                // Hit the player
                PlayerStatus temp = null;
                if(allPlayers.TryGetValue(info.collider, out temp) == false)
                {
                    temp = info.collider.GetComponent<PlayerStatus>();
                    allPlayers.Add(info.collider, temp);
                }
                temp.Health -= 1;
            }

            // FIXME: Needs more explosion
            Destroy(gameObject);
            //Network.Destroy(networkView);
            lastCollider = info.collider;
        }
    }
}
