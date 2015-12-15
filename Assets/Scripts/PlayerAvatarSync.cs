using UnityEngine;
using UnityEngine.Networking;

public class PlayerAvatarSync : NetworkBehaviour
{
    public const string HitTrigger = "hit";
    public const string AliveBool = "alive";
    public const string VelocityFloat = "velocity";
    public const string RunSpeedFloat = "runSpeed";

    [SerializeField]
    Animator avatarAnimations;
    [SerializeField]
    float runSpeedMultiplier = 2f;
    [SerializeField]
    UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController controller;
    [SerializeField]
    float floatThreshold = 0.1f;

    [SyncVar(hook = "VelocityChanged")]
    float velocity = 0f;
    [SyncVar(hook = "AliveChanged")]
    bool alive = true;
    [SyncVar(hook = "HitChanged")]
    bool hitToggle = false;
    [SyncVar(hook = "IsRunningChanged")]
    bool isRunning = false;

    PlayerStatus status;
    float lastVelocity = 0f, currentVelocity = 0f;
    int lastHealth = PlayerStatus.MaxHealth;
    bool lastRunning = false, currentRunning = false;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        status = GetComponent<PlayerStatus>();
    }

    void Update()
    {
        if (isLocalPlayer == true)
        {
            UpdateVelocity();
            UpdateAliveHit();
            UpdateRunning();
        }
    }

    #region Helper Methods
    private void UpdateRunning()
    {
        currentRunning = controller.Running;
        if (lastRunning != currentRunning)
        {
            CmdSetIsRunning(currentRunning);
            lastRunning = currentRunning;
        }
    }

    private void UpdateAliveHit()
    {
        // Check if health changed
        if (status.Health != lastHealth)
        {
            if (status.Health <= 0)
            {
                // Check if dead
                CmdSetAlive(false);
            }
            else
            {
                // Check if health decreased
                if (status.Health < lastHealth)
                {
                    CmdToggleHit();
                }

                // Indicate we're alive
                if (alive == false)
                {
                    CmdSetAlive(true);
                }
            }
            // Update health
            lastHealth = status.Health;
        }
    }

    private void UpdateVelocity()
    {
        // Check if the velocity has changed
        currentVelocity = controller.Velocity.magnitude;
        if (Mathf.Abs(currentVelocity - lastVelocity) > floatThreshold)
        {
            CmdSetVelocity(currentVelocity);
            lastVelocity = currentVelocity;
        }
    }
    #endregion

    #region Commands
    [Command]
    void CmdSetVelocity(float setVelocity)
    {
        velocity = setVelocity;
    }
    [Command]
    void CmdSetIsRunning(bool setRunning)
    {
        isRunning = setRunning;
    }
    [Command]
    void CmdSetAlive(bool setAlive)
    {
        alive = setAlive;
    }
    [Command]
    void CmdToggleHit()
    {
        hitToggle = !hitToggle;
    }
    #endregion

    #region Hooks
    void VelocityChanged(float newVelocity)
    {
        avatarAnimations.SetFloat(VelocityFloat, newVelocity);
    }

    void AliveChanged(bool newAlive)
    {
        avatarAnimations.SetBool(AliveBool, newAlive);
    }

    void HitChanged(bool newHit)
    {
        avatarAnimations.SetTrigger(HitTrigger);
    }

    void IsRunningChanged(bool newIsRunning)
    {
        if (currentRunning == true)
        {
            avatarAnimations.SetFloat(RunSpeedFloat, runSpeedMultiplier);
        }
        else
        {
            avatarAnimations.SetFloat(RunSpeedFloat, 1f);
        }
    }
    #endregion
}
