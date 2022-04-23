using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitType unitType;
    public Team teamColor;
    public bool unitActive = true;
    private Animator animator;
    private Transform unitPosition;
    private bool mouseOver = false;
    private GameManager gameManager;
    public GameObject healthBarUI;

    public int health;

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
        unitPosition = GetComponent<Transform>();

        health = unitType.maxHealth;
        gameManager.AddToUnitList(gameObject);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(gameManager.gameState == GameState.SelectingUnit)
            {
                if (mouseOver && unitActive && teamColor == gameManager.currentTurn)
                {
                    //Check valid
                    gameManager.SetSelectedUnit(gameObject, unitPosition.position);
                    gameManager.FindMoveableTiles(unitType, unitPosition.position);
                }
            }
        }
    }

    public void UnitSetActive()
    {
        unitActive = true;
        animator.enabled = true;
        GetComponent<SpriteRenderer>().color = gameManager.activeColor;
    }

    public void UnitSetInactive()
    {
        unitActive = false;
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

    public void TakeDamage(int damage)
    {
        health -= damage;
        healthBarUI.SetActive(true);
    }
}
