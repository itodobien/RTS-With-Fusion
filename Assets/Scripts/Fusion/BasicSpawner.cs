using System;
using System.Collections;
using System.Collections.Generic;
using Actions;
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
        public static BasicSpawner Instance { get; private set; }
        
        [SerializeField] public UnitDatabase unitDatabase;
        [SerializeField] private NetworkPrefabRef enemyPositionManagerPrefab;

        
        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters;
    
        private NetworkRunner _runner;
        private NetworkObject _unitActionSystem;
        private int _nextTeamId;

        private void Awake()
        {
            _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
            if (Instance != null && Instance != this)
            {
                Debug.LogError("Multiple BasicSpawner instances detected. Destroying the new one.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void OnGUI()
        {
            if (_runner == null)
            {
                if (GUI.Button(new Rect(50,50,200,50), "Host"))
                {
                    StartGame(GameMode.Host);
                }
                if (GUI.Button(new Rect(50,100,200,50), "Join"))
                {
                    StartGame(GameMode.Client);
                }
            }
        }

        private void Update()
        {
            Input.GetMouseButton(0);
            Input.GetMouseButton(1);
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

            if (_runner.IsServer)
            {
                _runner.Spawn(enemyPositionManagerPrefab, Vector3.zero, Quaternion.identity);
            }
            UnitSelectionManager.Instance.SetActivePlayer(_runner.LocalPlayer);
        }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                Vector3 spawnPosition = new Vector3(player.RawEncoded % runner.Config.Simulation.PlayerCount * 3, 0, 1);
                NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
                runner.SetPlayerObject(player, networkPlayerObject);

                Player playerScript = networkPlayerObject.GetComponent<Player>();
        
                int assignedTeamId = _nextTeamId;
                playerScript.SetTeamID(assignedTeamId);
                _nextTeamId++;
        
                int unitIndex = player.RawEncoded % unitDatabase.unitDataList.Length;
                playerScript.SetUnitPrefabIndex(unitIndex);
        
                _spawnedCharacters.Add(player, networkPlayerObject);

                if (player == runner.LocalPlayer)
                {
                    if (UnitActionSystem.Instance != null && UnitActionSystem.Instance.Object.InputAuthority == PlayerRef.None)
                    {
                        UnitActionSystem.Instance.Object.AssignInputAuthority(player);
                    }
                }
            }
            if (player == runner.LocalPlayer)
            {
                SetUpLocalPlayer(runner, player);
            }
        }

        private void SetUpLocalPlayer(NetworkRunner runner, PlayerRef playerRef)
        {
            if (!runner.TryGetPlayerObject(playerRef, out var localPlayerObj))
            {
                StartCoroutine(WaitForLocalPlayer(runner, playerRef));
                return;
            }
            
            var localPlayerScript = localPlayerObj.GetComponent<Player>();
            if (localPlayerScript != null)
            {
                UnitSelectionManager.Instance.SetLocalPlayer(localPlayerScript);
                UnitSelectionManager.Instance.SetActivePlayer(localPlayerScript.Object.InputAuthority); 
            }
        }
        
        private IEnumerator WaitForLocalPlayer(NetworkRunner runner, PlayerRef playerRef)
        {
            NetworkObject localPlayerObj;
            while (!runner.TryGetPlayerObject(playerRef, out localPlayerObj))
            {
                yield return null;
            }
            var localPlayerScript = localPlayerObj.GetComponent<Player>();
            UnitSelectionManager.Instance.SetLocalPlayer(localPlayerScript);
            UnitSelectionManager.Instance.SetActivePlayer(localPlayerScript.Object.InputAuthority); 
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
            {
                data.buttons.Set(NetworkInputData.MOUSEBUTTON0, true);
            }
            
            if (Input.GetMouseButton(1))
            {
                data.buttons.Set(NetworkInputData.MOUSEBUTTON1, true);
                
                if (RaycastUtility.TryRaycastFromCamera(Input.mousePosition, out RaycastHit rayHit))
                {
                    if (rayHit.collider.TryGetComponent(out Unit hitUnit))
                    {
                        var hitGridPos = LevelGrid.Instance.GetGridPosition(hitUnit.GetWorldPosition());
                        data.targetGridX = hitGridPos.x;
                        data.targetGridZ = hitGridPos.z;
                        data.targetPosition = rayHit.point;
                    }
                    else
                    {
                        var clickedGridPosition = LevelGrid.Instance.GetGridPosition(rayHit.point);
                        data.targetGridX = clickedGridPosition.x;
                        data.targetGridZ = clickedGridPosition.z;
                        data.targetPosition = rayHit.point;
                    }
                }
            }

            if (Input.GetKey(KeyCode.U))
            {
                data.buttons.Set(NetworkInputData.SPAWNUNIT, true);
                data.spawnPosition = MouseWorldPosition.GetMouseWorldPosition();
            }
            
            var selectionChange = UnitSelectionManager.Instance.GetNextSelectionChange();
            if (selectionChange.HasValue)
            {
                data.buttons.Set(NetworkInputData.SELECT_UNIT, true);
                data.selectedUnitId = selectionChange.Value.unitId;
                data.isSelected = selectionChange.Value.isSelected;                
            }
            
            ActionType localAction = UnitActionSystem.Instance.GetLocalSelectedAction();
            data.actionType = localAction;

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