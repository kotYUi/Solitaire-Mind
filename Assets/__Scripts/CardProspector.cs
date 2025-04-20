using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eCardState 
{ 
    drawpile,
    tableau,
    target,
    discard
}


public class CardProspector : Card
{
    [Header("Set D")]
    public eCardState state = eCardState.drawpile;
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    public int layoutID;
    public SlotDef slotDef;
    private Vector3 offset;
    private Vector3 startPosition;
    private Transform startParent;
    private eCardState startState;

    public override void OnMouseUpAsButton()
    {
        Prospector.S.CardClicked(this);
    }
}
