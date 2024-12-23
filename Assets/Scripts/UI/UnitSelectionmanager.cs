using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Fusion;
using Units;
using UnityEngine;

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

        private bool _isMouseDragging;
        private bool _isMouseDown;

        private Vector3 _mouseStartPosition;
        private List<Unit> _selectedUnits = new(); 
        private PlayerRef _activePlayer; 
        
        private readonly HashSet<(NetworkId unitId, bool isSelected)> _pendingSelectionChanges = new HashSet<(NetworkId, bool)>();

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
            HandleMouseInputs();
            if (_isMouseDragging)
            {
                UpdateSelectionBoxVisual();
            }
        }

        private void HandleMouseInputs()
        {
            switch (HandleMouseButtonState.GetMouseButtonState())
            {
                case HandleMouseButtonState.MouseButtonState.ButtonDown:
                    _isMouseDown = true;
                    _isMouseDragging = false;
                    _mouseStartPosition = Input.mousePosition;
                    _selectionBoxStart = _mouseStartPosition;
                    break;
                case HandleMouseButtonState.MouseButtonState.ButtonHeld:
                    if(_isMouseDown && !_isMouseDragging)
                    {
                        if (Vector3.Distance(Input.mousePosition, _mouseStartPosition) > 10f)
                        {
                            _isMouseDragging = true;
                            selectionBoxVisual.gameObject.SetActive(true);
                        }
                    }
                    break;
                case HandleMouseButtonState.MouseButtonState.ButtonUp:
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
                    break;
            }
        }
        
        private void TrySingleUnitSelection(Vector3 mousePosition)
        {
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, unitLayerMask))
                {
                    if (raycastHit.transform.TryGetComponent(out Unit unit))
                    {
                        if (unit.OwnerPlayerRef == _activePlayer)
                        {
                            UpdateSelectedUnits(new List<Unit> { unit }); 
                        }
                        else
                        {
                            UpdateSelectedUnits(new List<Unit>()); // Clear selection
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

            Vector2 boxCenter = (boxStart + boxEnd) / 2; 
            selectionBoxVisual.position = boxCenter;     

            Vector2 boxSize = new Vector2(
                Mathf.Abs(boxEnd.x - boxStart.x),        
                Mathf.Abs(boxEnd.y - boxStart.y)         
            );
            selectionBoxVisual.sizeDelta = boxSize;
        }

        private List<Unit> GetUnitsInSelectionBox()
        {
            List<Unit> unitsInBox = new();
            Rect selectionRect = GetScreenRect(_selectionBoxStart, _selectionBoxEnd);

            foreach (var unit in FindObjectsByType<Unit>(FindObjectsSortMode.None))
            {
                if (unit.OwnerPlayerRef == _activePlayer)
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
            /*start.y = Screen.height - start.y; // These 2 lines had completely broken my selection box selection accuracy. 
            end.y = Screen.height - end.y;*/
            Vector2 bottomLeft = Vector2.Min(start, end);
            Vector2 topRight = Vector2.Max(start, end);
            return Rect.MinMaxRect(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y);
        }

        private void UpdateSelectedUnits(List<Unit> newSelection)
        {
            foreach (var unit in _selectedUnits)
            {
                if (unit.Object.HasInputAuthority)
                {
                    _pendingSelectionChanges.Add((unit.Object.Id, false));
                }
            }
            foreach (var unit in newSelection)
            {
                if (unit.Object.HasInputAuthority)
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