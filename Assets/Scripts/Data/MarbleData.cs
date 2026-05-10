using UnityEngine;

namespace MarbleRace.Data
{
    public enum MarbleRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    [CreateAssetMenu(menuName = "MarbleRace/Data/Marble Data")]
    public class MarbleData : ScriptableObject
    {
        [Header("Identity")]
        public string marbleId;
        public string marbleName;
        public Color marbleColor = Color.white;
        public MarbleRarity rarity = MarbleRarity.Common;

        [Header("Physics Personality")]
        [Range(0.5f, 2f)] public float massMultiplier = 1f;
        [Range(0f, 0.5f)] public float dragMultiplier = 0.1f;
        [Range(0.3f, 1f)] public float bounciness = 0.6f;
        [Range(0.5f, 1.5f)] public float nudgeStrength = 1f;

        [Header("Visuals")]
        public Material marbleMaterial;
        public GameObject trailPrefab;

        [Header("Display")]
        [TextArea(1, 2)]
        public string description;
        public Sprite icon;
    }
}
