using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapTileManager : MonoBehaviour
{
    public Tilemap tilemap;
    public Tilemap highlightMap;
    public Tilemap moveTilemap;
    public Tile highlight;
    public Tile moveTile;
    public TileType grassTile;
    public TileType rockTile;
    public TileType forestTile;
    private Vector3Int location;

    private void Start()
    {
        
    }
    private void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        location = tilemap.WorldToCell(mousePosition);

        highlightMap.ClearAllTiles();
        if (highlightMap.GetTile(location) == null && tilemap.GetTile(location) != null)
        {
            highlightMap.SetTile(location, highlight);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if(tilemap.GetTile<Tile>(location) == grassTile.tile)
            {
                Debug.Log($"Grass Tile at Location: {location}");
            }
            else if(tilemap.GetTile<Tile>(location) == rockTile.tile)
            {
                Debug.Log($"Rock Tile at Location: {location}");
            }
            else if(tilemap.GetTile<Tile>(location) == forestTile.tile)
            {
                Debug.Log($"Forest Tile at Location: {location}");
            }
        }
    }

    public void FindMoveableTiles(UnitType unit, Vector3 unitPosition)
    {
        //Check each direction if the unit can walk onto that tile. Also check the movement cost of the tile.
        //Place a highligh object on those tiles while that unit is selected
        //Then remove the selectable tiles that are already occupied by other units.
        Vector3Int startPos = new Vector3Int((int)unitPosition.x, (int)unitPosition.y, (int)unitPosition.z);
        Vector3Int positiveY = startPos;
        Vector3Int negativeY = startPos;
        Vector3Int positiveX = startPos;
        Vector3Int negativeX = startPos;
        Debug.Log("Move tile triggered");
        for(int i = unit.moveAmount; i > 0; i--)
        {
            //+y
            HighlightMovableTiles(positiveY, unit, i);
            positiveY = new Vector3Int(positiveY.x, positiveY.y + 1, positiveY.z);
            //+x
            HighlightMovableTiles(positiveX, unit, i);
            positiveX = new Vector3Int(positiveX.x + 1, positiveX.y, positiveX.z);
            //-y
            HighlightMovableTiles(negativeY, unit, i);
            negativeY = new Vector3Int(negativeY.x, negativeY.y - 1, negativeY.z);
            //-x
            HighlightMovableTiles(negativeX, unit, i);
            negativeX = new Vector3Int(negativeX.x - 1, negativeX.y, negativeX.z);
        }
    }
    void HighlightMovableTiles(Vector3Int currentTilePosition, UnitType unit , int remaningMoves)
    {
        Debug.Log("Highlighting!");
        int[] posX = {-1,0,0,1};
        int[] posY = {0,1,-1,0};
        for(int x = 0; x < 4; x++)
        {
            for(int y = 0; y < 4; y++)
            {
                Vector3Int nextTileLocation = new Vector3Int(currentTilePosition.x + posX[x], currentTilePosition.y + posY[y], currentTilePosition.z);
                TileType nextTileType;
                if(tilemap.GetTile<Tile>(nextTileLocation) != null)
                {
                    if(tilemap.GetTile<Tile>(nextTileLocation) == grassTile.tile)
                    {
                        nextTileType = grassTile;
                    }
                    else if(tilemap.GetTile<Tile>(nextTileLocation) == forestTile.tile)
                    {
                        nextTileType = forestTile;
                    }
                    else
                    {
                        nextTileType = rockTile;
                    }

                    if (MoveCostCheck(remaningMoves, nextTileType, unit))
                    {
                        moveTilemap.SetTile(nextTileLocation, moveTile);
                    }
                }
            }
        }
    }
    bool MoveCostCheck(int playerMove, TileType tile, UnitType unit)
    {
        Debug.Log("Checking move cost...");
        if (!tile.notPassableToAll)
        {
            if (tile.blockCavalry && unit.unitName == "Cavalry")
            {
                return false;
            }
            else
            {
                if(playerMove >= tile.moveCost)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        else
        {
            return false;
        }
    }
}
