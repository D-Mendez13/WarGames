using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapTileManager : MonoBehaviour
{
    public Tilemap tilemap; //The main tilemap with Grass tiles, Forest tiles, and Rock tiles
    public Tilemap highlightMap; //This tilemap is only used for highlighting the tile the mouse is over.
    public Tilemap moveTilemap; //Places a unique highlight that shows the player what tile their unit can move on.
    public Tile highlight; //The highlight tile that will be placed on the highlightMap layer.
    public Tile moveTile; //The tile that will be placed on the moveTilemap layer.

    //These are the tiles with different move costs and other information stored in them.
    public TileType grassTile;
    public TileType rockTile;
    public TileType forestTile;

    private Vector3Int location; //The location of the tile that was clicked by the player.
    private float unitOffset = 0.5f; //When moving a player on a tile, add this to their x and y positions so they are centered on the tile.

    private void Start()
    {
        
    }
    private void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        location = tilemap.WorldToCell(mousePosition);

        //This highlights the tile the mouse is over.
        highlightMap.ClearAllTiles();
        if (highlightMap.GetTile(location) == null && tilemap.GetTile(location) != null)
        {
            highlightMap.SetTile(location, highlight);
        }

        if (Input.GetMouseButtonDown(0))
        {
            //Checks if a move tile is clicked then moves the selected unit to that position
            if(moveTilemap.GetTile<Tile>(location) != null)
            {
                GameManager.GetSelectedUnit().GetComponent<Transform>().position = new Vector2(location.x + unitOffset, location.y + unitOffset);
                moveTilemap.ClearAllTiles();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            moveTilemap.ClearAllTiles();
        }
    }

    /*
     * Triggered from the Unit.cs class
     * Will check which tiles the unit is able to move on.
     */
    public void FindMoveableTiles(UnitType unit, Vector3 unitPosition)
    {
        Vector3Int startPos = new Vector3Int((int)unitPosition.x, (int)unitPosition.y, (int)unitPosition.z);
        HighlightMovableTiles(startPos,unit,unit.moveAmount);

        moveTilemap.SetTile(startPos, null); //Removes highlight on selected unit.
    }

    /*
     * This will check the four adjacent tiles to see if the unit can move on it.
     * If it CAN, it will set a moveTile on that position for the player to later click on.
     */
    void HighlightMovableTiles(Vector3Int currentTilePosition, UnitType unit, int remaningMoves)
    {
        int[] posX = {-1,0,0,1};
        int[] posY = {0,1,-1,0};
        for(int x = 0; x < posX.Length; x++)
        {
            Vector3Int nextTilePosition = new Vector3Int(currentTilePosition.x + posX[x], currentTilePosition.y + posY[x], currentTilePosition.z);
            if (tilemap.GetTile<Tile>(nextTilePosition) != null)
            {
                if (CanMove(remaningMoves, GetTileType(nextTilePosition), unit))
                {
                    moveTilemap.SetTile(nextTilePosition, moveTile);
                    HighlightMovableTiles(nextTilePosition,unit,remaningMoves-GetTileType(nextTilePosition).moveCost);
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
