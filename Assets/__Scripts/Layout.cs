using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

[System.Serializable]
public class SlotDef
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public string type = "slot";
    public Vector2 stagger;
}

public class Layout : MonoBehaviour
{
    public PT_XMLReader xmlr;
    public PT_XMLHashtable xml;
    public Vector2 multiplier;

    public List<SlotDef> slotDefs;
    public SlotDef drawPile;
    public SlotDef discardPile;

    public string[] sortingLayerNames = new string[] { "Row0", "Row1", "Row2", "Row3", "Discard", "Draw" };

    public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText);
        xml = xmlr.xml["xml"][0];

        // Парсинг multiplier
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"), CultureInfo.InvariantCulture);
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"), CultureInfo.InvariantCulture);

        SlotDef tSD;
        PT_XMLHashList slotX = xml["slot"];

        for (int i = 0; i < slotX.Count; i++)
        {
            tSD = new SlotDef();
            tSD.type = slotX[i].HasAtt("type") ? slotX[i].att("type") : "slot";

            // Обязательные атрибуты
            tSD.x = float.Parse(slotX[i].att("x"), CultureInfo.InvariantCulture);
            tSD.y = float.Parse(slotX[i].att("y"), CultureInfo.InvariantCulture);
            tSD.layerID = int.Parse(slotX[i].att("layer"));
            tSD.layerName = sortingLayerNames[tSD.layerID];

            switch (tSD.type)
            {
                case "slot":
                    tSD.faceUp = slotX[i].att("faceup") == "1";
                    tSD.id = int.Parse(slotX[i].att("id"));

                    if (slotX[i].HasAtt("hiddenby"))
                    {
                        string[] hiding = slotX[i].att("hiddenby").Split(',');
                        foreach (string s in hiding)
                        {
                            tSD.hiddenBy.Add(int.Parse(s));
                        }
                    }
                    slotDefs.Add(tSD);
                    break;

                case "drawpile":
                    tSD.stagger.x = float.Parse(slotX[i].att("xstagger"), CultureInfo.InvariantCulture);
                    drawPile = tSD;
                    break;

                case "discardpile":
                    discardPile = tSD;
                    break;
            }
        }
    }
}