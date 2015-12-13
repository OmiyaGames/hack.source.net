using UnityEngine;
using UnityEngine.Networking;

public class PlayerRotationSync : NetworkBehaviour
{
    [SerializeField]
    Transform playerTransform;
    [SerializeField]
    Transform cameraTransform;
    [SerializeField]
    float lerpSpeed = 10f;
    [SerializeField]
    float threshold = 1f;

    [SyncVar(hook = "OnPlayerRotationSynced")]
    float playerAngle = 0f;
    [SyncVar(hook = "OnCameraRotationSynced")]
    float cameraAngle = 0f;
    
    float lastPlayerAngle = 0f;
    Vector3 playerRotationEuler = Vector3.zero;
    float lastCameraAngle = 0f;
    Vector3 cameraRotationEuler = Vector3.zero;
    
    [Client]
    void OnPlayerRotationSynced(float latestPlayerRotation)
    {
        playerAngle = latestPlayerRotation;
    }
    
    [Client]
    void OnCameraRotationSynced(float latestCameraRotation)
    {
        cameraAngle = latestCameraRotation;
    }
    
    [Client]
    void TransmitRotation()
    {
        if((Mathf.Abs(playerTransform.localEulerAngles.y - lastPlayerAngle) > threshold) || (Mathf.Abs(cameraTransform.localEulerAngles.x - lastCameraAngle) > threshold))
        {
            lastPlayerAngle = playerTransform.localEulerAngles.y;
            lastCameraAngle = cameraTransform.localEulerAngles.x;
            CmdUpdateRotation(lastPlayerAngle, lastCameraAngle);
        }
    }
    
    [Command]
    void CmdUpdateRotation(float playerRotation, float cameraRotation)
    {
        playerAngle = playerRotation;
        cameraAngle = cameraRotation;
    }
    
    // Update is called once per frame
    void Update ()
    {
        if(isLocalPlayer == true)
        {
            TransmitRotation();
        }
        else
        {
            playerRotationEuler.y = Mathf.Lerp(playerRotationEuler.y, playerAngle, (Time.deltaTime * lerpSpeed));
            cameraRotationEuler.x = Mathf.Lerp(cameraRotationEuler.x, cameraAngle, (Time.deltaTime * lerpSpeed));
            playerTransform.localRotation = Quaternion.Euler(playerRotationEuler);
            cameraTransform.localRotation = Quaternion.Euler(cameraRotationEuler);
        }
    }
}
