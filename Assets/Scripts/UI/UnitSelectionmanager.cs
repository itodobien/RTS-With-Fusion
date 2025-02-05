using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Units;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class UnitSelectionManager : MonoBehaviour
    {
        public static UnitSelectionManager Instance { get; private set; }
        public event EventHandler OnSelectedUnitsChanged;

        [SerializeField] private RectTransform selectionBoxVisual;
        [SerializeField] private LayerMask unitLayerMask;
        
        private Vector2 _selectionBoxStart;
        private Vector2 _selectionBoxEnd;
        
        private Vector3 _mouseStartPosition;
        private List<Unit> _selectedUnits = new(); 
        private PlayerRef _activePlayer; 
        private Player _localPlayer;

        private bool _isMouseDragging;
        private bool _isMouseDown;
        
        private readonly HashSet<(NetworkId unitId, bool isSelected)> _pendingSelectionChanges = new ();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            _isMouseDown = false;
            _isMouseDragging = false;
            selectionBoxVisual.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_localPlayer == null) return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ForceClearSelectionBox();
                UpdateSelectedUnits(new List<Unit>());
            }
            if (Input.GetMouseButtonDown(0)) 
            {
                if (EventSystem.current.IsPointerOverGameObject()) return;
                _isMouseDown = true;
                _isMouseDragging = false;
                _mouseStartPosition = Input.mousePosition;
                _selectionBoxStart = _mouseStartPosition;
            }
            else if (_isMouseDown && Input.GetMouseButton(0)) 
            {
                if (!_isMouseDragging) 
                {
                    if (Vector3.Distance(Input.mousePosition, _mouseStartPosition) > 10f) 
                    { _isMouseDragging = true;
                        selectionBoxVisual.gameObject.SetActive(true);
                    }
                }
            }
            else if (_isMouseDown && Input.GetMouseButtonUp(0)) 
            {
                if (_isMouseDragging) 
                {
                    var unitsInSelection = GetUnitsInSelectionBox();
                    UpdateSelectedUnits(unitsInSelection);
                }
                else 
                {
                    TrySingleUnitSelection(Input.mousePosition);
                }
                _isMouseDown = false;
                _isMouseDragging = false;
                selectionBoxVisual.gameObject.SetActive(false);
            }
            if (_isMouseDragging) 
            {
                UpdateSelectionBoxVisual();
            }
        }

        public void SetLocalPlayer(Player player) => _localPlayer = player;
        internal Player GetLocalPlayer() => _localPlayer;
        
        private void TrySingleUnitSelection(Vector3 mousePosition)
        {
            if (Camera.main != null)
            {
                
                if (RaycastUtility.TryRaycastFromCamera(mousePosition, out RaycastHit rayHit))
                {
                    if (rayHit.transform.TryGetComponent(out Unit unit))
                    {
                        if (!unit.Object || !unit.Object.IsInSimulation)
                        {
                            UpdateSelectedUnits(new List<Unit>());
                            return;
                        }

                        if (_localPlayer == null)
                        {
                            UpdateSelectedUnits(new List<Unit>());
                            return;
                        }

                        if (unit.GetTeamID() ==  _localPlayer.GetTeamID() && unit.OwnerPlayerRef == _activePlayer)
                        {
                            UpdateSelectedUnits(new List<Unit> { unit }); 
                        }
                        else
                        {
                            UpdateSelectedUnits(new List<Unit>());
                        }
                        return;
                    }
                }
            }
            UpdateSelectedUnits(new List<Unit>());
        }
        
        private void UpdateSelectionBoxVisual()
        {
            _selectionBoxEnd = Input.mousePosition;

            Vector2 boxStart = _selectionBoxStart;
            Vector2 boxEnd = _selectionBoxEnd;

            Vector2 boxCenter = (boxStart + boxEnd) * 0.5f; 
            selectionBoxVisual.position = boxCenter;     

            Vector2 boxSize = new Vector2(
                Mathf.Abs(boxEnd.x - boxStart.x),        
                Mathf.Abs(boxEnd.y - boxStart.y)         
            );
            selectionBoxVisual.sizeDelta = boxSize;
        }

        private List<Unit> GetUnitsInSelectionBox()
        {
            if (_localPlayer == null)
            {
                return new List<Unit>();
            }
            
            List<Unit> unitsInBox = new();
            Rect selectionRect = GetScreenRect(_selectionBoxStart, _selectionBoxEnd);

            foreach (var unit in FindObjectsByType<Unit>(FindObjectsSortMode.None))
            {
                int localPlayerTeam = _localPlayer.GetTeamID();
                
                if (!unit.Object || !unit.Object.IsInSimulation) continue;
                        
                if (unit.GetTeamID() == localPlayerTeam && unit.Object != null && unit.Object.IsValid)
                {
                    if (Camera.main != null)
                    {
                        Vector3 unitScreenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
                        if (selectionRect.Contains(unitScreenPos))
                        {
                            unitsInBox.Add(unit);
                        }
                    }
                }
            }
            return unitsInBox;
        }

        private Rect GetScreenRect(Vector2 start, Vector2 end)
        {
            Vector2 bottomLeft = Vector2.Min(start, end);
            Vector2 topRight = Vector2.Max(start, end);
            return Rect.MinMaxRect(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y);
        }

        private void UpdateSelectedUnits(List<Unit> newSelection)
        {
            _selectedUnits.RemoveAll(unit => unit == null || !unit.Object || !unit.Object.IsInSimulation);
            
            foreach (var unit in _selectedUnits.ToList())
            {
                if (!newSelection.Contains(unit))
                {
                    _pendingSelectionChanges.Add((unit.Object.Id, false));
                }
            }
            foreach (var unit in newSelection)
            {
                if (!_selectedUnits.Contains(unit))
                {
                    _pendingSelectionChanges.Add((unit.Object.Id, true));
                }
            }
            _selectedUnits = newSelection.ToList();
            OnSelectedUnitsChanged?.Invoke(this, EventArgs.Empty);
        }
        public (NetworkId unitId, bool isSelected)? GetNextSelectionChange()
        {
            if (_pendingSelectionChanges.Count > 0)
            {
                var change = _pendingSelectionChanges.First();
                _pendingSelectionChanges.Remove(change);
                return change;
            }
            return null;
        }

        public void ForceDeselectUnit(Unit unit)
        {
            if (unit == null || !unit.Object || !unit.Object.IsInSimulation)
                return;
        
            if (_selectedUnits.Remove(unit))
            {
                _pendingSelectionChanges.Add((unit.Object.Id, false));
                OnSelectedUnitsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void CleanupDestroyedUnits()
        {
            int removedCount = _selectedUnits.RemoveAll(unit => unit == null || !unit.Object || !unit.Object.IsInSimulation);
    
            if (removedCount > 0)
            {
                Debug.Log($"Cleaned up {removedCount} destroyed units from selection.");
                OnSelectedUnitsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ForceClearSelectionBox()
        {
            _isMouseDown = false;
            _isMouseDragging = false;
            selectionBoxVisual.gameObject.SetActive(false);
        }


        public void SetActivePlayer(PlayerRef playerRef)
        {
            _activePlayer = playerRef;
        }
        
        public List<Unit> GetSelectedUnits()
        {
            return new List<Unit>(_selectedUnits);
        }
    }
}