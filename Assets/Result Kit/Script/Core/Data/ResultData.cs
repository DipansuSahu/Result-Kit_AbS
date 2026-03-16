using System;
using System.Collections.Generic;

namespace ResultKit.AbS
{
    #region Data Models

    /// <summary>
    /// Stores result information for a single question.
    /// </summary>
    [Serializable]
    public class QuestionResult
    {
        public int questionNo;         // Question number
        public string question;        // Question text
        public string selectedOption;  // Player selected option
        public string correctOption;   // Correct answer
        public bool isCorrect;         // True if player answered correctly
        public bool isMissed;          // True if player skipped question
    }

    /// <summary>
    /// Contains the full result summary for the game session.
    /// </summary>
    [Serializable]
    public class ResultSummary
    {
        public int totalScore;
        public int maxScore;
        public float timeTaken;
        public int totalQuestions;

        // List containing all question results
        public List<QuestionResult> results = new();

        #region Calculated Properties

        /// <summary>Total number of correct answers.</summary>
        public int CorrectCount => results.FindAll(r => r.isCorrect).Count;

        /// <summary>Total incorrect answers.</summary>
        public int IncorrectCount => totalQuestions - CorrectCount;

        /// <summary>Score percentage for star rating.</summary>
        public float ScorePercentage =>
            maxScore > 0 ? (float)totalScore / maxScore * 100f : 0f;

        #endregion Calculated Properties
    }

    #endregion Data Models
}