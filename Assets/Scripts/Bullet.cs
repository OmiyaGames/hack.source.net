using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

[RequireComponent(typeof(CapsuleCollider))]
public class Bullet : NetworkBehaviour
{
    [SerializeField]
    Vector3 moveVelocity = new Vector3(0, 1, 0);
    [SerializeField]
    Transform spawnPosition;
    [SerializeField]
    GameObject spark;
    [SerializeField]
    GameObject explosion;

    [SyncVar(hook = "OnIgnorePlayerSynced")]
    string ignorePlayer = null;

    static readonly Dictionary<Collider, PlayerStatus> allPlayers = new Dictionary<Collider, PlayerStatus>();
    static readonly Dictionary<Collider, Bullet> allBullets = new Dictionary<Collider, Bullet>();

    Collider lastCollider = null;
    CharacterController lastCharacter = null;
    Rigidbody body = null;
    Vector3 localVelocity;

    public static bool TryGetBullet(Collider collider, out Bullet returnBullet)
    {
        return allBullets.TryGetValue(collider, out returnBullet);
    }

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
        allBullets.Add(GetComponent<Collider>(), this);
        localVelocity = body.rotation * moveVelocity;
    }

    void FixedUpdate()
    {
        if(isServer == true)
        {
            body.velocity = localVelocity;
        }
    }

    void OnDestroy()
    {
        allBullets.Remove(GetComponent<Collider>());
    }

    void OnIgnorePlayerSynced(string newPlayer)
    {
        // Update bullet layer
        if(string.IsNullOrEmpty(newPlayer) == true)
        {
            gameObject.layer = GameSetup.neutralBulletLayerInt;
        }
        else if(newPlayer == PlayerSetup.LocalInstance.name)
        {
            gameObject.layer = GameSetup.playerBulletLayerInt;
        }
        else
        {
            gameObject.layer = GameSetup.oppositionBulletLayerInt;
        }
    }

    #region Commands
    [Command]
    void CmdSetIgnoredPlayer(string newPlayer)
    {
        ignorePlayer = newPlayer;
    }

    [Command]
    void CmdSpawnSpark()
    {
        GameObject clone = (GameObject)Instantiate(spark.gameObject, spawnPosition.position, spawnPosition.rotation);
        NetworkServer.Spawn(clone);
    }

    [Command]
    void CmdSpawnExplosion()
    {
        GameObject clone = (GameObject)Instantiate(explosion.gameObject, spawnPosition.position, spawnPosition.rotation);
        NetworkServer.Spawn(clone);
    }
    #endregion

    void OnCollisionEnter(Collision info)
    {
        if((isServer == true) && (IgnoredPlayer != null) && (info.collider != lastCollider) && (info.collider.name != IgnoredPlayer) )
        {
            if (info.collider.CompareTag("ReflectBullet") == true)
            {
                ReflectDirection(info);
            }
            else if (info.collider.CompareTag("FlipBullet") == true)
            {
                FlipDirection(info.collider.name);
            }
            // Check if this is the player
            else if (info.collider.CompareTag("Player") == true)
            {
                // Hit the player
                PlayerStatus status = null;
                if (allPlayers.TryGetValue(info.collider, out status) == false)
                {
                    status = info.collider.GetComponent<PlayerStatus>();
                    allPlayers.Add(info.collider, status);
                }

                // Check status
                if (status.IsReflectEnabled == true)
                {
                    // Flip direction if player is reflecting
                    FlipDirection(status.name);
                }
                else
                {
                    // Decrease health if player is alive
                    if (status.CurrentState == PlayerStatus.State.Alive)
                    {
                        // Decrease player health
                        status.Health -= 1;
                    }

                    // Explode
                    Explode();
                }
            }
            else
            {
                // Explode
                Explode();
            }

            lastCollider = info.collider;
        }
    }

    public void PlayerHit(CharacterController controller, PlayerStatus status)
    {
        if ((isServer == true) && (IgnoredPlayer != null) && (controller != lastCharacter) && (controller.name != IgnoredPlayer))
        {
            // Check status
            if (status.IsReflectEnabled == true)
            {
                // Flip direction if player is reflecting
                FlipDirection(controller.name);
                Spark(false);
            }
            else if (status.CurrentState == PlayerStatus.State.Alive)
            {
                // Decrease player health
                status.Health -= 1;

                // Explode
                Explode();
            }
            else
            {
                Spark(true);
            }

            lastCharacter = controller;
        }
    }

    void FlipDirection(string playerId)
    {
        // Do a full 180
        Vector3 angles = transform.eulerAngles;
        angles.x += 180f;
        body.rotation = Quaternion.Euler(angles);

        // Recalculate the local velocity
        localVelocity = body.rotation * moveVelocity;

        // Ignore this player
        IgnoredPlayer = playerId;
    }

    void ReflectDirection(Collision info)
    {
        // FIXME: do something about this!
        //info.contacts[0].normal
        // FIXME: rotate, and recalculate the local velocity
    }

    void Explode()
    {
        CmdSpawnExplosion();
        Destroy(gameObject);
        NetworkServer.Destroy(gameObject);
    }

    void Spark(bool destroy)
    {
        CmdSpawnSpark();
        if (destroy == true)
        {
            Destroy(gameObject);
            NetworkServer.Destroy(gameObject);
        }
    }
}
