using UnityEngine;

namespace ResultKit.AbS
{
    [CreateAssetMenu(fileName = "ResultConfig", menuName = "Result/Config")]
    public class ResultConfig : ScriptableObject
    {
        [Header("Star Thresholds (%)")]
        public float oneStar = 40f;
        public float twoStar = 70f;
        public float threeStar = 90f;

        [Header("Star Sprites")]
        public Sprite filledStar;
        public Sprite emptyStar;
    }
}