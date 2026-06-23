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

        [Header("StarFill Mode")]
        [Tooltip("Sprite shown on a star image when that tier is reached.")]
        public Sprite filledStar;
        [Tooltip("Sprite shown on a star image when that tier is not yet reached.")]
        public Sprite emptyStar;

        [Header("BadgeSprite Mode")]
        [Tooltip("Index 0 = bronze (≤ twoStar), 1 = silver (≤ threeStar), 2 = gold (> threeStar).\n" +
                 "Needs exactly 3 sprites assigned.")]
        public Sprite[] achievementSprites = new Sprite[3];
    }
}