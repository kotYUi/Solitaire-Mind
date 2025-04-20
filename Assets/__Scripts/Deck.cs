using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization; // Для CultureInfo.InvariantCulture

public class Deck : MonoBehaviour
{
    public static Deck S;

    [Header("Set in I")]
    public bool startFaceUp = false;
    public Sprite suitClub;
    public Sprite suitDiamond;
    public Sprite suitHeart;
    public Sprite suitSpade;

    public Sprite[] faceSprites;
    public Sprite[] rankSprites;

    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;

    public GameObject prefabSprite;
    public GameObject prefabCard;
    public GameObject prefabGoldenCard;
    public Sprite goldFont;

    public int handSize = 28;

    [Range(0f, 1f)]
    public float goldenCardChance = 0.03f;


    [Header("Set Dynamically")]
    public PT_XMLReader xmlr;
    public List<string> cardNames;
    public List<Card> cards;
    public List<Card> handCards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardDefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuits;

    public void InitDeck(string deckXMLText)
    {
        if (GameObject.Find("_Deck") == null)
        {
            GameObject anchorGO = new GameObject("_Deck");
            deckAnchor = anchorGO.transform;
        }

        dictSuits = new Dictionary<string, Sprite>()
        {
            {"C", suitClub},
            {"D", suitDiamond},
            {"H", suitHeart},
            {"S", suitSpade},
        };

        ReadDeck(deckXMLText);

        MakeCards();
    }

    public void ReadDeck(string deckXMLText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(deckXMLText);

        // Инициализация списка декораторов
        decorators = new List<Decorator>();

        PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
        Decorator deco;

        for (int i = 0; i < xDecos.Count; i++)
        {
            deco = new Decorator();

            // Обрабатываем обязательные атрибуты
            deco.type = xDecos[i].att("type");
            deco.flip = (xDecos[i].att("flip") == "1");

            // Обрабатываем scale (может отсутствовать)
            if (xDecos[i].HasAtt("scale"))
            {
                if (!float.TryParse(xDecos[i].att("scale"), NumberStyles.Float, CultureInfo.InvariantCulture, out deco.scale))
                {
                    Debug.LogWarning($"Failed to parse 'scale' in decorator {i}, using default 1.0");
                    deco.scale = 1.0f;
                }
            }
            else
            {
                deco.scale = 1.0f;
            }

            // Обрабатываем координаты x, y, z
            if (!float.TryParse(xDecos[i].att("x"), NumberStyles.Float, CultureInfo.InvariantCulture, out deco.loc.x))
            {
                Debug.LogWarning($"Failed to parse 'x' in decorator {i}, using default 0");
                deco.loc.x = 0f;
            }

            if (!float.TryParse(xDecos[i].att("y"), NumberStyles.Float, CultureInfo.InvariantCulture, out deco.loc.y))
            {
                Debug.LogWarning($"Failed to parse 'y' in decorator {i}, using default 0");
                deco.loc.y = 0f;
            }

            if (!float.TryParse(xDecos[i].att("z"), NumberStyles.Float,CultureInfo.InvariantCulture, out deco.loc.z))
            {
                Debug.LogWarning($"Failed to parse 'z' in decorator {i}, using default 0");
                deco.loc.z = 0f;
            }

            decorators.Add(deco);
        }

        // Инициализация определений карт
        cardDefs = new List<CardDefinition>();
        PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];

