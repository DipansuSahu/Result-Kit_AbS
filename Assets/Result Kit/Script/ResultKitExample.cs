using UnityEngine;
using ResultKit.AbS;

public class ResultKitExample : MonoBehaviour
{
    void Start()
    {
        ResultSystem.Instance.SetScorePerAnswer(10); // set score per correct answer, default is 10

        // Always clear at game start
        ResultSystem.Instance.ClearResults();

        OnQuestionAnswered(1, "koi bhi option"); // simulate player wrong answer
        OnQuestionAnswered(2, "correct option hai ye"); // simulate player correct answer
        OnQuestionAnswered(3, ""); // simulate player missed answer

        ShowResult(); // simulate game end after all questions are answered
    }

    // Call this after player answers each question
    void OnQuestionAnswered(int questionNo, string selectedOption)
    {
        ResultSystem.Instance.AddQuestionResult(
            questionNo: questionNo,
            question: "this is a question", // QuestionManager.instance.GetQuestion(questionNo) in real case
            selected: selectedOption,   // pass "" or null if skipped
            correct: "correct option hai ye" // QuestionManager.instance.GetCorrectOption(questionNo) in real case
        );
    }

    // Call this when game ends
    void ShowResult()
    {
        ResultSystem.Instance.ShowFinalResult(
            maxScore: 30,
            timeTaken: 150,
            totalQuestions: 3
        );
    }
}