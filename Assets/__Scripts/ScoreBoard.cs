using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{
    public static ScoreBoard S;

    [Header("Set in I")]
    public GameObject prefabFloatingScore;
    public Text scoreBoardText;
    public Color goldenScoreColor;

    [Header("Set D")]
    [SerializeField] private int _score = 0;
    [SerializeField] private string _scoreString;

    private Transform canvasTrans;

    public int score
    {
        get
        {
            return _score;
        }
        set
        {
            _score = value;
            _scoreString = _score.ToString("N0");
        }
    }

    public string scoreString
    {
        get
        {
            return _scoreString;
        }
        set
        {
            _scoreString = value;
            if (scoreBoardText != null)
                scoreBoardText .text = _scoreString;
        }
    }

    private void Awake()
    {
        if (S  == null)
        {
            S = this;
            scoreBoardText = GetComponent<UnityEngine.UI.Text>();
        }
            

        canvasTrans = transform.parent;     
    }

    public void FSCallback(FloatingScore fs)
    {
        score += fs.score;
    }

    public FloatingScore CreateFloatingScore(int amt, List<Vector2> pts, bool isGolden = false)
    {
        GameObject go = Instantiate<GameObject>(prefabFloatingScore);
        go.transform.SetParent(canvasTrans);
        FloatingScore fs = go.GetComponent<FloatingScore>();
        fs.score = amt;
        fs.reportFinishTo = this.gameObject;

        Color textColor = Color.white; // По умолчанию - белый
        if (isGolden)
        {
            textColor = goldenScoreColor; // Используем золотой цвет, если isGolden = true
        }
        fs.Init(pts, textColor); // Передаем цвет и значения по умолчанию для eTimeS и eTimeD
        return fs;
    }
}
