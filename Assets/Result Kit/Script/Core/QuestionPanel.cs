using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ResultKit.AbS
{
    /// <summary>
    /// Handles the UI display for a single question result.
    /// </summary>
    public class QuestionPanel : MonoBehaviour
    {
        #region Inspector References

        [Header("Backgrounds: [0]=Missed  [1]=Correct  [2]=Incorrect")]
        [SerializeField] private Sprite[] questionBg;
        [SerializeField] private Image bgImage;
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private TMP_Text selectedOptionText;
        [SerializeField] private TMP_Text correctOptionText;

        #endregion Inspector References

        #region Public Methods

        /// <summary>
        /// Setup panel with question result data.
        /// </summary>
        public void Setup(QuestionResult r)
        {
            questionText.text = $"Q{r.questionNo}. {r.question}";

            if (r.isMissed)
            {
                selectedOptionText.text = "<color=white>Not Attempted</color>";
                correctOptionText.text = $"<color=white>Correct:</color> {r.correctOption}";
                SetBg(0);
            }
            else if (r.isCorrect)
            {
                selectedOptionText.gameObject.SetActive(false);
                correctOptionText.text = $"<color=white>Correct:</color> {r.correctOption}";
                SetBg(1);
            }
            else
            {
                selectedOptionText.text = $"<color=white>Selected:</color> {r.selectedOption}";
                correctOptionText.text = $"<color=white>Correct:</color> {r.correctOption}";
                SetBg(2);
            }
        }

        #endregion Public Methods

        #region Internal

        /// <summary>
        /// Set background based on result state.
        /// </summary>
        private void SetBg(int index)
        {
            if (bgImage && questionBg != null && index < questionBg.Length)
                bgImage.sprite = questionBg[index];
        }

        #endregion Internal
    }
}