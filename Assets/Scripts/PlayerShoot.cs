using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(PlayerStatus))]
public class PlayerShoot : NetworkBehaviour
{
    [SerializeField]
    Transform spawnPosition;
    [SerializeField]
    Bullet bulletPrefab;

    PlayerStatus status;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        status = GetComponent<PlayerStatus>();
    }

    void Update ()
    {
        if((isLocalPlayer == true) && (Input.GetButton("Fire1") == true))
        {
            switch(status.CurrentState)
            {
                case PlayerStatus.State.Alive:
                case PlayerStatus.State.Invincible:
                    CmdSpawn();
                    break;
            }
        }
    }

    [Command]
    void CmdSpawn()
    {
        GameObject clone = (GameObject)Instantiate(bulletPrefab.gameObject, spawnPosition.position, spawnPosition.rotation);
        clone.GetComponent<Bullet>().IgnoredPlayer = name;
        NetworkServer.Spawn(clone);
    }
}
