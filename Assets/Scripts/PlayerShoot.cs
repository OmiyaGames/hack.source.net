using UnityEngine;
using UnityEngine.Networking;
using OmiyaGames;

[RequireComponent(typeof(PlayerStatus))]
public class PlayerShoot : NetworkBehaviour
{
    [SerializeField]
    Transform spawnPosition;
    [SerializeField]
    Bullet bulletPrefab;
    [SerializeField]
    float fireRate = 0.3f;

    PauseMenu pauseMenu;
    PlayerStatus status;
    float nextFire = 0f;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        status = GetComponent<PlayerStatus>();
        pauseMenu = Singleton.Get<MenuManager>().GetMenu<PauseMenu>();
    }

    void Update ()
    {
        // Check if this is a local instance firing
        if((isLocalPlayer == true) && (Input.GetButton("Fire1") == true) && (pauseMenu.CurrentState == IMenu.State.Hidden) && (Time.time > nextFire))
        {
            // Check if the player can fire
            switch (status.CurrentState)
            {
                case PlayerStatus.State.Alive:
                case PlayerStatus.State.Invincible:
                    CmdSpawn();
                    nextFire = Time.time + fireRate;
                    break;
            }
        }
    }

    [Command]
    void CmdSpawn()
    {
        GameObject clone = (GameObject)Instantiate(bulletPrefab.gameObject, spawnPosition.position, spawnPosition.rotation);
        NetworkServer.SpawnWithClientAuthority(clone, connectionToClient);
        clone.GetComponent<Bullet>().IgnoredPlayer = name;
    }
}
