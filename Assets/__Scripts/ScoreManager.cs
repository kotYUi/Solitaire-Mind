using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eScoreEvent
{
    draw,
    mine,
    gameWin,
    gameLoss
}

public class ScoreManager : MonoBehaviour
{
    static private ScoreManager S;

    static public int SCORE_FROM_PREV_ROUND = 0;
    static public int HIGH_SCORE = 0;

    [Header("Set in I")]
    public int chain = 0;
    public int scoreRun = 0;
    public int score = 0;

    private void Awake()
    {
        if (S == null)
        {
            S = this;
        }
        else
        {
            Debug.LogError("no score managerr");
        }

        if (PlayerPrefs.HasKey("ProspectorHighScore"))
        {
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }

        score += SCORE_FROM_PREV_ROUND;
        SCORE_FROM_PREV_ROUND = 0;
    }

    static public void EVENT(eScoreEvent evt, bool isGolden = false) 
    {
        try
        {
            S.Event(evt, isGolden); // Передаем isGolden во внутренний метод Event
        }
        catch
        {
            Debug.LogError("s == null");
        }
    }

    void Event(eScoreEvent evt, bool isGolden = false)
    {
        int mult = 1;
        
        if (isGolden) // Используем параметр isGolden
        {
            mult = 3;
        }
        switch (evt)
        { 
            case eScoreEvent.draw:
            case eScoreEvent.gameLoss:
            case eScoreEvent.gameWin:
                chain = 0;
                score += scoreRun;
                scoreRun = 0;
                UpdateScoreBoard();
                break;

            case eScoreEvent.mine:
                chain++;
                scoreRun += chain * mult;
                break;
        }

        switch (evt)
        { 
            case eScoreEvent.gameWin:
                SCORE_FROM_PREV_ROUND = score;
                print("you won" + score);
                break;

            case eScoreEvent.gameLoss:
                if (HIGH_SCORE <= score)
                {
                    HIGH_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                }
                else
                {
                    print("this is not high score");
                }
                break;
        }
    }

    void UpdateScoreBoard()
    {
        if (ScoreBoard.S != null)
        {
            ScoreBoard.S.score = score;
            // Принудительное обновление
            ScoreBoard.S.scoreString = score.ToString("N0");
        }
        else Debug.LogWarning("ScoreBoard instance missing!");
    }

    static public int CHAIN { get {return S.chain; } }
    static public int SCORE {  get {return S.score;} }
    static public int SCORE_RUN {  get {return S.scoreRun;} }
}
