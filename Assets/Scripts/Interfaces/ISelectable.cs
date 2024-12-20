using UnityEngine;
using Fusion;

public interface ISelectable
{
    int Team { get; }
    
    PlayerRef OwnerPlayerRef { get; }
    NetworkBool IsSelected { get; set; }
    
    GameObject GameObject { get; }
    
    void Select();
    void Deselect();
}