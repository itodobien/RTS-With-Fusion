using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unit_Activities
{
    public class UnitSelectionManager : MonoBehaviour
    {
        public static UnitSelectionManager Instance { get; private set; }

        public event EventHandler OnSelectedUnitsChanged;

        [SerializeField] private RectTransform selectionBoxVisual;
        [SerializeField] private LayerMask unitLayerMask;

        private Vector2 selectionBoxStart;
        private Vector2 selectionBoxEnd;

        private bool isMouseDown;
        private bool isMouseDragging;
        private Vector3 mouseStartPosition;
        private List<Unit> selectedUnits = new();

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
            if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, unitLayerMask))
            {
                if (raycastHit.transform.TryGetComponent(out Unit unit))
                {
                    SetSelectedUnits(new List<Unit> { unit });
                    return;
                }
            }
            SetSelectedUnits(new List<Unit>());
        }
        private List<Unit> GetUnitsInSelectionBox()
        {
            List<Unit> unitsInBox = new List<Unit>();
            Rect selectionRect = GetScreenRect(selectionBoxStart, selectionBoxEnd);

            foreach (var unit in FindObjectsByType<Unit>(FindObjectsSortMode.None))
            {
                Vector3 unitScreenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
                if (selectionRect.Contains(unitScreenPos))
                {
                    unitsInBox.Add(unit);
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
            Debug.Log("Selected Units: " + selectedUnits.Count);
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
    }
}
