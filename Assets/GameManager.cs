using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    private NetworkVariable<int> p1Score = new NetworkVariable<int>(0);
    private NetworkVariable<int> p2Score = new NetworkVariable<int>(0);
    
    public NetworkVariable<int> totalPlayers = new NetworkVariable<int>();
    public NetworkVariable<bool> isPlaying = new NetworkVariable<bool>();
    
    private TMP_Text scoreBoard;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        scoreBoard = GetComponent<TMP_Text>();
    }
    
    public override void OnNetworkSpawn()
    {
        isPlaying.Value = true;
        SendScoreRPC(0, 0);
    }
    [Rpc(SendTo.Server)]
    public void PlayerJoinedRPC()
    {
        totalPlayers.Value++;

        // Above is code for bigger parties, but let's focus on two players 
        if (totalPlayers.Value > 2)
            totalPlayers.Value = 1;
    }

    [Rpc(SendTo.Server)]
    public void UpdateScoreRPC(int deathCount, int playerNumber)
    {
        switch (playerNumber)
        {
            case 1:
                p2Score.Value = deathCount;
                break;
            case 2:
                p1Score.Value = deathCount;
                break;
            default: 
                Debug.Log("more than two players??");
                break;
        }

        StartCoroutine(WinPause());
        SendScoreRPC(p1Score.Value, p2Score.Value);
    }

    [Rpc(SendTo.Everyone)]
    private void SendScoreRPC(int score1, int score2)
    {
        scoreBoard.text = "P1: " + score1 + " - P2: " + score2;
    }
    
    private IEnumerator WinPause()
    {
        isPlaying.Value = false;
        yield return new WaitForSeconds(3f);
        isPlaying.Value = true;
    }
}

