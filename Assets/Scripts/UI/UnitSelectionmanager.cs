using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;

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

        private bool isMouseDragging;
        private bool isMouseDown;

        private Vector3 mouseStartPosition;
        private List<Unit> selectedUnits = new(); 
        private PlayerRef activePlayer; 

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
                    var unitsInSelection = GetUnitsInSelectionBox();
                    UpdateSelectedUnits(unitsInSelection);
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
        private void TrySingleUnitSelection(Vector3 mousePosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, unitLayerMask))
            {
                if (raycastHit.transform.TryGetComponent(out Unit unit))
                {
                    if (unit.OwnerPlayerRef == activePlayer)
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
            UpdateSelectedUnits(new List<Unit>());
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

        private List<Unit> GetUnitsInSelectionBox()
        {
            List<Unit> unitsInBox = new();
            Rect selectionRect = GetScreenRect(selectionBoxStart, selectionBoxEnd);

            foreach (var unit in FindObjectsOfType<Unit>())
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

        private Rect GetScreenRect(Vector2 start, Vector2 end)
        {
            start.y = Screen.height - start.y; 
            end.y = Screen.height - end.y;
            Vector2 bottomLeft = Vector2.Min(start, end);
            Vector2 topRight = Vector2.Max(start, end);
            return Rect.MinMaxRect(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y);
        }

        private void UpdateSelectedUnits(List<Unit> newSelection)
        {
            foreach (var unit in selectedUnits)
            {
                unit.SetSelected(false);
            }
            foreach (var unit in newSelection)
            {
                if (unit.OwnerPlayerRef == activePlayer)
                {
                    unit.SetSelected(true);
                }
            }
            selectedUnits = newSelection.ToList();
            OnSelectedUnitsChanged?.Invoke(this, EventArgs.Empty);
        }


        public void SetActivePlayer(PlayerRef playerRef)
        {
            activePlayer = playerRef;
        }
        
        public List<Unit> GetSelectedUnits()
        {
            return new List<Unit>(selectedUnits);
        }
    }
}