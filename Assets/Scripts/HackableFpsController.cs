using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(PlayerSetup))]
public class HackableFpsController : RigidbodyFirstPersonController
{
    PlayerSetup player;

    protected override void Start()
    {
        base.Start();
        player = GetComponent<PlayerSetup>();
    }

    protected override Vector2 GetMovementInput()
    {
        Vector2 input = new Vector2
        {
            x = Horizontal,
            y = Vertical
        };
        movementSettings.UpdateDesiredTargetSpeed(input);
        return input;
    }

    protected override bool GetJumpInput()
    {
        if((player.CurrentActiveControls & PlayerSetup.ActiveControls.Jump) != 0)
        {
            return base.GetJumpInput();
        }
        else
        {
            return false;
        }
    }

    protected override bool GetRunInput()
    {
        if ((player.CurrentActiveControls & PlayerSetup.ActiveControls.Run) != 0)
        {
            return CrossPlatformInputManager.GetButtonDown("Run");
        }
        else
        {
            return false;
        }
    }

    float Horizontal
    {
        get
        {
            float returnValue = CrossPlatformInputManager.GetAxis("Horizontal");
            if ((returnValue < 0) && ((player.CurrentActiveControls & PlayerSetup.ActiveControls.Left) == 0))
            {
                returnValue = 0;
            }
            if ((returnValue > 0) && ((player.CurrentActiveControls & PlayerSetup.ActiveControls.Right) == 0))
            {
                returnValue = 0;
            }
            return returnValue;
        }
    }

    float Vertical
    {
        get
        {
            float returnValue = CrossPlatformInputManager.GetAxis("Vertical");
            if ((returnValue < 0) && ((player.CurrentActiveControls & PlayerSetup.ActiveControls.Back) == 0))
            {
                returnValue = 0;
            }
            if ((returnValue > 0) && ((player.CurrentActiveControls & PlayerSetup.ActiveControls.Forward) == 0))
            {
                returnValue = 0;
            }
            return returnValue;
        }
    }
}
