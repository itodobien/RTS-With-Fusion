using TMPro;
using UnityEngine;

namespace Grid
{
    public class GridDebugObject : MonoBehaviour
    {
        [SerializeField] private TextMeshPro textMeshPro;
        private GridObject _gridObject;

        public void SetGridObject(GridObject gridObject)
        {
            this._gridObject = gridObject;
        }

        private void Update()
        {
            textMeshPro.text = _gridObject.ToString();
        }
    }
}
