using System;
using System.Collections.Generic;
using Fusion.Sockets;
using Grid;
using UI;
using Units;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fusion
{
    public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkPrefabRef playerPrefab;
        
        [SerializeField] private NetworkPrefabRef[] unitPrefabs;
        
        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters;
    
        private NetworkRunner _runner;
        private NetworkObject _unitActionSystem;
        
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

        private void Update()
        {
            _mouseButton1 = Input.GetMouseButton(1);
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
            
            UnitSelectionManager.Instance.SetActivePlayer(_runner.LocalPlayer);
        }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 0, 1);
                NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
                 int unitPrefabIndex = player.RawEncoded % unitPrefabs.Length;
                 
                 Player playerScript = networkPlayerObject.GetComponent<Player>();
                 playerScript.SetUnitPrefabIndex(unitPrefabIndex);
                 
                 playerScript.SetUnitPrefabs(unitPrefabs);
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

            if (Input.GetMouseButton(1))
            {
                data.buttons.Set(NetworkInputData.MOUSEBUTTON1, true);
                
                var mouseWorldPosition = MouseWorldPosition.GetMouseWorldPosition();
                var clickedGridPosition = LevelGrid.Instance.GetGridPosition(mouseWorldPosition);
                
                data.targetGridX = clickedGridPosition.x;
                data.targetGridZ = clickedGridPosition.z;
                
                data.targetPosition = MouseWorldPosition.GetMouseWorldPosition();
                _mouseButton1 = false; // Reset button after it's recorded
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
            if (Input.GetKey(KeyCode.R))
            {
                data.buttons.Set(NetworkInputData.SPIN, true);
            }
            
            var selectionChange = UnitSelectionManager.Instance.GetNextSelectionChange();
            if (selectionChange.HasValue)
            {
                data.buttons.Set(NetworkInputData.SELECT_UNIT, true);
                data.selectedUnitId = selectionChange.Value.unitId;
                data.isSelected = selectionChange.Value.isSelected;                
            }

            data.buttons.Set(NetworkInputData.JUMP, Input.GetKey(KeyCode.Space));
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