using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum eFSState
{
    idle,
    pre,
    active,
    post
}

public class FloatingScore : MonoBehaviour
{
    [Header("Set D")]
    public eFSState state = eFSState.idle;

    [SerializeField]
    protected int _score = 0;
    public string scoreString;
    public bool isGoldenCard = false;
    public Color normalColor = Color.white;

    private Text txt; 

    public int score
    {
        get
        {
            return _score;
        }
        set
        {
            _score = value;
            scoreString = score.ToString("N0");
            txt.text = scoreString;
        }
    }

    public List<Vector2> bezierPts;
    public List<float> fontSizes;

    public float timeDuration = 1f;
    public float timeStart = -1f;
    public string easingCurve = Easing.InOut;

    public GameObject reportFinishTo = null;

    private RectTransform rectTrans;

    void Awake()
    {
        txt = GetComponent<Text>();
        if (txt == null)
        {
            Debug.LogError("FloatingScore: TextMeshProUGUI component not found!");
        }
    }

    public void Init(List<Vector2> ePts, Color textColor, float eTimeS = 0, float eTimeD = 1)
    {
        rectTrans = GetComponent<RectTransform>();
        rectTrans.anchoredPosition = Vector2.zero;

        Debug.Log("Setting text color to: " + textColor);
        txt.color = textColor;
        bezierPts = new List<Vector2>(ePts);

        if (ePts.Count == 1)
        {
            transform.position = ePts[0];
            return;
        }

        if (eTimeS == 0) eTimeS = Time.time;
        timeStart = eTimeS;
        timeDuration = eTimeD;
        state = eFSState.pre;
    }

    public void FSCallback(FloatingScore fs)
    {
        score += fs.score;
    }

    private void Update()
    {
        if (state == eFSState.idle) return;
        float u = (Time.time - timeStart) / timeDuration;
        float uC = Easing.Ease(u, easingCurve);
        if (u < 0)
        {
            state = eFSState.pre;
            gameObject.SetActive(false); // Используем gameObject.SetActive
        }
        else
        {
            if (u >= 1)
            {
                uC = 1;
                state = eFSState.post;
                if (reportFinishTo != null)
                {
                    reportFinishTo.GetComponent<ScoreBoard>()?.FSCallback(this);
                    Destroy(gameObject);
                }
                else
                {
                    state = eFSState.idle;
                }
            }
            else
            {
                state = eFSState.active;
                gameObject.SetActive(true); // Используем gameObject.SetActive
            }
        }

        Vector2 pos = Utils.Bezier(uC, bezierPts);
        rectTrans.anchorMin = rectTrans.anchorMax = pos;
        if (fontSizes != null && fontSizes.Count > 0)
        {
            int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
            txt.fontSize = size; // Используем txt.fontSize
        }
    }
}