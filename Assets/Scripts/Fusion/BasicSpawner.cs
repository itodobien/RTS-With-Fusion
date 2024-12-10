using System;
using System.Collections.Generic;
using Fusion.Sockets;
using Unit_Activities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Fusion
{
    public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
    {
        [FormerlySerializedAs("_playerPrefab")] [SerializeField] private NetworkPrefabRef playerPrefab;
        [FormerlySerializedAs("_unitPrefab")] [SerializeField] private NetworkPrefabRef unitPrefab;
        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
        private Dictionary<PlayerRef, List<NetworkObject>> _spawnedUnits = new Dictionary<PlayerRef, List<NetworkObject>>();
    
        private NetworkRunner _runner;

        private void OnGUI()
        {
            if (_runner == null)
            {
                if (GUI.Button(new Rect(0,0,200,40), "Host"))
                {
                    StartGame(GameMode.Host);
                }
                if (GUI.Button(new Rect(0,40,200,40), "Join"))
                {
                    StartGame(GameMode.Client);
                }
            }
        }

        async void StartGame(GameMode mode)
        {
            // Create the Fusion _runner and let it know that we will be providing user input
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;

            // Create a NetworkSceneInfo from the current scene
            var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
            var sceneInfo = new NetworkSceneInfo();
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

            // Start or join (depends on gamemode) a session with a specific name
            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = "TestRoom",
                Scene = sceneInfo,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });

            Debug.Log($"Game started in {mode} mode");
        }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"Player joined: {player}");
            if (runner.IsServer)
            {
                Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 1, 0);
                NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
                _spawnedCharacters.Add(player, networkPlayerObject);
                _spawnedUnits.Add(player, new List<NetworkObject>());
                Debug.Log($"Player {player} added to _spawnedCharacters");
            }
        }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnedCharacters.Remove(player);
            }
        
            if (_spawnedUnits.TryGetValue(player, out List<NetworkObject> units))
            {
                foreach (var unit in units)
                {
                    runner.Despawn(unit);
                }
                _spawnedUnits.Remove(player);
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new NetworkInputData();

            if (Input.GetKey(KeyCode.W)) data.direction += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) data.direction += Vector3.back;
            if (Input.GetKey(KeyCode.A)) data.direction += Vector3.left;
            if (Input.GetKey(KeyCode.D)) data.direction += Vector3.right;
        
            data.mousePosition = MouseWorldPosition.GetMouseWorldPosition();
            data.spawnUnit = Input.GetKeyDown(KeyCode.U);

            input.Set(data);
        }
    
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

        public void OnSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void FixedUpdateNetwork()
        {
            Debug.Log("FixedUpdateNetwork called");
            if (_runner != null && _runner.IsServer)
            {
                Debug.Log("Runner is server");
                foreach (var kvp in _spawnedCharacters)
                {
                    var player = kvp.Key;
                    var playerObject = kvp.Value;

                    Debug.Log($"Checking input for player {player}");
                    if (_runner.TryGetInputForPlayer(player, out NetworkInputData input))
                    {
                        Debug.Log($"Got input for player {player}, spawnUnit: {input.spawnUnit}");
                        if (input.spawnUnit)
                        {
                            Debug.Log($"Attempting to spawn unit for player {player} at position {input.mousePosition}");
                            SpawnUnit(player, input.mousePosition);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("Runner is null or not server");
            }
        }


        private void SpawnUnit(PlayerRef player, Vector3 position)
        {
            Debug.Log($"Spawning unit for player {player} at position {position}");
            NetworkObject unitObject = _runner.Spawn(unitPrefab, position, Quaternion.identity, player);
            if (unitObject != null)
            {
                _spawnedUnits[player].Add(unitObject);
                Debug.Log($"Unit spawned successfully: {unitObject.name}");
            }
            else
            {
                Debug.LogError("Failed to spawn unit");
            }
        }
    }
}