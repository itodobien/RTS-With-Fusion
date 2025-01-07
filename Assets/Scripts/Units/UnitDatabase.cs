using UnityEngine;

[CreateAssetMenu(fileName = "UnitDatabase", menuName = "RTS/UnitDatabase", order = 1)]
public class UnitDatabase : ScriptableObject
{
    public UnitData[] unitDataList; // Array or List<UnitData>
}