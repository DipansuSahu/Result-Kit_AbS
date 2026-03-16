using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ResultKit.AbS
{
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

        [Header("Screen")]
        [SerializeField] private GameObject resultScreen;

        [Header("Stats UI")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text timeTakenText;
        [SerializeField] private TMP_Text correctText;
        [SerializeField] private TMP_Text incorrectText;

        [Header("Stars")]
        [SerializeField] private Image[] starImages;

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
        public void AddQuestionResult(
            int questionNo,
            string question,
            string selected,
            string correct)
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
        public void ShowFinalResult(
            int maxScore,
            float timeTaken,
            int totalQuestions,
            bool isAssessment = false)
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
                ShowDetailedResult(summary);
            else
                ShowAssessmentResult();

            ApplyStars(summary.ScorePercentage);

            resultScreen.SetActive(true);

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
                incorrectText.text = $"{s.IncorrectCount}/{s.totalQuestions}";

            RebuildPanels(s.results);
        }

        private void ShowAssessmentResult()
        {
            RebuildPanels(null);

            if (assessmentTitlePrefab && panelParent)
                Instantiate(assessmentTitlePrefab, panelParent);
        }

        #endregion Internal Result Logic

        #region UI Building

        private void RebuildPanels(List<QuestionResult> results)
        {
            if (!panelParent)
                return;

            for (int i = panelParent.childCount - 1; i >= 2; i--)
                Destroy(panelParent.GetChild(i).gameObject);

            if (results == null)
                return;

            foreach (var r in results)
            {
                var obj = Instantiate(questionPanelPrefab, panelParent);
                obj.GetComponent<QuestionPanel>()?.Setup(r);
            }
        }

        private void ApplyStars(float pct)
        {
            if (starImages == null || config == null)
                return;

            bool[] filled =
            {
                pct > config.oneStar,
                pct > config.twoStar,
                pct > config.threeStar
            };

            for (int i = 0; i < starImages.Length && i < filled.Length; i++)
            {
                starImages[i].sprite =
                    filled[i] ? config.filledStar : config.emptyStar;

                starImages[i].gameObject.SetActive(true);
            }
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