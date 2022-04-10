using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TeamTurn currentTurn;
    public Color activeColor = new Color(1, 1, 1, 1);
    public Color inactiveColor = new Color(0.3f, 0.3f, 0.3f, 1);
    public GameObject actionPanel;
    private GameObject selectedUnit;
    private GameState gameState;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void EnableActionPanel()
    {
        actionPanel.SetActive(true);
    }

    public void DisableActionPanel()
    {
        actionPanel.SetActive(false);
    }

    public void WaitButton()
    {
        selectedUnit.GetComponent<Unit>().UnitSetInactive();
        DisableActionPanel();

    }

    public void SetSelectedUnit(GameObject unit)
    {
        selectedUnit = unit;
        Debug.Log($"Selected unit: {selectedUnit.name}");
    }

    public GameObject GetSelectedUnit()
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
