using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Prospector : MonoBehaviour
{
    static public Prospector S;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public Deck deck;
    public TextAsset layoutXML;
    public Layout layout;
    public List<CardProspector> drawPile;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter;
    public Transform layoutAnchor;
    public CardProspector target;
    public List<CardProspector> tableau;
    public List<CardProspector> discardPile;
    public FloatingScore fsRun;
    public float reloadDelay = 2f;
    public Text gameOverText, roundResultText, highScoreText;
    public Sprite normalFont;
    public Sprite backNormal;
    public Vector2 fsPosMid = new Vector2(0.5f, 0.9f);
    public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
    public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
    public Vector2 fcPosEnd = new Vector2(0.5f, 0.95f);

    const float Z_OFFSET = 1000f;

    [Header("Drag&Drop Settings")]
    [Range(0.1f, 2f)]
    public float dropTolerance = 0.5f;

    private void Awake()
    {
        S = this;
        SetUpUITexts();
    }

    private void Start()
    {
        ScoreBoard.S.score = ScoreManager.SCORE;
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<Layout>();
        layout.ReadLayout(layoutXML.text);
        drawPile = ConvertListCardsToListCardProspectors(deck.cards);
        LayoutGame();
        foreach (CardProspector card in drawPile) {
        card.faceUp = false; // Принудительная инициализация
    }
    }

    void SetUpUITexts()
    {
        GameObject go = GameObject.Find("HighScore");
        if (go != null )
        {
            highScoreText = go.GetComponent<Text>();
        }
        int highScore = ScoreManager.HIGH_SCORE;
        string hScore = "High Score:" + Utils.AddCommasToNumber(highScore);

        go.GetComponent<Text>().text = hScore;

        go = GameObject.Find("GameOver");
        if (go != null)
        {
            gameOverText = go.GetComponent<Text>();
        }
        go = GameObject.Find("RoundResult");
        if (go != null)
        {
            roundResultText = go.GetComponent<Text>();
        }

        ShowResultsUI(false);
    }

    void ShowResultsUI(bool show)
    {
        gameOverText.gameObject.SetActive(show);
        roundResultText.gameObject.SetActive(show);
    }

    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD)
    {
        List<CardProspector> lCP = new List<CardProspector>();
        foreach (Card tCD in lCD)
        {
            lCP.Add(tCD as CardProspector);
        }
        return lCP;
    }

    CardProspector Draw()
    {
        if (drawPile.Count == 0) return null;

        CardProspector cd = drawPile[0];
        drawPile.RemoveAt(0);

        // Включаем только необходимые компоненты
        cd.faceUp = true;
        cd.GetComponent<Collider>().enabled = true;
        foreach (var pip in cd.pipGOs)
        {
            pip.SetActive(true);
        }
        foreach (var deco in cd.decoGOs)
        {
            deco.SetActive(true);
        }

        return cd;
    }

    void LayoutGame()
    {   
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        foreach (SlotDef tSD in layout.slotDefs)
        {
            CardProspector cp = Draw();

            if (cp != null) // Проверяем, что cp не равен null
            {
                cp.faceUp = tSD.faceUp;
                cp.transform.parent = layoutAnchor;

                cp.transform.localPosition = new Vector3(
                    layout.multiplier.x * tSD.x,
                    layout.multiplier.y * tSD.y,
                    -tSD.layerID
                );

                cp.layoutID = tSD.id;
                cp.slotDef = tSD;
                cp.state = eCardState.tableau;
                cp.SetSortingLayerName(tSD.layerName);
                tableau.Add(cp);
            }
        }

        foreach(CardProspector tCP in tableau)
        {
            foreach(int hid in tCP.slotDef.hiddenBy)
            {
                CardProspector cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }

        CardProspector initialTarget = Draw();
        if (initialTarget != null)
        {
            initialTarget.faceUp = true; // Принудительно открываем карту
            initialTarget.GetComponent<Collider>().enabled = true;
            MoveToTarget(initialTarget);
        }
        UpdateDrawPile();    
    }

    CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach (CardProspector tCP in tableau)
        {
            if (tCP.layoutID == layoutID)
            {
                return tCP;
            }
        }
        return null;
    }

    void SetTableauFaces()
    {
        foreach(CardProspector cd in tableau)
        {
            bool faceUp = true;
            foreach(CardProspector cover in cd.hiddenBy)
            {
                if (cover.state == eCardState.tableau)
                {
                    faceUp = false;
                }
            }
            cd.faceUp = faceUp;
        }
    }

    public void CardClicked(CardProspector cd)
    {
        switch (cd.state)
        {
            case eCardState.target:
                // Ничего не делаем при клике на target
                break;

            case eCardState.drawpile:
                MoveToDiscard(target);
                MoveToTarget(Draw());
                UpdateDrawPile();
                ScoreManager.EVENT(eScoreEvent.draw);
                FloatingScoreHandler(eScoreEvent.draw);
                break;

            case eCardState.tableau:
                bool validMatch = true;
                if (!cd.faceUp)
                {
                    validMatch = false;
                }
                if (!AdjacentRank(cd, target))
                {
                    validMatch = false;
                }
                if (!validMatch) return;
                tableau.Remove(cd);
                MoveToTarget(cd);
                SetTableauFaces();
                ScoreManager.EVENT(eScoreEvent.mine, cd.isGoldenCard);
                FloatingScoreHandler(eScoreEvent.mine, cd.isGoldenCard);
                break;
        }
        CheckForGameOver();
    }

    void CheckForGameOver()
    {
        if (tableau.Count == 0)
        {
            GameOver(true);
            return;
        }

        if (drawPile.Count > 0)
        {
            return;
        }

        foreach (CardProspector cd in tableau)
        {
            if (AdjacentRank(cd, target))
            {
                return;
            }
        }

        GameOver(false);
    }

    void GameOver(bool won)
    {
        int score = ScoreManager.SCORE;
        if (fsRun != null)
            score += fsRun.score;

        if (won)
        {
            gameOverText.text = "Round is Over";
            roundResultText.text = "You won this round!\nRound Score: " + score;
            ShowResultsUI(true);
            ScoreManager.EVENT(eScoreEvent.gameWin);
            FloatingScoreHandler(eScoreEvent.gameWin);
        }
        else
        {
            gameOverText.text = "Game is Over";
            if (ScoreManager.HIGH_SCORE <= score)
            {
                string str = "You got the high score!\nHigh score: " + score;
                roundResultText.text = str;
            }
            else
            {
                roundResultText.text = "You final score was: " + score;
            }
            ShowResultsUI(true);
            ScoreManager.EVENT(eScoreEvent.gameLoss);
            FloatingScoreHandler(eScoreEvent.gameLoss);
        }
        
        Invoke("ReloadLevel1", reloadDelay);
    }

    public void ReloadLevel1()
    {
        SceneManager.LoadScene("__Prospector_Scene_0");
    }

    void FloatingScoreHandler(eScoreEvent evt, bool isGolden = false)
    {
        List<Vector2> fsPts;
        switch (evt) 
        { 
            case eScoreEvent.gameWin:
            case eScoreEvent.gameLoss:
            case eScoreEvent.draw:
                if (fsRun != null)
                {
                    fsPts = new List<Vector2>();
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fcPosEnd);
                    fsRun.reportFinishTo = ScoreBoard.S.gameObject;
                    if (isGolden)
                    {
                        fsRun.Init(fsPts, Color.yellow);
                    }
                    else
                    {
                        fsRun.Init(fsPts, Color.white);
                    }
                    
                    fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
                    fsRun = null;
                }
                break;

            case eScoreEvent.mine:
                FloatingScore fs;
                Vector2 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPts = new List<Vector2>();
                fsPts.Add(p0);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                fs = ScoreBoard.S.CreateFloatingScore(ScoreManager.CHAIN, fsPts, isGolden);
                fs.isGoldenCard = isGolden;
                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                if (fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    fs.reportFinishTo = fsRun.gameObject;
                }
                break;
        }
    }

    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        if (!c0.faceUp || !c1.faceUp) return false;

        if (Mathf.Abs(c0.rank - c1.rank)  == 1)
        {
            return true;
        }

        if (c0.rank == 1 &&  c1.rank == 13) return true;
        if(c0.rank == 13 && c1.rank == 1) return true;

        return false;
    }

    void MoveToDiscard(CardProspector cd)
    {
        if (cd == null) return;

        cd.state = eCardState.discard;
        discardPile.Add(cd);
        cd.transform.parent = layoutAnchor;

        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID + Z_OFFSET
        );

        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-10 + discardPile.Count);
    }

    public void MoveToTarget(CardProspector cd)
    {
        if (cd == null) return;

        if (target != null) MoveToDiscard(target);

        cd.gameObject.SetActive(true);
        foreach (SpriteRenderer renderer in cd.GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.gameObject.SetActive(true);
            renderer.sortingLayerName = "Draw";
        }

        target = cd;
        cd.state = eCardState.target;
        cd.transform.parent = layoutAnchor;

        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID
        );

        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        Debug.Log(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    void UpdateDrawPile()
    {
        foreach (CardProspector card in drawPile)
        {
            // Проверяем, является ли карта золотой
            if (card.isGoldenCard)
            {
                // Получаем SpriteRenderer карты
                SpriteRenderer spriteRenderer = card.gameObject.GetComponent<SpriteRenderer>();

                // Проверяем, что SpriteRenderer не равен null
                if (spriteRenderer != null)
                {
                    // Устанавливаем спрайт карты на normalFont
                    spriteRenderer.sprite = normalFont;
                }
                else
                {
                    Debug.LogError("SpriteRenderer is null on card: " + card.name);
                }

                // Получаем Transform карты
                Transform trCard = card.transform;

                // Получаем GameObject "back"
                GameObject goBack = trCard.Find("back")?.gameObject;  // Safe access

                // Проверяем, что GameObject "back" найден
                if (goBack != null)
                {
                    // Получаем SpriteRenderer "back"
                    SpriteRenderer back = goBack.GetComponent<SpriteRenderer>();

                    // Проверяем, что SpriteRenderer "back" не равен null
                    if (back != null)
                    {
                        // Устанавливаем спрайт "back" на Deck.S.cardBack
                        back.sprite = backNormal;
                    }
                    else
                    {
                        Debug.LogError("SpriteRenderer on 'back' is null on card: " + card.name);
                    }

                    //  Деактивируем "back"
                    goBack.SetActive(false);
                }
                else
                {
                    Debug.LogError("GameObject 'back' not found on card: " + card.name);
                }
                // Устанавливаем isGoldenCard в false
                card.isGoldenCard = false;
            }
        }

        for (int i = 0; i < drawPile.Count; i++)
        {
            CardProspector cd = drawPile[i];            
            cd.transform.parent = layoutAnchor;
            cd.state = eCardState.drawpile;
            cd.faceUp = false;
            cd.back.SetActive(true);

            foreach (Transform child in cd.transform)
            {
                if (child.gameObject != cd.back)
                {
                    child.gameObject.SetActive(false);
                }
            }

            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                -layout.drawPile.layerID + 0.1f * i
            );

            cd.faceUp = false; // Всегда false для drawPile!
            cd.state = eCardState.drawpile;
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    public Vector2 GetTargetPosition2D()
    {
        Vector3 targetPos = layoutAnchor.TransformPoint(new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID));

        return new Vector2(targetPos.x, targetPos.y);
    }
}