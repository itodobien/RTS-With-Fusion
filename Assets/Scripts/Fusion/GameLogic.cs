/*
using Fusion;
using UnityEngine;

public class GameLogic : NetworkBehaviour, IPlayerJoined, IPlayerLeft // this class is just for the KCC demo. not really used in the game.
{
    
    
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [Networked, Capacity(4)] private NetworkDictionary<PlayerRef, Player> Players => default;
    
    public void PlayerJoined(PlayerRef player)
    {
        if (HasStateAuthority)
        {
            NetworkObject playerObject = Runner.Spawn(playerPrefab, Vector3.up, Quaternion.identity, player);
            Players.Add(player, playerObject.GetComponent<Player>());
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        throw new System.NotImplementedException();
    }
}
*/
