using System;
using System.Collections.Generic;
using Fusion.Sockets;
using Unit_Activities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fusion
{
    public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkPrefabRef playerPrefab;
        [SerializeField] private NetworkPrefabRef unitActionSystemPrefab;

        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters;
    
        private NetworkRunner _runner;
        
        private bool _mouseButton0;
        private bool _mouseButton1;
        
        private void Awake()
        {
            _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
        }
        
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
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
            _runner.AddCallbacks(this);
    
            var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
            var sceneInfo = new NetworkSceneInfo();
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
    
            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = "TestRoom",
                Scene = scene,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
            Debug.Log($"Local PlayerRef after game start: {_runner.LocalPlayer}");
            
            UnitSelectionManager.Instance.SetActivePlayer(_runner.LocalPlayer);
        }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 1, 0);
                NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
                _spawnedCharacters.Add(player, networkPlayerObject);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnedCharacters.Remove(player);
            }
        }
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new NetworkInputData();

            if (Input.GetMouseButton(0))
                data.buttons.Set(NetworkInputData.MOUSEBUTTON0, true);
            if (Input.GetMouseButton(1))
            {
                data.buttons.Set(NetworkInputData.MOUSEBUTTON1, true);
                data.targetPosition = MouseWorldPosition.GetMouseWorldPosition();
            }

            if (Input.GetKey(KeyCode.W)) data.direction += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) data.direction += Vector3.back;
            if (Input.GetKey(KeyCode.A)) data.direction += Vector3.left;
            if (Input.GetKey(KeyCode.D)) data.direction += Vector3.right;

            if (Input.GetKey(KeyCode.U))
            {
                data.buttons.Set(NetworkInputData.SPAWNUNIT, true);
                data.spawnPosition = MouseWorldPosition.GetMouseWorldPosition();
            }

            Debug.Log($"Input collected: Buttons = {data.buttons}, TargetPosition = {data.targetPosition}");

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
        
    }
}