using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public enum GameState
{
    Menu,
    PlacingUnits,
    SelectingUnit,
    MovingUnit,
    UnitAction,
    SelectingTarget
}

public enum Team
{
    None,
    Blue,
    Red
}

public class GameManager : MonoBehaviour
{
    public Team currentTurn;
    public GameState gameState;
    private GameState updateGameState;

    public Color activeColor = new Color(1, 1, 1, 1);
    public Color inactiveColor = new Color(0.3f, 0.3f, 0.3f, 1);

    [Header("UI Objects:")]
    public GameObject actionPanel;
    public GameObject endTurnButton;

    [Header("Tilemap Layers:")]
    public Tilemap tilemap; //The main tilemap with Grass tiles, Forest tiles, and Rock tiles
    public Tilemap highlightMap; //This tilemap is only used for highlighting the tile the mouse is over.
    public Tilemap dynamicTilemapTopLayer; //Places a unique highlight that shows the player what tile their unit can move on.
    public Tilemap dynamicTilemapBottomLayer;

    [Header("Dynamic Tiles:")]
    public DynamicTileBank dynamicTiles;

    //These are the tiles with different move costs and other information stored in them.
    [Header("Tile Types:")]
    public TileType grassTile;
    public TileType rockTile;
    public TileType forestTile;

    private GameObject selectedUnit;
    private Vector3 selectedUnitStartingPos;
    private Transform selectedUnitPosition;
    private GameObject targetUnit; //The unit that is being attack will be stored here.
    private Vector3Int location; //The location of the tile that was clicked by the player.
    private float unitOffset = 0.5f; //When moving a player on a tile, add this to their x and y positions so they are centered on the tile.
    private float apOffset = 1.0f;
    private int[] posX = { -1, 0, 0, 1 };
    private int[] posY = { 0, 1, -1, 0 };
    private List<GameObject> inactiveUnits = new List<GameObject>();

    void Start()
    {
        gameState = GameState.SelectingUnit;
        currentTurn = Team.Blue;
    }

    private void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        location = tilemap.WorldToCell(mousePosition);

        //This highlights the tile the mouse is over.
        highlightMap.ClearAllTiles();
        if (gameState == GameState.SelectingUnit || gameState == GameState.MovingUnit || gameState == GameState.SelectingTarget && highlightMap.GetTile(location) == null && tilemap.GetTile(location) != null)
        {
            highlightMap.SetTile(location, dynamicTiles.highlightTile);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (gameState == GameState.SelectingTarget)
            {
                if(dynamicTilemapBottomLayer.GetTile<Tile>(location) == dynamicTiles.attackTile)
                {
                    SetTargetUnit(location);
                    Combat();
                    if(selectedUnit != null)
                    {
                        selectedUnit.GetComponent<Unit>().UnitSetInactive();
                    }
                    gameState = GameState.SelectingUnit;
                    dynamicTilemapBottomLayer.ClearAllTiles();
                    dynamicTilemapTopLayer.ClearAllTiles();
                }
            }

            if (gameState == GameState.MovingUnit)
            {
                if (dynamicTilemapBottomLayer.GetTile<Tile>(location) == dynamicTiles.moveTile || dynamicTilemapTopLayer.GetTile<Tile>(location) == dynamicTiles.selectedUnitTile)
                {
                    selectedUnitPosition.position = new Vector2(location.x + unitOffset, location.y + unitOffset);
                    EnableActionPanel();
                }
                
            }
        }

        IsUnitMoving();

