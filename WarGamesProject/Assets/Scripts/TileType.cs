using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName ="New Tile Type",menuName ="ScriptableObjects/TileType")]
public class TileType : ScriptableObject
{
    public string tileName;
    public int moveCost;
    public bool blockCavalry;
    public Tile tile;
}
