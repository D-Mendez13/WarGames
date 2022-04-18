using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName ="New Dynamic Tile Bank", menuName ="ScriptableObjects/Dynamic Tile Bank")]
public class DynamicTileBank : ScriptableObject
{
    public Tile highlightTile;
    public Tile moveTile;
    public Tile occupiedMoveTile;
    public Tile selectedUnitTile;
    public Tile attackTile;
}
