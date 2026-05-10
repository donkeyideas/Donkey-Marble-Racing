using UnityEngine;
using MarbleRace.Data;

namespace MarbleRace.Runtime.Marble
{
    public class MarbleIdentity : MonoBehaviour
    {
        [SerializeField] private MarbleData marbleData;

        private Renderer _renderer;

        public string MarbleId => marbleData != null ? marbleData.marbleId : "";
        public string MarbleName => marbleData != null ? marbleData.marbleName : "Unknown";
        public Color MarbleColor => marbleData != null ? marbleData.marbleColor : Color.white;
        public MarbleRarity Rarity => marbleData != null ? marbleData.rarity : MarbleRarity.Common;
        public MarbleData Data => marbleData;

        public void Setup(MarbleData data)
        {
            marbleData = data;
            ApplyVisuals();
        }

        private void Start()
        {
            ApplyVisuals();
        }

        private void ApplyVisuals()
        {
            if (marbleData == null) return;

            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                if (marbleData.marbleMaterial != null)
                {
                    _renderer.material = marbleData.marbleMaterial;
                }
                else
                {
                    // Set color while keeping metallic properties
                    _renderer.material.color = marbleData.marbleColor;
                }
            }

        }
    }
}
