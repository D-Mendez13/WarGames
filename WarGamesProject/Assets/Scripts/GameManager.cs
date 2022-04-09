using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static TeamTurn currentTurn;
    public static Color activeColor = new Color(1, 1, 1, 1);
    public static Color inactiveColor = new Color(0.3f, 0.3f, 0.3f, 1);
    private static GameObject selectedUnit;
    private static GameState gameState;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public static void SetSelectedUnit(GameObject unit)
    {
        selectedUnit = unit;
        Debug.Log($"Selected unit: {selectedUnit.name}");
    }

    public static GameObject GetSelectedUnit()
    {
        return selectedUnit;
    }
}

public enum GameState
{
    Menu,
    PlacingUnits,
    SelectingUnit,
    MovingUnit,
    SelectingTarget
}

public enum TeamTurn
{
    Blue_Turn,
    Red_Turn
}
