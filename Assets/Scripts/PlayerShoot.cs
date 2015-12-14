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
    PlayerSetup setup;
    float nextFire = 0f;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        status = GetComponent<PlayerStatus>();
        setup = GetComponent<PlayerSetup>();
        pauseMenu = Singleton.Get<MenuManager>().GetMenu<PauseMenu>();
    }

    void Update ()
    {
        // Check if this is a local instance firing
        if((isLocalPlayer == true) && (Input.GetButton("Fire1") == true) && (Time.time > nextFire) &&
            (status.IsReflectEnabled == false) && (pauseMenu.CurrentState == IMenu.State.Hidden))
        {
            // Check if the player can fire
            switch (status.CurrentState)
            {
                case PlayerStatus.State.Alive:
                case PlayerStatus.State.Invincible:
                    CmdSpawn();
                    setup.hudAnimations.SetTrigger(PlayerSetup.ShootTrigger);
                    nextFire = Time.time + fireRate;
                    break;
            }
        }
    }

    [Command]
    void CmdSpawn()
    {
        GameObject clone = (GameObject)Instantiate(bulletPrefab.gameObject, spawnPosition.position, spawnPosition.rotation);
        //NetworkServer.SpawnWithClientAuthority(clone, connectionToClient);
        NetworkServer.Spawn(clone);
        clone.GetComponent<Bullet>().IgnoredPlayer = name;
    }
}
