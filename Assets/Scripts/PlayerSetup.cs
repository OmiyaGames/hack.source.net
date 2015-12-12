using UnityEngine;
using UnityEngine.Networking;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField]
    UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController controller;
    [SerializeField]
    UnityStandardAssets.Characters.FirstPerson.HeadBob headBob;
    [SerializeField]
    Camera view;
    [SerializeField]
    AudioListener listener;

    // Use this for initialization
    void Start ()
    {
        controller.enabled = isLocalPlayer;
        headBob.enabled = isLocalPlayer;
        view.enabled = isLocalPlayer;
        listener.enabled = isLocalPlayer;
    }
}
