using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapTileManager : MonoBehaviour
{
    public Tilemap tilemap;
    public Tile grass;
    public TileType grassTile;
    public Tile rock;
    public TileType rockTile;
    public Tile forest;
    public TileType forestTile;
    private Vector3Int location;

    private void Start()
    {
        
    }
    private void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        location = tilemap.WorldToCell(mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if(tilemap.GetTile<Tile>(location) == grass)
            {
                Debug.Log($"Grass Tile at Location: {location}");
            }
            else if(tilemap.GetTile<Tile>(location) == rock)
            {
                Debug.Log($"Rock Tile at Location: {location}");
            }
            else if(tilemap.GetTile<Tile>(location) == forest)
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
