using UnityEngine;

namespace GameFramework.UI
{
    [CreateAssetMenu(fileName = "UISettings", menuName = "GameFramework/UI Settings")]
    public class UISettings : ScriptableObject
    {
        [SerializeField] private string _prefabRoot;
        [SerializeField] private Vector2 _referenceResolution = new Vector2(1920, 1080);
        [SerializeField] private float _matchWidthOrHeight = 1f;
        [SerializeField] private float _cameraDistance = 120f;

        public string PrefabRoot => _prefabRoot;
        public Vector2 ReferenceResolution => _referenceResolution;
        public float MatchWidthOrHeight => _matchWidthOrHeight;
        public float CameraDistance => _cameraDistance;

        public int SortingOrder(UILayer layer) => 100 + (int)layer * 10;
    }
}
