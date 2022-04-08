using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitType unitType;
    public Team teamColor;
    public UnitStatus status;
    private bool mouseOver = false;

    private void OnMouseEnter()
    {
        mouseOver = true;
    }

    private void OnMouseExit()
    {
        mouseOver = false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(mouseOver && status == UnitStatus.Active && GameManager.currentTurn == TeamTurn.Blue_Turn)
            {
                //Check valid tiles
                MapTileManager.FindMoveableTiles(unitType);
            }
        }
    }

    public void MoveUnit()
    {

    }
}

public enum Team
{
    Blue,
    Red
}

public enum UnitStatus
{
    Active,
    Inactive
}
