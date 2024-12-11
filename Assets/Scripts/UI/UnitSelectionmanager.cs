using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Unit_Activities
{
    public class UnitSelectionManager : NetworkBehaviour
    {
        public static UnitSelectionManager Instance { get; private set; }

        [SerializeField] private RectTransform selectionBoxVisual;
        [SerializeField] private LayerMask unitLayerMask;

        private Vector3 startPosition;
        private List<Unit> selectedUnits = new List<Unit>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartSelection();
            }

            if (Input.GetMouseButton(0))
            {
                UpdateSelection();
            }

            if (Input.GetMouseButtonUp(0))
            {
                EndSelection();
            }
        }

        private void StartSelection()
        {
            selectionBoxVisual.gameObject.SetActive(true);
            startPosition = Input.mousePosition;
            UpdateSelectionBox(startPosition, Input.mousePosition);
        }

        private void UpdateSelection()
        {
            UpdateSelectionBox(startPosition, Input.mousePosition);
        }

        private void EndSelection()
        {
            selectionBoxVisual.gameObject.SetActive(false);
            SelectUnitsInBox();
        }

        private void UpdateSelectionBox(Vector3 start, Vector3 end)
        {
            Vector3 center = (start + end) / 2f;
            selectionBoxVisual.position = center;

            float width = Mathf.Abs(start.x - end.x);
            float height = Mathf.Abs(start.y - end.y);
            selectionBoxVisual.sizeDelta = new Vector2(width, height);
        }

        private void SelectUnitsInBox()
        {
            Rect selectionRect = GetSelectionRect();
            selectedUnits.Clear();

            Unit[] allUnits = FindObjectsOfType<Unit>();
            foreach (Unit unit in allUnits)
            {
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(unit.transform.position);
                if (selectionRect.Contains(screenPosition))
                {
                    selectedUnits.Add(unit);
                }
            }

            // Update the UnitActionSystem with the new selection
            if (selectedUnits.Count > 0)
            {
                UnitActionSystem.Instance.SetSelectedUnits(selectedUnits);
            }
        }

        private Rect GetSelectionRect()
        {
            Vector3 end = Input.mousePosition;
            Vector3 min = Vector3.Min(startPosition, end);
            Vector3 max = Vector3.Max(startPosition, end);
            return new Rect(min, max - min);
        }
    }
}
