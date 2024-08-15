using Unity.Netcode;

public class CameraController : NetworkBehaviour
{
    private void Start()
    {
        if (!IsLocalPlayer)
            gameObject.SetActive(false);
            // Destroy(gameObject);
    }
}