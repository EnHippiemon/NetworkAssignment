using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
    [SerializeField] private TMP_Text textBubble;

    private NetworkVariable<float> messageTimer = new NetworkVariable<float>();
    private NetworkVariable<bool> isTicking = new NetworkVariable<bool>();
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && IsLocalPlayer)
            SendMessageRPC("you suck", OwnerClientId);
        
        else if(Input.GetKeyDown(KeyCode.Alpha2) && IsLocalPlayer)
            SendMessageRPC("give me kebab", OwnerClientId);        
        
        else if(Input.GetKeyDown(KeyCode.Alpha3) && IsLocalPlayer)
            SendMessageRPC("gg ez", OwnerClientId);    
        
        else if(Input.GetKeyDown(KeyCode.Alpha4) && IsLocalPlayer)
            SendMessageRPC("ur handsome", OwnerClientId);

        if (IsServer && isTicking.Value && messageTimer.Value < 2f)
        {
            messageTimer.Value += Time.deltaTime;
            
            if (messageTimer.Value >= 2f)
            {
                isTicking.Value = false;
                SendMessageRPC("", OwnerClientId);
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void SendMessageRPC(FixedString128Bytes message, ulong clientID)
    {
        if (message != "")
            isTicking.Value = true;
        
        messageTimer.Value = 0;
        DistributeMessageRPC(message, clientID);
    }

    [Rpc(SendTo.Everyone)]
    private void DistributeMessageRPC(FixedString128Bytes message, ulong clientID)
    {
        if (clientID == OwnerClientId)
            textBubble.text = message.ToString();
    }
}
