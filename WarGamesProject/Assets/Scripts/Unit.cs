using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitType unitType;
    public Team teamColor;
    public UnitStatus status;
    public GameObject highlight;
    private bool mouseOver = false;

    private void OnMouseEnter()
    {
        mouseOver = true;
        highlight.SetActive(true);
    }

    private void OnMouseExit()
    {
        mouseOver = false;
        highlight.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(mouseOver && status == UnitStatus.Active) //Also ADD a check if it's the players turn
            {
                //Check valid tiles
            }
        }
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