        for (int i = 0; i < xCardDefs.Count; i++)
        {
            CardDefinition cDef = new CardDefinition();

            // Обрабатываем rank
            if (!int.TryParse(xCardDefs[i].att("rank"), out cDef.rank))
            {
                Debug.LogWarning($"Failed to parse 'rank' in card {i}, using default 0");
                cDef.rank = 0;
            }

            // Обрабатываем pips
            PT_XMLHashList xPips = xCardDefs[i]["pip"];
            if (xPips != null)
            {
                for (int j = 0; j < xPips.Count; j++)
                {
                    deco = new Decorator();
                    deco.type = "pip";

                    // Обрабатываем flip
                    deco.flip = (xPips[j].att("flip") == "1");

                    // Обрабатываем координаты
                    if (!float.TryParse(xPips[j].att("x"), NumberStyles.Float, CultureInfo.InvariantCulture, out deco.loc.x))
                    {
                        Debug.LogWarning($"Failed to parse pip 'x' in card {i}, pip {j}");
                        deco.loc.x = 0f;
                    }

                    if (!float.TryParse(xPips[j].att("y"), NumberStyles.Float, CultureInfo.InvariantCulture, out deco.loc.y))
                    {
                        Debug.LogWarning($"Failed to parse pip 'y' in card {i}, pip {j}");
                        deco.loc.y = 0f;
                    }

                    if (!float.TryParse(xPips[j].att("z"), NumberStyles.Float, CultureInfo.InvariantCulture, out deco.loc.z))
                    {
                        Debug.LogWarning($"Failed to parse pip 'z' in card {i}, pip {j}");
                        deco.loc.z = 0f;
                    }

                    // Обрабатываем scale (может отсутствовать)
                    if (xPips[j].HasAtt("scale"))
                    {
                        if (!float.TryParse(xPips[j].att("scale"), NumberStyles.Float, CultureInfo.InvariantCulture, out deco.scale))
                        {
                            Debug.LogWarning($"Failed to parse pip 'scale' in card {i}, pip {j}");
                            deco.scale = 1.0f;
                        }
                    }
                    else
                    {
                        deco.scale = 1.0f;
                    }

                    cDef.pips.Add(deco);
                }
            }

            // Обрабатываем face (может отсутствовать)
            if (xCardDefs[i].HasAtt("face"))
            {
                cDef.face = xCardDefs[i].att("face");
            }

            cardDefs.Add(cDef);
        }
    }

    public CardDefinition GetCardDefinitionByRank(int rnk)
    {
        foreach(CardDefinition cd in cardDefs)
        {
            if (cd.rank == rnk)
            {
                return cd;
            }
        }
        return null;
    }

    public void MakeCards()
    {
        cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach (string s in letters)
        {
            for (int i = 0; i < 13; i++)
            {
                cardNames.Add(s + (i + 1));
            }
        }

        cards = new List<Card>();

        for (int i = 0; i < cardNames.Count - handSize; i++) // Вычитаем initialHandSize
        {
            cards.Add(MakeCard(i, false)); // Создаем обычные карты для колоды
        }

        // Создаем карты для руки (с шансом быть золотыми)
        for (int i = cardNames.Count - handSize; i < cardNames.Count; i++)
        {
            bool shouldBeGolden = (Random.value <= goldenCardChance);
            cards.Add(MakeCard(i, shouldBeGolden)); // Создаем карты для руки
        }
    }

    public Card MakeCard(int cNum, bool isGoldenCard)
    {
        GameObject cgo = Instantiate(isGoldenCard ? prefabGoldenCard : prefabCard) as GameObject;
        cgo.transform.parent = deckAnchor;
        Card card = cgo.GetComponent<Card>();

        cgo.transform.localPosition = new Vector3 ((cNum%13)*3, cNum/13*4, 0);

        card.name = cardNames[cNum];
        card.suit = card.name[0].ToString();
        card.rank = int.Parse(card.name.Substring(1));
        if (card.suit == "D" || card.suit == "H")
        {
            card.colS = "Red";
            card.color = Color.red;
        }
        card.def = GetCardDefinitionByRank(card.rank);
        card.isGoldenCard = isGoldenCard;

        AddDecorators(card);
        AddPips(card);
        AddFace(card);
        AddBack(card);
        card.faceUp = startFaceUp;
        return card;
    }

    private Sprite _tSp = null;
    private GameObject _tGO = null;
    private SpriteRenderer _tSR = null;

    private void AddDecorators(Card card)
    {
        foreach(Decorator deco in decorators)
        {
            if (deco.type == "suit")
            {
                _tGO = Instantiate(prefabSprite) as GameObject;
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                _tSR.sprite = dictSuits[card.suit];
            }
            else
            {
                _tGO = Instantiate(prefabSprite) as GameObject;
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                _tSp = rankSprites[card.rank];
                _tSR.sprite = _tSp;
                _tSR.color = card.color;
            }

            _tSR.sortingOrder = 1;
            _tGO.transform.SetParent(card.transform);
            _tGO.transform.localPosition = deco.loc;

            if (deco.flip)
            {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            if (deco.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * deco.scale;
            }

            _tGO.name = deco.type;
            card.decoGOs.Add(_tGO);
        }
    }

    private void AddPips(Card card)
    {
        foreach(Decorator pip in card.def.pips)
        {
            _tGO = Instantiate(prefabSprite) as GameObject;
            _tGO.transform.SetParent(card.transform);
            _tGO.transform.localPosition = pip.loc;
            if (pip.flip)
            {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            if (pip.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * pip.scale;
            }
            _tGO.name = "pip";
            _tSR = _tGO.GetComponent<SpriteRenderer>();
            _tSR.sprite = dictSuits[card.suit];
            _tSR.sortingOrder = 1;
            card.pipGOs.Add(_tGO);
        }
    }

    private void AddFace(Card card)
    {
        if (card.def.face == "")
        {
            return;
        }
        _tGO = Instantiate(prefabSprite) as GameObject;
        _tSR = _tGO.GetComponent<SpriteRenderer>();
        _tSp = GetFace(card.def.face + card.suit);
        _tSR.sprite = _tSp;
        _tSR.sortingOrder = 1;
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;
        _tGO.name = "face";      
    }

    private Sprite GetFace(string faceS)
    {
        foreach(Sprite _tSP in faceSprites)
        {
            if (_tSP.name == faceS)
            {
                return _tSP;
            }
        }
        return null;
    }

    private void AddBack(Card card)
    {
        _tGO = Instantiate(prefabSprite) as GameObject;
        _tSR = _tGO.GetComponent<SpriteRenderer>();

        if (card.isGoldenCard)
        {
            _tSR.sprite = cardBackGold; // Устанавливаем "золотой" спрайт рубашки
        }
        else
        {
            _tSR.sprite = cardBack; 
        }
        
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;
        _tSR.sortingOrder = 2;
        _tGO.name = "back";
        card.back = _tGO;
        card.faceUp = startFaceUp;
    }

    static public void Shuffle(ref List<Card> oCards)
    {
        List<Card> tCards = new List<Card>();

        int ndx;

        tCards = new List<Card> ();

        while(oCards.Count > 0)
        {
            ndx = Random.Range(0, oCards.Count);
            tCards.Add(oCards[ndx]);
            oCards.RemoveAt(ndx);
        }

        oCards = tCards;
    }

}