using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CapsuleCollider))]
public class Bullet : NetworkBehaviour
{
    [SyncVar]
    string ignorePlayer = string.Empty;

    Collider lastCollider = null;

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
                SetIgnoredPlayer(value);
            }
        }
    }

    [Command]
    void SetIgnoredPlayer(string newPlayer)
    {
        ignorePlayer = newPlayer;
    }

    void OnCollisionEnter(Collision info)
    {
        if(info.collider != lastCollider)
        {
            // FIXME: for now, just destroy yourself
            Destroy(gameObject);
            lastCollider = info.collider;
        }
    }
}
