using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ResultKit.AbS
{
    /// <summary>
    /// Defines how achievements are displayed on the result screen.
    /// </summary>
    public enum AchievementDisplayMode
    {
        /// <summary>
        /// Classic mode: fills 1, 2, or 3 star images using filledStar / emptyStar sprites from config.
        /// </summary>
        StarFill,

        /// <summary>
        /// Badge mode: shows a single achievement sprite from <see cref="ResultConfig.achievementSprites"/>
        /// (index 0 = bronze / 1-star tier, 1 = silver / 2-star tier, 2 = gold / 3-star tier)
        /// and also updates scoreIconImage to match.
        /// </summary>
        BadgeSprite
    }

    /// <summary>
    /// Core Result System responsible for:
    /// - Collecting question results
    /// - Calculating score
    /// - Showing final result UI
    /// </summary>
    public class ResultSystem : MonoBehaviour
    {
        #region Singleton

        public static ResultSystem Instance { get; private set; }

        #endregion Singleton

        #region Inspector References

        [Header("Config")]
        [SerializeField] private ResultConfig config;
        public bool showMissedCount; // Whether to display the count of missed/skipped questions in the result summary.
        public int keepInScrolview = 2; // Number of "question panels/ui objects" to keep in the scroll view before recycling.
        public int keepInScrolviewAssessment = 1; // Number of "question panels/ui objects" to keep in the scroll view before recycling in assessment mode.

        [Header("Achievement Display")]
        [Tooltip("StarFill  – fills 1/2/3 star images using filledStar/emptyStar from ResultConfig (classic).\n" +
                 "BadgeSprite – picks one sprite from ResultConfig.achievementSprites and updates scoreIconImage.")]
        [SerializeField] private AchievementDisplayMode achievementDisplayMode = AchievementDisplayMode.StarFill;

        [Header("Screen")]
        [SerializeField] private GameObject resultScreen;
        [SerializeField] private GameObject assessmentResultScreen;

        [Header("Stats Text")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text timeTakenText;
        [SerializeField] private TMP_Text correctText;
        [SerializeField] private TMP_Text missedText;
        [SerializeField] private TMP_Text incorrectText;

        [Header("Stats UI")]
        [SerializeField] private Image scoreIconImage;

        [Header("Achievement Images")]
        [SerializeField] private Image[] achievementImages;

        [Header("Question Panels")]
        [SerializeField] private Transform panelParent;
        [SerializeField] private GameObject questionPanelPrefab;

        [Header("Assessment Mode (Optional)")]
        [SerializeField] private GameObject assessmentTitlePrefab;

        #endregion Inspector References

        #region Events

        public event Action<ResultSummary> OnResultShown;

        #endregion Events

        #region Runtime Data

        private List<QuestionResult> _results = new List<QuestionResult>();

        private int _totalScore = 0;

        private int _scorePerCorrectAnswer = 10;

        #endregion Runtime Data

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        #endregion Unity Lifecycle

        #region Public API

        /// <summary>
        /// Clears all previous results.
        /// Call at game start.
        /// </summary>
        public void ClearResults()
        {
            _results.Clear();
            _totalScore = 0;
        }

        /// <summary>
        /// Change score per correct answer.
        /// Default = 10.
        /// </summary>
        public void SetScorePerAnswer(int points)
        {
            _scorePerCorrectAnswer = points;
        }

        /// <summary>
        /// Add result after each question.
        /// </summary>
        public void AddQuestionResult(int questionNo, string question, string selected, string correct)
        {
            bool missed = string.IsNullOrEmpty(selected);
            bool isCorrect = !missed && selected == correct;

            QuestionResult result = new QuestionResult
            {
                questionNo = questionNo,
                question = question,
                selectedOption = missed ? "" : selected,
                correctOption = correct,
                isCorrect = isCorrect,
                isMissed = missed
            };

            int index = _results.FindIndex(r => r.questionNo == questionNo);

            if (index >= 0)
            {
                if (_results[index].isCorrect)
                    _totalScore -= _scorePerCorrectAnswer;

                _results[index] = result;
            }
            else
            {
                _results.Add(result);
            }

            if (isCorrect)
                _totalScore += _scorePerCorrectAnswer;
        }

        /// <summary>
        /// Call this when the game finishes.
        /// </summary>
        public void ShowFinalResult(int maxScore, float timeTaken, int totalQuestions, bool isAssessment = false)
        {
            ResultSummary summary = new ResultSummary
            {
                totalScore = _totalScore,
                maxScore = maxScore,
                timeTaken = timeTaken,
                totalQuestions = totalQuestions,
                results = new List<QuestionResult>(_results)
            };

            ShowResult(summary, isAssessment);
        }

        /// <summary>
        /// Hide result screen.
        /// </summary>
        public void Hide()
        {
            resultScreen.SetActive(false);
        }

        #endregion Public API

        #region Internal Result Logic

        private void ShowResult(ResultSummary summary, bool isAssessment = false)
        {
            if (!isAssessment)
            {
                ShowDetailedResult(summary);
                resultScreen.SetActive(true); // Show the result screen
            }
            else
            {
                if (assessmentResultScreen)
                    assessmentResultScreen.SetActive(true); // Show the assessment result screen
                else
                {
                    ShowAssessmentResult();
                    resultScreen.SetActive(true); // Show the result screen
                }
            }

            ApplyAchievement(summary.ScorePercentage);

            OnResultShown?.Invoke(summary);
        }

        private void ShowDetailedResult(ResultSummary s)
        {
            if (scoreText)
                scoreText.text = s.totalScore.ToString();

            if (timeTakenText)
                timeTakenText.text = FormatTime(s.timeTaken);

            if (correctText)
                correctText.text = $"{s.CorrectCount}/{s.totalQuestions}";

            if (incorrectText)
            {
                if (showMissedCount) // If showing missed count, only show incorrect count (excluding missed) in the incorrectText.
                    incorrectText.text = $"{s.IncorrectCount}/{s.totalQuestions}";
                else // If not showing missed count, show total incorrect + missed in the incorrectText.
                    incorrectText.text = $"{s.IncorrectCount + s.MissedCount}/{s.totalQuestions}";
            }

            if (missedText && showMissedCount)
                missedText.text = $"{s.MissedCount}/{s.totalQuestions}";

            RebuildPanels(s.results, keepInScrolview);
        }

        private void ShowAssessmentResult()
        {
            RebuildPanels(null, keepInScrolviewAssessment);

            if (assessmentTitlePrefab && panelParent)
                Instantiate(assessmentTitlePrefab, panelParent);
        }

        #endregion Internal Result Logic

        #region UI Building

        private void RebuildPanels(List<QuestionResult> results, int keepInScrolview = 2)
        {
            if (!panelParent)
                return;

            for (int i = panelParent.childCount - 1; i >= keepInScrolview; i--)
                Destroy(panelParent.GetChild(i).gameObject);

            if (results == null)
                return;

            foreach (var r in results)
            {
                var obj = Instantiate(questionPanelPrefab, panelParent);
                obj.GetComponent<QuestionPanel>()?.Setup(r);
            }
        }

        /// <summary>
        /// Routes achievement display to the correct method based on <see cref="achievementDisplayMode"/>.
        /// </summary>
        private void ApplyAchievement(float pct)
        {
            switch (achievementDisplayMode)
            {
                case AchievementDisplayMode.StarFill:
                    ApplyStarFill(pct);
                    break;

                case AchievementDisplayMode.BadgeSprite:
                    ApplyBadgeSprite(pct);
                    break;
            }
        }

        /// <summary>
        /// Classic mode: fills 1 / 2 / 3 star images using filledStar / emptyStar from config.
        /// </summary>
        private void ApplyStarFill(float pct)
        {
            if (achievementImages == null || config == null)
                return;

            bool[] filled =
            {
                pct > config.oneStar,
                pct > config.twoStar,
                pct > config.threeStar
            };

            for (int i = 0; i < achievementImages.Length && i < filled.Length; i++)
            {
                achievementImages[i].sprite =
                    filled[i] ? config.filledStar : config.emptyStar;

                achievementImages[i].gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Badge mode: resolves a tier index (0 / 1 / 2) from the score percentage,
        /// sets every achievementImage to the matching sprite from config.achievementSprites,
        /// and also updates scoreIconImage.
        /// </summary>
        private void ApplyBadgeSprite(float pct)
        {
            if (config == null || config.achievementSprites == null || config.achievementSprites.Length < 3)
            {
                Debug.LogWarning("[ResultSystem] BadgeSprite mode requires 3 sprites assigned in ResultConfig.achievementSprites.");
                return;
            }

            // Resolve tier: 2 = gold (3-star), 1 = silver (2-star), 0 = bronze (1-star)
            int tierIndex;
            if (pct > config.threeStar)
                tierIndex = 2;
            else if (pct > config.twoStar)
                tierIndex = 1;
            else
                tierIndex = 0;

            Sprite badgeSprite = config.achievementSprites[tierIndex];

            // Update all achievement images
            if (achievementImages != null)
            {
                foreach (Image img in achievementImages)
                {
                    if (img == null) continue;
                    img.sprite = badgeSprite;
                    img.gameObject.SetActive(true);
                }
            }

            // Update score icon
            if (scoreIconImage != null)
                scoreIconImage.sprite = badgeSprite;
        }

        #endregion UI Building

        #region Utilities

        /// <summary>
        /// Converts seconds into MM:SS format.
        /// </summary>
        private static string FormatTime(float seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            return $"{t.Minutes:D2}:{t.Seconds:D2}";
        }

        #endregion Utilities
    }
}