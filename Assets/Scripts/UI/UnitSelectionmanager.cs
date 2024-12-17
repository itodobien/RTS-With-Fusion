using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

namespace Unit_Activities
{
    public class UnitSelectionManager : NetworkBehaviour
    {
        
        [Networked] private NetworkDictionary<PlayerRef, NetworkArray<NetworkId>> SelectedUnits { get; }
        public static UnitSelectionManager Instance { get; private set; }
        public event EventHandler OnSelectedUnitsChanged;

        [SerializeField] private RectTransform selectionBoxVisual;
        [SerializeField] private LayerMask unitLayerMask;
        private PlayerRef activePlayer;

        private Vector2 selectionBoxStart;
        private Vector2 selectionBoxEnd;

        private bool isMouseDown;
        private bool isMouseDragging;
        private Vector3 mouseStartPosition;
        private List<Unit> selectedUnits = new();

        private NetworkRunner _runner;

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
            isMouseDown = false;
            isMouseDragging = false;
            selectionBoxVisual.gameObject.SetActive(false);
            StartCoroutine(WaitForNetworkRunner());
        }

        private IEnumerator WaitForNetworkRunner()
        {
            while (FindObjectOfType<NetworkRunner>() == null)
            {
                yield return null;
            }
            _runner = FindObjectOfType<NetworkRunner>();
            activePlayer = _runner.LocalPlayer;
            Debug.Log($"NetworkRunner found. Local PlayerRef: {activePlayer}");
        }

        private void Update()
        {
            HandleMouseInputs();
            if (isMouseDragging)
            {
                UpdateSelectionBoxVisual();
            }
        }

        private void HandleMouseInputs()
        {
            if (Input.GetMouseButtonDown(0))
            {
                isMouseDown = true;
                isMouseDragging = false;
                mouseStartPosition = Input.mousePosition;
                selectionBoxStart = mouseStartPosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (isMouseDragging)
                {
                    List<Unit> unitsInSelection = GetUnitsInSelectionBox();
                    SetSelectedUnits(unitsInSelection);
                }
                else
                {
                    TrySingleUnitSelection(Input.mousePosition);
                }

                isMouseDown = false;
                isMouseDragging = false;
                selectionBoxVisual.gameObject.SetActive(false);
            }

            if (isMouseDown && !isMouseDragging)
            {
                if (Vector3.Distance(Input.mousePosition, mouseStartPosition) > 10f)
                {
                    isMouseDragging = true;
                    selectionBoxVisual.gameObject.SetActive(true);
                }
            }
        }
        
        public void SelectUnit(Unit unit, bool selected)
        {
            if (HasStateAuthority)
            {
                bool selectionChanged = false;
                PlayerRef localPlayer = Runner.LocalPlayer;
                List<NetworkId> currentSelection = new List<NetworkId>();

                if (SelectedUnits.TryGet(localPlayer, out NetworkArray<NetworkId> existingSelection))
                {
                    currentSelection.AddRange(existingSelection);
                }

                if (selected && !currentSelection.Contains(unit.Object.Id))
                {
                    currentSelection.Add(unit.Object.Id);
                    selectionChanged = true;
                }
                else if (!selected && currentSelection.Contains(unit.Object.Id))
                {
                    currentSelection.Remove(unit.Object.Id);
                    selectionChanged = true;
                }

                if (selectionChanged)
                {
                    NetworkArray<NetworkId> newSelection = new NetworkArray<NetworkId>();
                    for (int i = 0; i < currentSelection.Count; i++)
                    {
                        newSelection.Set(i, currentSelection[i]);
                    }
                    SelectedUnits.Set(localPlayer, newSelection);
                }
                unit.SetSelected(selected);
            }
        }


        public bool IsUnitSelected(Unit unit)
        {
            return SelectedUnits.TryGet(Runner.LocalPlayer, out var playerUnits) && 
                   playerUnits.Contains(unit.Object.Id);
        }
        private void UpdateSelectionBoxVisual()
        {
            selectionBoxEnd = Input.mousePosition;
            Vector2 boxStart = selectionBoxStart;
            Vector2 boxEnd = selectionBoxEnd;

            Vector2 boxCenter = (boxStart + boxEnd) / 2;
            selectionBoxVisual.position = boxCenter;

            Vector2 boxSize = new Vector2(
                Mathf.Abs(boxEnd.x - boxStart.x),
                Mathf.Abs(boxEnd.y - boxStart.y)
            );
            selectionBoxVisual.sizeDelta = boxSize;
        }

        private void TrySingleUnitSelection(Vector3 mousePosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, unitLayerMask))
            {
                if (raycastHit.transform.TryGetComponent(out Unit unit))
                {
                    if (unit.OwnerPlayerRef == activePlayer)
                    {
                        SetSelectedUnits(new List<Unit> { unit });
                        Debug.Log($"Unit '{unit.name}' selected by player {activePlayer}.");
                    }
                    else
                    {
                        Debug.Log($"Cannot select unit '{unit.name}' - owned by player {unit.OwnerPlayerRef}, not {activePlayer}.");
                        SetSelectedUnits(new List<Unit>()); 
                    }
                    return;
                }
            }
            Debug.Log("No selectable unit detected.");
            SetSelectedUnits(new List<Unit>());
        }

        private List<Unit> GetUnitsInSelectionBox()
        {
            List<Unit> unitsInBox = new List<Unit>();
            Rect selectionRect = GetScreenRect(selectionBoxStart, selectionBoxEnd);

            foreach (var unit in FindObjectsByType<Unit>(FindObjectsSortMode.None))
            {
                if (unit.OwnerPlayerRef == activePlayer)
                {
                    Vector3 unitScreenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
                    if (selectionRect.Contains(unitScreenPos))
                    {
                        unitsInBox.Add(unit);
                    }
                }
            }
            return unitsInBox;
        }

        private Rect GetScreenRect(Vector2 screenPosition1, Vector2 screenPosition2)
        {
            screenPosition1.y = Screen.height - screenPosition1.y;
            screenPosition2.y = Screen.height - screenPosition2.y;
            Vector2 bottomLeft = Vector2.Min(screenPosition1, screenPosition2);
            Vector2 topRight = Vector2.Max(screenPosition1, screenPosition2);
            return Rect.MinMaxRect(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y);
        }

        private void SetSelectedUnits(IEnumerable<Unit> units)
        {
            foreach (var unit in selectedUnits.Except(units))
            {
                unit.SetSelected(false);
            }
            foreach (var unit in units.Except(selectedUnits))
            {
                unit.SetSelected(true);
            }
            selectedUnits = units.ToList();

            OnSelectedUnitsChanged?.Invoke(this, EventArgs.Empty);

            Debug.Log($"Selected Units: {selectedUnits.Count}, Local PlayerRef: {activePlayer}");
        }

        public List<Unit> GetSelectedUnits(PlayerRef playerRef)
        {
            List<Unit> units = new List<Unit>();
            if (SelectedUnits.TryGet(playerRef, out NetworkArray<NetworkId> selectedUnits))
            {
                for (int i = 0; i < selectedUnits.Length; i++)
                {
                    NetworkId unitId = selectedUnits.Get(i);
                    if (Runner.TryFindObject(unitId, out NetworkObject networkObject))
                    {
                        if (networkObject.TryGetComponent(out Unit unit))
                        {
                            units.Add(unit);
                        }
                    }
                }
            }
            return units;
        }
        public void SetActivePlayer(PlayerRef playerRef)
        {
            activePlayer = playerRef;
            Debug.Log($"Active player set: {activePlayer}");
        }
    }
}