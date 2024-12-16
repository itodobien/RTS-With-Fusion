using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Unit_Activities
{
    public class UnitSelectionManager : MonoBehaviour
    {
        public static UnitSelectionManager Instance { get; private set; }
        public event EventHandler OnSelectedUnitsChanged;

        [SerializeField] private RectTransform selectionBoxVisual;
        [SerializeField] private LayerMask unitLayerMask;
        private PlayerRef localPlayerRef;

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
            localPlayerRef = _runner.LocalPlayer;
            Debug.Log($"NetworkRunner found. Local PlayerRef: {localPlayerRef}");
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

            bool unitSelected = false;

            if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, unitLayerMask))
            {
                Debug.Log($"Raycast hit: {raycastHit.transform.name}");
                if (raycastHit.transform.TryGetComponent(out Unit unit))
                {
                    Debug.Log($"Unit found: {unit.name}, OwnerPlayerRef: {unit.OwnerPlayerRef}, Local PlayerRef: {localPlayerRef}");
                    if (unit.OwnerPlayerRef == localPlayerRef)
                    {
                        SetSelectedUnits(new List<Unit> { unit });
                        Debug.Log($"Unit selected successfully by Local PlayerRef: {localPlayerRef}");
                        unitSelected = true;
                    }
                    else
                    {
                        Debug.Log($"Selection FAILED: Unit belongs to {unit.OwnerPlayerRef}, but Local PlayerRef is {localPlayerRef}");
                        SetSelectedUnits(new List<Unit>());
                    }
                }
            }
            
            if (!unitSelected)
            {
                Debug.Log("No unit selected");
                SetSelectedUnits(new List<Unit>());
            }
        }

        private List<Unit> GetUnitsInSelectionBox()
        {
            List<Unit> unitsInBox = new List<Unit>();
            Rect selectionRect = GetScreenRect(selectionBoxStart, selectionBoxEnd);

            foreach (var unit in FindObjectsByType<Unit>(FindObjectsSortMode.None))
            {
                if (unit.OwnerPlayerRef == localPlayerRef)
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

        private void SetSelectedUnits(List<Unit> units)
        {
            foreach (var unit in selectedUnits)
            {
                unit.SetSelected(false);
            }

            selectedUnits.Clear();
            selectedUnits.AddRange(units);

            foreach (var unit in selectedUnits)
            {
                unit.SetSelected(true);
            }

            OnSelectedUnitsChanged?.Invoke(this, EventArgs.Empty);
            Debug.Log($"Selected Units: {selectedUnits.Count}, Local PlayerRef: {localPlayerRef}");
        }

        public List<Unit> GetSelectedUnits()
        {
            return new List<Unit>(selectedUnits);
        }
        public void AddToSelection(Unit unit)
        {
            if (!selectedUnits.Contains(unit))
            {
                selectedUnits.Add(unit);
                OnSelectedUnitsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public void SetLocalPlayerRef(PlayerRef playerRef)
        {
            localPlayerRef = playerRef;
            Debug.Log($"SetLocalPlayerRef called. Local PlayerRef is now: {localPlayerRef}");
        }
    }
}