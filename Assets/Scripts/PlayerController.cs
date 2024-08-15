using TMPro;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : NetworkBehaviour
{
    private NetworkVariable<Vector2> moveInput = new NetworkVariable<Vector2>();
    private NetworkVariable<int> health = new NetworkVariable<int>();
    private NetworkVariable<int> deathCount = new NetworkVariable<int>();

    private NetworkVariable<Vector3> projectileDirection = new NetworkVariable<Vector3>();
    private NetworkVariable<float> projectileRotation = new NetworkVariable<float>();
    
    private Vector3 mousePos;
    private Vector3 shootDirection;
    private float shootRotation;
    
    private TMP_Text healthBar;
    private int playerNumber;
    
    [SerializeField] private GameObject projectile;
    [SerializeField] private float projectileSpeed = 1f;

    private Camera localCamera;
    private float movementX;
    private float movementY;
    [SerializeField] private float movementSpeed = 1f;

    private Vector2 movementVector;

    private void Start()
    {
        if (IsLocalPlayer)
            localCamera = GetComponentInChildren<Camera>();
        
        if (IsServer)
        {
            GameManager.instance.PlayerJoinedRPC();
            playerNumber = GameManager.instance.totalPlayers.Value;
            
            health.Value = 5;
        }
        
        healthBar = GetComponentInChildren<TMP_Text>();
        CheckHealthRPC();
        
        // if (IsServer)
        // {
        
        // Atm this is not decided by the server. How to fix that? If IsServer,
        // the colors only change on the host. 
        
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            switch (OwnerClientId)
            {
                case 0:
                    sprite.color = Color.red;
                    break;
                case 1:
                    sprite.color = Color.blue;
                    break;
                case 2:
                    sprite.color = Color.yellow;
                    break;
                case 3:
                    sprite.color = Color.black;
                    break;
                default:
                    sprite.color = Color.green;
                    break;
            }
        // }
    }

    void Update()
    {
        if (IsLocalPlayer)
        {
            PlayerMovement(); 
            
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                mousePos = localCamera.ScreenToWorldPoint(Input.mousePosition);
                shootDirection = mousePos - transform.position;
                
                Vector3 rotation = transform.position - mousePos;
                shootRotation = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
                
                ShootProjectileRPC(shootDirection, shootRotation);
            }
        }
        
        if (IsServer)
        {
            transform.position += (Vector3)moveInput.Value;
        }
    }

    private void PlayerMovement()
    {
        movementX = Input.GetAxisRaw("Horizontal");
        movementY = Input.GetAxisRaw("Vertical");

        movementVector = new Vector2(movementX, movementY);
        MoveRPC(movementVector);
    }

    [Rpc(SendTo.Server)]
    private void MoveRPC(Vector2 movementData)
    {
        if (GameManager.instance.isPlaying.Value == false)
        {
            moveInput.Value = new Vector2(0f, 0f);
            return;
        }
        
        movementData.Normalize();
        moveInput.Value = movementData * Time.deltaTime * movementSpeed;
    }

    [Rpc(SendTo.Server)]
    private void ShootProjectileRPC(Vector3 aimDirection, float aimRotation)
    {
        if (GameManager.instance.isPlaying.Value == false)
            return;

        projectileDirection.Value = aimDirection;
        projectileRotation.Value = aimRotation;
        
        GameObject projectileClone = Instantiate(
            projectile, 
            transform.position + new Vector3(projectileDirection.Value.x, projectileDirection.Value.y).normalized, 
            Quaternion.Euler(0, 0, projectileRotation.Value + 90));
        
        projectileClone.GetComponent<NetworkObject>().Spawn();
        
        Rigidbody2D projectileCloneRigidBody = projectileClone.GetComponent<Rigidbody2D>();
        projectileCloneRigidBody.velocity = new Vector2(projectileDirection.Value.x, projectileDirection.Value.y).normalized * projectileSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Projectile") && IsServer)
        {
            TakeDamageRPC();
        }
    }

    [Rpc(SendTo.Server)]
    private void TakeDamageRPC()
    {
        health.Value--;
        CheckHealthRPC();
    }
    
    [Rpc(SendTo.Server)]
    private void CheckHealthRPC()
    {
        if (health.Value <= 0)
        {
            health.Value = 5;
            deathCount.Value++;
            GameManager.instance.UpdateScoreRPC(deathCount.Value, playerNumber);
        }

        UpdateHealthBarRPC(health.Value.ToString());
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateHealthBarRPC(string currentHealth)
    {
        if (!healthBar)
            return;
        
        healthBar.text = currentHealth;
    }
}