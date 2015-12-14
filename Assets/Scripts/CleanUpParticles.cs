using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class CleanUpParticles : NetworkBehaviour
{
    [SerializeField]
    float lifeTime = 1f;
    [SerializeField]
    OmiyaGames.SoundEffect playOnAwake;
    // FIXME: testing here
    [SerializeField]
    bool isCleanedUp = false;

    [SyncVar]
    double spawnTime = -1;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        CmdSetTime();
        Debug.Log("Particle created");
    }

    void Start()
    {
        if(playOnAwake != null)
        {
            playOnAwake.Play();
        }
    }

    [Client]
    void Update()
    {
        if ((isCleanedUp == false) && (isLocalPlayer == true) && (spawnTime > 0) && (Network.time > (spawnTime + lifeTime)))
        {
            Destroy(gameObject);
            NetworkServer.Destroy(gameObject);
            isCleanedUp = true;
        }
    }

    [Command]
    void CmdDestroy()
    {
        Destroy(gameObject);
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    void CmdSetTime()
    {
        spawnTime = Network.time;
    }
}
