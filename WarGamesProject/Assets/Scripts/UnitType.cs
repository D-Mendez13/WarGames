using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Unit Type",menuName ="ScriptableObjects/Unit")]
public class UnitType : ScriptableObject
{
    public string unitName;
    public int moveAmount;

    public int maxHealth;
    public int attack;
    public int defense;
    public int attackRange;
    public Sprite blueInactiveImage;
    public Sprite redInactiveImage;
}
