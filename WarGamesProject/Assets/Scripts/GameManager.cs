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

public enum TeamTurn
{
    Blue_Turn,
    Red_Turn
}

public class GameManager : MonoBehaviour
{
    public TeamTurn currentTurn;
    public GameState gameState;

    public Color activeColor = new Color(1, 1, 1, 1);
    public Color inactiveColor = new Color(0.3f, 0.3f, 0.3f, 1);

    [Header("=== UI Objects ===")]
    public GameObject actionPanel;
    public GameObject endTurnButton;
    private GameObject selectedUnit;
    private Vector3 selectedUnitStartingPos;

    [Header("=== Tilemap Layers ===")]
    public Tilemap tilemap; //The main tilemap with Grass tiles, Forest tiles, and Rock tiles
    public Tilemap highlightMap; //This tilemap is only used for highlighting the tile the mouse is over.
    public Tilemap moveTilemap; //Places a unique highlight that shows the player what tile their unit can move on.
    [Header("=== Tiles ===")]
    public Tile highlight; //The highlight tile that will be placed on the highlightMap layer.
    public Tile moveTile; //The tile that will be placed on the moveTilemap layer.
    public Tile occupiedMoveTile;
    public Tile selectedUnitTile;

    //These are the tiles with different move costs and other information stored in them.
    [Header("=== Tile Types ===")]
    public TileType grassTile;
    public TileType rockTile;
    public TileType forestTile;

    private Vector3Int location; //The location of the tile that was clicked by the player.
    private float unitOffset = 0.5f; //When moving a player on a tile, add this to their x and y positions so they are centered on the tile.

    void Start()
    {
        gameState = GameState.SelectingUnit;
    }

    private void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        location = tilemap.WorldToCell(mousePosition);

        //This highlights the tile the mouse is over.
        highlightMap.ClearAllTiles();
        if (gameState != GameState.Menu && highlightMap.GetTile(location) == null && tilemap.GetTile(location) != null)
        {
            highlightMap.SetTile(location, highlight);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if(gameState == GameState.MovingUnit)
            {
                if (moveTilemap.GetTile<Tile>(location) == moveTile && GetSelectedUnit() != null)
                {
                    GetSelectedUnit().GetComponent<Transform>().position = new Vector2(location.x + unitOffset, location.y + unitOffset);
                    EnableActionPanel();
                }
                
            }
        }

        //Right click will cancel an go back to the previous state.
        if (Input.GetMouseButtonDown(1))
        {
            if(gameState == GameState.Menu)
            {
                endTurnButton.SetActive(false);
                gameState = GameState.SelectingUnit;
            }
            else if(gameState == GameState.SelectingUnit)
            {
                endTurnButton.SetActive(true);
                gameState = GameState.Menu;
            }
            else if(gameState == GameState.MovingUnit)
            {
                moveTilemap.ClearAllTiles();
                gameState = GameState.SelectingUnit;
            }
            else if(gameState == GameState.UnitAction)
            {
                //move unit back to starting positoin
                selectedUnit.GetComponent<Transform>().position = selectedUnitStartingPos;
                moveTilemap.ClearAllTiles();
                DisableActionPanel();
                gameState = GameState.SelectingUnit;
            }
        }
    }
    //ACTION PANEL ---------------------------------------------------------------------------
    //This section is the code for the action panel that pops up after a unit moves
    public void EnableActionPanel()
    {
        actionPanel.SetActive(true);
        gameState = GameState.UnitAction;
    }

    public void DisableActionPanel()
    {
        actionPanel.SetActive(false);
    }

    public void AttackButton()
    {
        //Attack logic here
    }

    public void WaitButton()
    {
        selectedUnit.GetComponent<Unit>().UnitSetInactive();
        DisableActionPanel();
        moveTilemap.ClearAllTiles();
        gameState=GameState.SelectingUnit;
    }
    //----------------------------------------------------------------------------------------

    public void EndTurnButton()
    {
        if(currentTurn == TeamTurn.Blue_Turn)
        {
            
        }
    }

    public void SetSelectedUnit(GameObject unit, Vector3 startingPosition)
    {
        selectedUnit = unit;
        selectedUnitStartingPos = startingPosition;
        Debug.Log($"Selected unit: {selectedUnit.name}");
    }

    public GameObject GetSelectedUnit()
    {
        return selectedUnit;
    }

    /*
     * Triggered from the Unit.cs class
     * Will check which tiles the unit is able to move on.
     */
    public void FindMoveableTiles(UnitType unit, Vector3 unitPosition)
    {
        moveTilemap.ClearAllTiles();
        Vector3Int startPos = new Vector3Int((int)unitPosition.x, (int)unitPosition.y, (int)unitPosition.z);
        HighlightMovableTiles(startPos, unit, unit.moveAmount);

        moveTilemap.SetTile(startPos, selectedUnitTile);
        gameState = GameState.MovingUnit;
    }

    /*
     * This will check the four adjacent tiles to see if the unit can move on it.
     * If it CAN, it will set a moveTile on that position for the player to later click on.
     */
    void HighlightMovableTiles(Vector3Int currentTilePosition, UnitType unit, int remaningMoves)
    {
        int[] posX = { -1, 0, 0, 1 };
        int[] posY = { 0, 1, -1, 0 };
        for (int x = 0; x < posX.Length; x++)
        {
            Vector3Int nextTilePosition = new Vector3Int(currentTilePosition.x + posX[x], currentTilePosition.y + posY[x], currentTilePosition.z);
            if (tilemap.GetTile<Tile>(nextTilePosition) != null && CanMove(remaningMoves, GetTileType(nextTilePosition), unit))
            {
                if (UnitOnTile(nextTilePosition) == false)
                {
                    moveTilemap.SetTile(nextTilePosition, moveTile);
                }
                else
                {
                    moveTilemap.SetTile(nextTilePosition, occupiedMoveTile);
                }
                HighlightMovableTiles(nextTilePosition, unit, remaningMoves - GetTileType(nextTilePosition).moveCost);
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
        if(Physics2D.OverlapBox(new Vector2(position.x+unitOffset,position.y+unitOffset),new Vector2(0.1f,0.1f),0.0f))
        {
            return true;
        }
        else
        {
            return false;
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
}