        //Right click will cancel an go back to the previous state.
        if (Input.GetMouseButtonDown(1))
        {
            switch (gameState)
            {
                case GameState.Menu:
                    endTurnButton.SetActive(false);
                    gameState = GameState.SelectingUnit;
                    break;
                case GameState.PlacingUnits:
                    break;
                case GameState.SelectingUnit:
                    endTurnButton.SetActive(true);
                    gameState = GameState.Menu;
                    break;
                case GameState.MovingUnit:
                    ReturnToSelectingUnit();
                    break;
                case GameState.UnitAction:
                    ReturnToSelectingUnit();
                    break;
                case GameState.SelectingTarget:
                    ReturnToSelectingUnit();
                    break;
                default:
                    break;
            }
        }
    }

    void ReturnToSelectingUnit()
    {
        //move unit back to starting positoin
        selectedUnitPosition.position = selectedUnitStartingPos;
        dynamicTilemapTopLayer.ClearAllTiles();
        dynamicTilemapBottomLayer.ClearAllTiles();
        DisableActionPanel();
        gameState = GameState.SelectingUnit;
    }
    //ACTION PANEL ---------------------------------------------------------------------------
    //This section is the code for the action panel that pops up after a unit moves
    public void EnableActionPanel()
    {
        if (tilemap.GetTile<Tile>(new Vector3Int((int)selectedUnitPosition.position.x - 1, (int)selectedUnitPosition.position.y, (int)selectedUnitPosition.position.z)) == null)
        {
            actionPanel.GetComponent<Transform>().position = new Vector3(selectedUnitPosition.position.x + apOffset, selectedUnitPosition.position.y, selectedUnitPosition.position.z);
        }
        else
        {
            actionPanel.GetComponent<Transform>().position = new Vector3(selectedUnitPosition.position.x - apOffset, selectedUnitPosition.position.y, selectedUnitPosition.position.z);
        }
        actionPanel.SetActive(true);
        gameState = GameState.UnitAction;
    }

    public void DisableActionPanel()
    {
        actionPanel.SetActive(false);
    }

    public void AttackButton()
    {
        DisableActionPanel();
        FindAttackableTiles(selectedUnit.GetComponent<Unit>().unitType, selectedUnitPosition.position);
        gameState = GameState.SelectingTarget;
    }

    public void WaitButton()
    {
        selectedUnit.GetComponent<Unit>().UnitSetInactive();
        DisableActionPanel();
        dynamicTilemapTopLayer.ClearAllTiles();
        dynamicTilemapBottomLayer.ClearAllTiles();
        gameState=GameState.SelectingUnit;
    }
    //----------------------------------------------------------------------------------------

    public void EndTurnButton()
    {
        if(currentTurn == Team.Blue)
        {
            currentTurn = Team.Red;
        }
        else
        {
            currentTurn = Team.Blue;
        }

        for (int i = 0; i < inactiveUnits.Count; i++)
        {
            inactiveUnits[i].GetComponent<Unit>().UnitSetActive();
        }
        inactiveUnits.Clear();
        endTurnButton.SetActive(false);
    }

    public void SetSelectedUnit(GameObject unit, Vector3 startingPosition)
    {
        selectedUnit = unit;
        selectedUnitStartingPos = startingPosition;
        selectedUnitPosition = selectedUnit.GetComponent<Transform>();
        Debug.Log($"Selected unit: {selectedUnit.name}");
    }

    public void SetUnitAsTarget(GameObject unit)
    {
        targetUnit = unit;
    }

    public GameObject GetSelectedUnit()
    {
        return selectedUnit;
    }

    public void Combat()
    {
        Unit attacker = selectedUnit.GetComponent<Unit>();
        Unit defender = targetUnit.GetComponent<Unit>();
        defender.TakeDamage(attacker.unitType.attack);
        if(defender.health <= 0)
        {
            Destroy(targetUnit);
        }
        else
        {
            if(defender.unitType.attackRange == attacker.unitType.attackRange)
            {
                attacker.TakeDamage(defender.unitType.attack / 2 );
            }
        }
    }

    /*
     * Triggered from the Unit.cs class
     * Will check which tiles the unit is able to move on.
     */
    public void FindMoveableTiles(UnitType unit, Vector3 unitPosition)
    {
        dynamicTilemapBottomLayer.ClearAllTiles();
        Vector3Int startPos = new Vector3Int((int)unitPosition.x, (int)unitPosition.y, (int)unitPosition.z);
        HighlightMovableTiles(startPos, unit, unit.moveAmount);

        dynamicTilemapTopLayer.SetTile(startPos, dynamicTiles.selectedUnitTile);
        SetUnitMoving();
    }

    /*
     * This will check the four adjacent tiles to see if the unit can move on it.
     * If it CAN, it will set a moveTile on that position for the player to later click on.
     */
    void HighlightMovableTiles(Vector3Int currentTilePosition, UnitType unit, int remaningMoves)
    {
        for (int x = 0; x < posX.Length; x++)
        {
            Vector3Int nextTilePosition = new Vector3Int(currentTilePosition.x + posX[x], currentTilePosition.y + posY[x], currentTilePosition.z);
            if (tilemap.GetTile<Tile>(nextTilePosition) != null && CanMove(remaningMoves, GetTileType(nextTilePosition), unit))
            {
                if (UnitOnTile(nextTilePosition))
                {
                    dynamicTilemapBottomLayer.SetTile(nextTilePosition, dynamicTiles.occupiedMoveTile);
                }
                else
                {
                    dynamicTilemapBottomLayer.SetTile(nextTilePosition, dynamicTiles.moveTile);
                }

                if(dynamicTilemapBottomLayer.GetTile<Tile>(nextTilePosition) == dynamicTiles.occupiedMoveTile && UnitTeamColor(nextTilePosition) != selectedUnit.GetComponent<Unit>().teamColor)
                {
                    //If enemy is detected on the checked tile, pass in 0 to remaning moves for the next function call so unit can not pass through enemies.
                    HighlightMovableTiles(nextTilePosition, unit, 0);
                }
                else
                {
                    HighlightMovableTiles(nextTilePosition, unit, remaningMoves - GetTileType(nextTilePosition).moveCost);
                }
            }
        }
    }

    public void FindAttackableTiles(UnitType unit, Vector3 unitPosition)
    {
        dynamicTilemapTopLayer.ClearAllTiles();
        dynamicTilemapBottomLayer.ClearAllTiles();
        Vector3Int startPos = new Vector3Int((int)unitPosition.x, (int)unitPosition.y, (int)unitPosition.z);
        HighlightAttackableTiles(startPos, unit, unit.attackRange);

        dynamicTilemapTopLayer.SetTile(startPos, dynamicTiles.selectedUnitTile);
        dynamicTilemapBottomLayer.SetTile(startPos, null);
    }

    void HighlightAttackableTiles(Vector3Int currentTilePosition, UnitType unit, int remaningAttackRange)
    {
        for (int x = 0; x < posX.Length; x++)
        {
            Vector3Int nextTilePosition = new Vector3Int(currentTilePosition.x + posX[x], currentTilePosition.y + posY[x], currentTilePosition.z);
            if (tilemap.GetTile<Tile>(nextTilePosition) != null && remaningAttackRange > 0)
            {
                if(remaningAttackRange != 1)
                {
                    HighlightAttackableTiles(nextTilePosition, unit, remaningAttackRange - 1);
                }
                else
                {
                    if(UnitOnTile(nextTilePosition) && UnitTeamColor(nextTilePosition) != selectedUnit.GetComponent<Unit>().teamColor)
                    {
                        dynamicTilemapBottomLayer.SetTile(nextTilePosition, dynamicTiles.attackTile);
                    }
                    else
                    {
                        dynamicTilemapBottomLayer.SetTile(nextTilePosition, dynamicTiles.occupiedMoveTile);
                    }
                    
                }
            }
        }
    }

    /*
     * Returns true if the player has enough move points to move on that tile.
     * It will first check if the blockCavalry flag is active in that TileType
     */
    bool CanMove(int remaningMoves, TileType tile, UnitType unit)
    {
        if (tile.blockCavalry && unit.unitName == "Cavalry")
        {
            return false;
        }
        else
        {
            if (remaningMoves >= tile.moveCost)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /*
     * Will check if a unit is at the position
     */
    bool UnitOnTile(Vector3Int position)
    {
        Collider2D unit = Physics2D.OverlapBox(new Vector2(position.x + unitOffset, position.y + unitOffset), new Vector2(0.1f, 0.1f), 0.0f);

        if(unit != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    Team UnitTeamColor(Vector3Int position)
    {
        Collider2D unit = Physics2D.OverlapBox(new Vector2(position.x + unitOffset, position.y + unitOffset), new Vector2(0.1f, 0.1f), 0.0f);

        if(unit != null)
        {
            return unit.GetComponent<Unit>().teamColor;
        }
        else
        {
            return Team.None;
        }
    }

    void SetTargetUnit(Vector3 position)
    {
        Collider2D unit = Physics2D.OverlapBox(new Vector2(position.x + unitOffset, position.y + unitOffset), new Vector2(0.1f, 0.1f), 0.0f);

        if(unit != null)
        {
            targetUnit = unit.gameObject;
            Debug.Log($"Selected {unit.name} as a target.");
        }
    }

    /*
     * Returns what kind of TileType is at the passed in coordinates.
     */
    TileType GetTileType(Vector3Int position)
    {
        if (tilemap.GetTile<Tile>(position) == forestTile.tile)
        {
            return forestTile;
        }
        else if (tilemap.GetTile<Tile>(position) == rockTile.tile)
        {
            return rockTile;
        }
        else
        {
            return grassTile;
        }
    }

    public void addToInactiveList(GameObject g)
    {
        inactiveUnits.Add(g);
    }

    public void IsUnitMoving()
    {
        if(updateGameState == GameState.MovingUnit)
        {
            gameState = GameState.MovingUnit;
            updateGameState = GameState.Menu;
        }
    }

    public void SetUnitMoving()
    {
        updateGameState = GameState.MovingUnit;
    }
}