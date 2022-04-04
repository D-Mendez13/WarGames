using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static TeamTurn currentTurn;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}

public enum GameState
{
    Menu,
    PlacingUnits,
    InGame
}

public enum TeamTurn
{
    Blue_Turn,
    Red_Turn
}
