using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapTileManager : MonoBehaviour
{
    public Tilemap tilemap;
    public Tilemap highlightMap;
    public Tile highlight;
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

    public static void FindMoveableTiles(UnitType unit)
    {
        //Check each direction if the unit can walk onto that tile. Also check the movement cost of the tile.
        //Place a highligh object on those tiles while that unit is selected
        //Then remove the selectable tiles that are already occupied by other units.
    }

}
