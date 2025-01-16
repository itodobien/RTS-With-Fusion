using Units;
using UnityEngine;

namespace Fusion
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private float spawnDelay = 0.5f;
        [Networked] private TickTimer Delay { get; set; }
        [Networked] private int UnitPrefabIndex { get; set; }
        [Networked] private int TeamID { get; set; }

        public void SetUnitPrefabIndex(int index)
        {
            UnitPrefabIndex = index;
        }

        public void SetTeamID(int teamID)
        {
            if (Object.HasStateAuthority)
            {
                Debug.Log($"[Unit] Setting TeamID={teamID} for {name}");
                TeamID = teamID;
            }
        }

        public int GetTeamID()
        {
            return TeamID;
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                if (HasStateAuthority && Delay.ExpiredOrNotRunning(Runner))
                {
                    if (data.buttons.IsSet(NetworkInputData.SPAWNUNIT)) 
                    {
                        Delay = TickTimer.CreateFromSeconds(Runner, spawnDelay);
                        Vector3 spawnPos = data.spawnPosition != Vector3.zero
                            ? data.spawnPosition
                            : transform.position;
                        
                        var unitData = BasicSpawner.Instance.unitDatabase.unitDataList[UnitPrefabIndex];
                        NetworkPrefabRef chosenUnitPrefab = unitData.liveUnitPrefab;

                        Runner.Spawn(
                            chosenUnitPrefab,
                            spawnPos,
                            Quaternion.identity,
                            Object.InputAuthority,
                            (runner, spawnObject) =>
                            {
                                var spawnedUnit = spawnObject.GetComponent<Unit>();
                                if (spawnedUnit != null)
                                {
                                    spawnedUnit.OwnerPlayerRef = Object.InputAuthority;
                                    spawnedUnit.SetTeamID(TeamID);
                                    spawnedUnit.SetPrefabIndex(UnitPrefabIndex);
                                }
                            }
                        );
                    }
                }
            }
        }
    }
}