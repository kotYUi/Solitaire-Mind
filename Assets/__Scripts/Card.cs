using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    
    [Header("Set D")]
    public string suit;
    public int rank;
    public Color color = Color.black; // цвет значка
    public string colS = "Black"; // имя цвета
    public List<GameObject> decoGOs = new List<GameObject>();
    public List<GameObject> pipGOs = new List<GameObject>();

    public GameObject back; 
    public GameObject goldBack; 

    public CardDefinition def; 

    public SpriteRenderer[] spriteRenderers;
    public bool isGoldenCard = false;
    
    private void Start()
    {
        SetSortOrder(0);
    }
   
    public void PopulateSpriteRenderers()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    public void SetSortingLayerName(string tSLN)
    {
        PopulateSpriteRenderers();
        foreach(SpriteRenderer tSR in spriteRenderers)
        {
            tSR.sortingLayerName = tSLN;
        }
    }

    public void SetSortOrder(int sOrd)
    {
        PopulateSpriteRenderers();
        foreach (SpriteRenderer tSR in spriteRenderers)
        {
            if (tSR.gameObject == this.gameObject)
            {
                tSR.sortingOrder = sOrd;
                continue;
            }

            switch (tSR.gameObject.name)
            {
                case "back":
                    tSR.sortingOrder = sOrd+2;
                    break;
                case "face":
                    tSR.sortingOrder= sOrd+1;
                    break;
            }

        }
    }

    public bool faceUp
    {
        get
        {
            return (!back.activeSelf);
        }
        set
        {
            back.SetActive(!value);
        }
    }

    virtual public void OnMouseUpAsButton() 
    {
        print("No");
    }
}


[System.Serializable]
public class Decorator
{
    public string type; // достоинство карты
    public Vector3 loc; // позиция спрайта на карте
    public bool flip = false; // признак переворота спрайта
    public float scale = 1f;
}


[System.Serializable]
public class CardDefinition
{
    public string face;
    public int rank;
    public List<Decorator> pips = new List<Decorator>(); // значки
}

