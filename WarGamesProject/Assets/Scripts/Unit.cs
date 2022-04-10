using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitType unitType;
    public Team teamColor;
    public UnitStatus status;
    private Animator animator;
    private MapTileManager tileManager;
    private Transform unitPosition;
    private bool mouseOver = false;
    private GameManager gameManager;

    private void OnMouseEnter()
    {
        mouseOver = true;
    }

    private void OnMouseExit()
    {
        mouseOver = false;
    }

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if(gameManager == null)
        {
            Debug.LogError("Could not find the Game Manager for the scene");
        }
        animator = GetComponent<Animator>();
        tileManager = FindObjectOfType<MapTileManager>();
        unitPosition = GetComponent<Transform>();
        if(tileManager == null)
        {
            Debug.LogError("Could not find a Map Tile Manager GameObject in the Scene");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(mouseOver && status == UnitStatus.Active && gameManager.currentTurn == TeamTurn.Blue_Turn)
            {
                //Check valid tiles
                tileManager.FindMoveableTiles(unitType, unitPosition.position);
                gameManager.SetSelectedUnit(gameObject);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            
        }
    }

    public void MoveUnit()
    {

    }

    public void UnitSetActive()
    {
        status = UnitStatus.Active;
        animator.enabled = true;
        GetComponent<SpriteRenderer>().color = gameManager.activeColor;
    }

    public void UnitSetInactive()
    {
        status = UnitStatus.Inactive;
        animator.enabled=false;
        if (teamColor == Team.Blue)
        {
            GetComponent<SpriteRenderer>().sprite = unitType.blueInactiveImage;
        }
        else if (teamColor == Team.Red)
        {
            GetComponent<SpriteRenderer>().sprite = unitType.redInactiveImage;
        }
        GetComponent<SpriteRenderer>().color = gameManager.inactiveColor;
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
