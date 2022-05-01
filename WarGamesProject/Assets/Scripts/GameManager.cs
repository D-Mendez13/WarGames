using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using TMPro;

public enum GameState
{
    Menu,
    StartingTurn,
    Waiting,
    SelectingUnit,
    MovingUnit,
    UnitWalking,
    UnitAction,
    SelectingTarget,
    UnitAttacking,
    EndingTurn,
    GameOver
}

public enum Team
{
    None,
    Blue,
    Red
}

public class GameManager : MonoBehaviour
{
    public Team currentTurn;
    public GameState gameState;
    private GameState updateGameState;

    public Color activeColor = new Color(1, 1, 1, 1);
    public Color inactiveColor = new Color(0.3f, 0.3f, 0.3f, 1);

    [Header("AI:")]
    public static bool enableBlueAI = false;
    public static bool enableRedAI = true;

    [Header("UI Objects:")]
    public GameObject actionPanel;
    public GameObject endTurnButton;
    public GameObject blueWinsText;
    public GameObject redWinsText;
    public GameObject tieText;
    public GameObject blueTurnPanel;
    public GameObject redTurnPanel;

    [Header("Tilemap Layers:")]
    public Tilemap tilemap; //The main tilemap with Grass tiles, Forest tiles, and Rock tiles
    public Tilemap highlightMap; //This tilemap is only used for highlighting the tile the mouse is over.
    public Tilemap dynamicTilemapTopLayer; //Places a unique highlight that shows the player what tile their unit can move on.
    public Tilemap dynamicTilemapBottomLayer;

    [Header("Dynamic Tiles:")]
    public DynamicTileBank dynamicTiles;

    //These are the tiles with different move costs and other information stored in them.
    [Header("Tile Types:")]
    public TileType grassTile;
    public TileType rockTile;
    public TileType forestTile;
    public TileType lavaTile;

    private GameObject selectedUnit;
    private Vector3 selectedUnitStartingPos;
    private Vector3 movePoint;
    private float moveSpeed = 5.0f;
    private Transform selectedUnitPosition;
    private GameObject targetUnit; //The unit that is being attack will be stored here.
    private Vector3Int location; //The location of the tile that was clicked by the player.
    private float unitOffset = 0.5f; //When moving a player on a tile, add this to their x and y positions so they are centered on the tile.
    private float apOffset = 1.0f;
    private int[] posX = { -1, 0, 0, 1 };
    private int[] posY = { 0, 1, -1, 0 };
    private int[] posX_2 = { -2, 0, 0, 2 };
    private int[] posY_2 = { 0, 2, -2, 0 };
    private int blueUnitCount;
    private int redUnitCount;
    private int AIUnitIndex; //Index used to have the AI go through it's own list of active units to give commands.
    private bool AI_InRange;
    private List<GameObject> PotentialTargets = new List<GameObject>(); //A list of enemy units in range for the AI.
    private List<GameObject> BlueUnitList = new List<GameObject>();
    private List<GameObject> RedUnitList = new List<GameObject>();
    private List<Unit> inactiveUnits = new List<Unit>();

    void Start()
    {
        gameState = GameState.SelectingUnit;
        currentTurn = Team.Blue;
        AIUnitIndex = 0;
    }

    private void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        location = tilemap.WorldToCell(mousePosition);

        //This highlights the tile the mouse is over.
        highlightMap.ClearAllTiles();
        if (gameState == GameState.SelectingUnit || gameState == GameState.MovingUnit || gameState == GameState.SelectingTarget && highlightMap.GetTile(location) == null && tilemap.GetTile(location) != null)
        {
            highlightMap.SetTile(location, dynamicTiles.highlightTile);
        }

        if(gameState == GameState.StartingTurn)
        {
            if(currentTurn == Team.Blue)
            {
                for(int i = 0; i < BlueUnitList.Count; i++)
                {
                    Vector3 uniPos = BlueUnitList[i].GetComponent<Transform>().position;
                    Vector3Int unitPos2 = new Vector3Int((int)uniPos.x, (int)uniPos.y, (int)uniPos.z);
                    if (GetTileType(unitPos2) == lavaTile)
                    {
                        BlueUnitList[i].GetComponent<Unit>().TakeDamage(5);
                    }
                }
            }
            else
            {
                for (int i = 0; i < RedUnitList.Count; i++)
                {
                    Vector3 uniPos = RedUnitList[i].GetComponent<Transform>().position;
                    Vector3Int unitPos2 = new Vector3Int((int)uniPos.x, (int)uniPos.y, (int)uniPos.z);
                    if (GetTileType(unitPos2) == lavaTile)
                    {
                        RedUnitList[i].GetComponent<Unit>().TakeDamage(5);
                    }
                }
            }
            AIUnitIndex = 0;
            gameState = GameState.SelectingUnit;
        }

        if(gameState == GameState.GameOver)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
        else
        {
            //Check input for player or AI
            if (enableBlueAI && currentTurn == Team.Blue && gameState != GameState.GameOver)
            {
                AI_Input(BlueUnitList);
            }
            else if (enableRedAI && currentTurn == Team.Red && gameState != GameState.GameOver)
            {
                AI_Input(RedUnitList);
            }
            else
            {
                PlayerInput(location);
            }
        }

        if (gameState == GameState.UnitWalking)
        {
            selectedUnitPosition.position = Vector3.MoveTowards(selectedUnitPosition.position, new Vector3(movePoint.x, movePoint.y, 0f), moveSpeed * Time.deltaTime);
            if (selectedUnitPosition.position == movePoint)
            {
                EnableActionPanel();
            }
        }
        if(gameState == GameState.UnitAttacking)
        {
            if(selectedUnit.GetComponent<Unit>().unitActive == false || selectedUnit.activeSelf == false)
            {
                gameState = GameState.SelectingUnit;
            }
        }
    }

    public void setAI(string x)
    {
        if (x == "0")
        {
            enableRedAI = false;
            enableBlueAI = true;
        }
        else if(x == "1")
        {
            enableRedAI = true;
            enableBlueAI = false;
        }
        else if (x == "2")
        {
            enableRedAI = false;
            enableBlueAI = false;
        }
    }

    void PlayerInput(Vector3Int location)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (gameState == GameState.SelectingTarget)
            {
                if (dynamicTilemapBottomLayer.GetTile<Tile>(location) == dynamicTiles.attackTile)
                {
                    SetTargetUnit(location);
                    Combat();
                }
            }

            if (gameState == GameState.MovingUnit)
            {
                if (dynamicTilemapBottomLayer.GetTile<Tile>(location) == dynamicTiles.moveTile || dynamicTilemapTopLayer.GetTile<Tile>(location) == dynamicTiles.selectedUnitTile)
                {
                    //selectedUnitPosition.position = new Vector2(location.x + unitOffset, location.y + unitOffset);
                    MoveUnitToTile(location);
                }

            }
        }

        IsUnitMoving();

        //Right click will cancel an go back to the previous state.
        if (Input.GetMouseButtonDown(1))
        {
            switch (gameState)
            {
                case GameState.Menu:
                    endTurnButton.SetActive(false);
                    gameState = GameState.SelectingUnit;
                    break;
                case GameState.StartingTurn:
                    break;
                case GameState.SelectingUnit:
                    endTurnButton.SetActive(true);
                    gameState = GameState.Menu;
                    break;
                case GameState.MovingUnit:
                    ReturnToSelectingUnit();
                    break;
                case GameState.UnitAction:
                    ReturnToSelectingUnit();
                    break;
                case GameState.SelectingTarget:
                    ReturnToSelectingUnit();
                    break;
                default:
                    break;
            }
        }
    }

    /*
     AI steps
     1. Find all of it's active units at the start of the turn.
     2. Select one unit from that list that is not inactive. If none are left active, End Turn.
     3. First check if there is an enemy unit within the move range of the unit.
        3A. If true, Step 4
        3B. If false, check if there is an enemy unit within the range of 2* the move amount.
            3BA. If true, 
            3BB. If false, set unit to Wait (inactive). Then return to step 2.
     4. Find the enemy unit that has the least amount of health and set that as the target.
     5. Move to a tile that is within attacking range to the targeted unit.
     6. Attack the target. Then return to step 2.
    */
    void AI_Input(List<GameObject> unitList)
    {
        if (gameState == GameState.SelectingUnit)
        {
            if (AIUnitIndex < unitList.Count)
            {
                while(unitList[AIUnitIndex].activeSelf == false)
                {
                    AIUnitIndex++;
                    if(AIUnitIndex >= unitList.Count)
                    {
                        EndTurnButton();
                        return;
                    }
                }

                if (unitList[AIUnitIndex].GetComponent<Unit>().unitActive)
                {
                    SetSelectedUnit(unitList[AIUnitIndex], unitList[AIUnitIndex].GetComponent<Transform>().position);
                    PotentialTargets.Clear();
                    EnemyRadar(selectedUnit.GetComponent<Unit>().unitType.moveAmount, new Vector3Int((int)selectedUnitStartingPos.x, (int)selectedUnitStartingPos.y, (int)selectedUnitStartingPos.z));
                    SelectTarget();
                    if (targetUnit != null)
                    {
                        FindMoveableTiles(unitList[AIUnitIndex].GetComponent<Unit>().unitType, selectedUnitStartingPos);
                    }
                    else
                    {
                        PotentialTargets.Clear();
                        EnemyRadar(selectedUnit.GetComponent<Unit>().unitType.moveAmount + 4, new Vector3Int((int)selectedUnitStartingPos.x, (int)selectedUnitStartingPos.y, (int)selectedUnitStartingPos.z));
                        SelectTarget();
                        if (targetUnit != null)
                        {
                            FindMoveableTiles(unitList[AIUnitIndex].GetComponent<Unit>().unitType, selectedUnitStartingPos);
                        }
                    }
                    gameState = GameState.MovingUnit;
                    AIUnitIndex++;
                }
            }
            else
            {
                EndTurnButton();
            }
        }

        if(gameState == GameState.MovingUnit)
        {
            //If there is a move target, move unit. Else, wait.
            if(targetUnit != null)
            {
                if(movePoint == selectedUnit.GetComponent<Transform>().position)
                {
                    gameState = GameState.UnitAction;
                }
                else
                {
                    MoveUnitToTile(movePoint);
                }
            }
            else
            {
                gameState = GameState.UnitAction;
            }
        }

        if (gameState == GameState.UnitAction)
        {
            //If there is a target, Select Target. Else, wait.
            if(targetUnit != null)
            {
                AttackButton();
            }
            else
            {
                WaitButton();
            }
        }

        if (gameState == GameState.SelectingTarget)
        {
            //If multiple targets, select one with the least amount of health && does the least amount of counter attack damage.
            if(targetUnit != null)
            {
                if (AI_InRange)
                {
                    Combat();
                    AI_InRange = false;
                }
                else
                {
                    WaitButton();
                }
            }
            else
            {
                WaitButton();
            }
        }

    }

    void ReturnToSelectingUnit()
    {
        //move unit back to starting positoin
        selectedUnitPosition.position = selectedUnitStartingPos;
        movePoint = selectedUnitPosition.position;
        dynamicTilemapTopLayer.ClearAllTiles();
        dynamicTilemapBottomLayer.ClearAllTiles();
        DisableActionPanel();
        gameState = GameState.SelectingUnit;
    }
    //ACTION PANEL ---------------------------------------------------------------------------
    //This section is the code for the action panel that pops up after a unit moves
    public void EnableActionPanel()
    {
        if (tilemap.GetTile<Tile>(new Vector3Int((int)movePoint.x - 1, (int)movePoint.y, (int)movePoint.z)) == null)
        {
            actionPanel.GetComponent<Transform>().position = new Vector3(movePoint.x + apOffset, movePoint.y, movePoint.z);
        }
        else
        {
            actionPanel.GetComponent<Transform>().position = new Vector3(movePoint.x - apOffset, movePoint.y, movePoint.z);
        }
        actionPanel.SetActive(true);
        selectedUnit.GetComponent<Animator>().SetBool("walking", false);
        gameState = GameState.UnitAction;
    }

    public void DisableActionPanel()
    {
        actionPanel.SetActive(false);
    }

    public void AttackButton()
    {
        DisableActionPanel();
        FindAttackableTiles(selectedUnit.GetComponent<Unit>().unitType, selectedUnitPosition.position);
        gameState = GameState.SelectingTarget;
        Debug.Log("Attack Button Clicked");
    }

    public void WaitButton()
    {
        if(selectedUnitStartingPos == selectedUnitPosition.position)
        {
            //Heal for waiting in the same spot
            selectedUnit.GetComponent<Unit>().Heal();
        }
        selectedUnit.GetComponent<Unit>().UnitSetInactive();
        DisableActionPanel();
        dynamicTilemapTopLayer.ClearAllTiles();
        dynamicTilemapBottomLayer.ClearAllTiles();
        gameState = GameState.SelectingUnit;
        targetUnit = null;
        Debug.Log("Wait Button Clicked");
    }
    //----------------------------------------------------------------------------------------

    public void EndTurnButton()
    {
        Debug.Log("End Button Clicked");
        gameState = GameState.EndingTurn;
        endTurnButton.SetActive(false);

        if(currentTurn == Team.Blue)
        {
            currentTurn = Team.Red;
            redTurnPanel.SetActive(true);
        }
        else
        {
            currentTurn = Team.Blue;
            blueTurnPanel.SetActive(true);
        }

        for (int i = 0; i < inactiveUnits.Count; i++)
        {
            inactiveUnits[i].UnitSetActive();
        }
        inactiveUnits.Clear();
        IsUnitMoving();
        gameState = GameState.Waiting;
        AIUnitIndex = 0;
        Invoke("TurnPanelOff", 1);
    }

    void TurnPanelOff()
    {
        redTurnPanel.SetActive(false);
        blueTurnPanel.SetActive(false);
        gameState = GameState.StartingTurn;
    }

    public void SetSelectedUnit(GameObject unit, Vector3 startingPosition)
    {
        selectedUnit = unit;
        selectedUnitStartingPos = startingPosition;
        movePoint = startingPosition;
        selectedUnitPosition = selectedUnit.GetComponent<Transform>();
        Debug.Log($"Unit Index: {AIUnitIndex}");
        Debug.Log($"Selected unit: {selectedUnit.name}");
    }

    public void SetUnitAsTarget(GameObject unit)
    {
        targetUnit = unit;
    }

    public GameObject GetSelectedUnit()
    {
        return selectedUnit;
    }

    public void Combat()
    {
        gameState = GameState.UnitAttacking;
        Unit attacker = selectedUnit.GetComponent<Unit>();
        Unit defender = targetUnit.GetComponent<Unit>();

        attacker.animator.SetTrigger("attack");
        defender.TakeDamage(attacker.unitType.attack - defender.unitType.defense);

        if(defender.health > 0 && defender.unitType.attackRange == attacker.unitType.attackRange)
        {
            defender.animator.SetTrigger("attack");
            attacker.TakeDamage((defender.unitType.attack / 2) - attacker.unitType.defense);
        }

        selectedUnit.GetComponent<Unit>().DelayedInactive();
        targetUnit = null;
        dynamicTilemapBottomLayer.ClearAllTiles();
        dynamicTilemapTopLayer.ClearAllTiles();
        GameOverCheck();
    }

    /*
     * Triggered from the Unit.cs class
     * Will check which tiles the unit is able to move on.
     */
    public void FindMoveableTiles(UnitType unit, Vector3 unitPosition)
    {
        dynamicTilemapBottomLayer.ClearAllTiles();
        Vector3Int startPos = new Vector3Int((int)unitPosition.x, (int)unitPosition.y, (int)unitPosition.z);
        movePoint = unitPosition;
        HighlightMovableTiles(startPos, unit, unit.moveAmount, unit.attackRange);

        dynamicTilemapTopLayer.SetTile(startPos, dynamicTiles.selectedUnitTile);
        SetUnitMoving();
    }

    /*
     * This will check the four adjacent tiles to see if the unit can move on it.
     * If it CAN, it will set a moveTile on that position for the player to later click on.
     */
    void HighlightMovableTiles(Vector3Int currentTilePosition, UnitType unit, int remaningMoves, int remaningAttackRange)
    {
        for (int x = 0; x < posX.Length; x++)
        {
            Vector3Int nextTilePosition = new Vector3Int(currentTilePosition.x + posX[x], currentTilePosition.y + posY[x], currentTilePosition.z);
            if (tilemap.GetTile<Tile>(nextTilePosition) != null)
            {
                if(CanMove(remaningMoves, GetTileType(nextTilePosition), unit))
                {
                    if (UnitOnTile(nextTilePosition))
                    {
                        if (UnitTeamColor(nextTilePosition) != selectedUnit.GetComponent<Unit>().teamColor)
                        {
                            dynamicTilemapBottomLayer.SetTile(nextTilePosition, dynamicTiles.attackTile);
                            //For the AI
                            if (targetUnit != null)
                            {
                                if(selectedUnit.GetComponent<Unit>().unitType.attackRange == 1)
                                {
                                    //Attack range of 1
                                    for(int y = 0; y < posY.Length; y++)
                                    {
                                        Vector3Int adjacentTilePos = new Vector3Int(nextTilePosition.x + posX[y], nextTilePosition.y + posY[y], nextTilePosition.z);
                                        if(dynamicTilemapBottomLayer.GetTile<Tile>(adjacentTilePos) == dynamicTiles.moveTile)
                                        {
                                            movePoint = adjacentTilePos;
                                        }
                                    }
                                }
                                else
                                {
                                    //Attack range of 2
                                    for (int y = 0; y < posY_2.Length; y++)
                                    {
                                        Vector3Int adjacentTilePos = new Vector3Int(nextTilePosition.x + posX_2[y], nextTilePosition.y + posY_2[y], nextTilePosition.z);
                                        if (dynamicTilemapBottomLayer.GetTile<Tile>(adjacentTilePos) == dynamicTiles.moveTile)
                                        {
                                            movePoint = adjacentTilePos;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            dynamicTilemapBottomLayer.SetTile(nextTilePosition, dynamicTiles.occupiedMoveTile);
                        }
                    }
                    else
                    {
                        dynamicTilemapBottomLayer.SetTile(nextTilePosition, dynamicTiles.moveTile);
                        if (targetUnit != null)
                        {
                            float dist = Vector3.Distance(nextTilePosition, targetUnit.transform.position);
                            if(dist < Vector3.Distance(movePoint, targetUnit.transform.position))
                            {
                                movePoint = nextTilePosition;
                            }
                        }
                    }

                    if (dynamicTilemapBottomLayer.GetTile<Tile>(nextTilePosition) == dynamicTiles.attackTile)
                    {
                        //If enemy is detected on the checked tile, pass in 0 to remaning moves for the next function call so unit can not pass through enemies.
                        HighlightMovableTiles(nextTilePosition, unit, 0, remaningAttackRange - 1);
                    }
                    else
                    {
                        HighlightMovableTiles(nextTilePosition, unit, remaningMoves - GetTileType(nextTilePosition).moveCost, unit.attackRange);
                    }
                }
                else
                {
                    //If they can't move anymore, that means it's time to highlight attackable tiles in red.
                    if(dynamicTilemapBottomLayer.GetTile<Tile>(nextTilePosition) == null && remaningAttackRange > 0)
                    {
                        if(GetTileType(nextTilePosition) != rockTile && UnitTeamColor(nextTilePosition) != selectedUnit.GetComponent<Unit>().teamColor)
                        {
                            dynamicTilemapBottomLayer.SetTile(nextTilePosition, dynamicTiles.attackTile);
                        }
                        HighlightMovableTiles(nextTilePosition, unit, 0, remaningAttackRange - 1);
                    }
                }
            }
        }
    }

    public void FindAttackableTiles(UnitType unit, Vector3 unitPosition)
    {
        targetUnit = null;
        dynamicTilemapTopLayer.ClearAllTiles();
        dynamicTilemapBottomLayer.ClearAllTiles();
        Vector3Int startPos = new Vector3Int((int)unitPosition.x, (int)unitPosition.y, (int)unitPosition.z);
        HighlightAttackableTiles(startPos, unit, unit.attackRange);

        dynamicTilemapTopLayer.SetTile(startPos, dynamicTiles.selectedUnitTile);
        dynamicTilemapBottomLayer.SetTile(startPos, null);
    }

    void HighlightAttackableTiles(Vector3Int currentTilePosition, UnitType unit, int remaningAttackRange)
    {
        for (int x = 0; x < posX.Length; x++)
        {
            Vector3Int nextTilePosition = new Vector3Int(currentTilePosition.x + posX[x], currentTilePosition.y + posY[x], currentTilePosition.z);
            if (tilemap.GetTile<Tile>(nextTilePosition) != null && remaningAttackRange > 0)
            {
                if(remaningAttackRange != 1)
                {
                    HighlightAttackableTiles(nextTilePosition, unit, remaningAttackRange - 1);
                }
                else
                {
                    if(UnitOnTile(nextTilePosition) && UnitTeamColor(nextTilePosition) != selectedUnit.GetComponent<Unit>().teamColor)
                    {
                        dynamicTilemapBottomLayer.SetTile(nextTilePosition, dynamicTiles.attackTile);
                        if(targetUnit != null)
                        {
                            if(targetUnit.GetComponent<Unit>().health > GetUnitOnTile(nextTilePosition).GetComponent<Unit>().health)
                            {
                                targetUnit = GetUnitOnTile(nextTilePosition);
                            }
                        }
                        else
                        {
                            targetUnit = GetUnitOnTile(nextTilePosition);
                        }
                        AI_InRange = true;
                    }
                    else
                    {
                        dynamicTilemapBottomLayer.SetTile(nextTilePosition, dynamicTiles.occupiedMoveTile);
                    }
                    
                }
            }
        }
    }

    /*
     * Will check twice the unit's move distance for enemy units.
     * This will add any enemy units it finds to the PotentialTarget list.
     */

    void EnemyRadar(int RemaningMoves, Vector3Int currentPosition)
    {
        for (int i = 0; i < posX.Length; i++)
        {
            Vector3Int nextTilePosition = new Vector3Int(currentPosition.x + posX[i], currentPosition.y + posY[i], currentPosition.z);
            if (UnitOnTile(nextTilePosition) && UnitTeamColor(nextTilePosition) != selectedUnit.GetComponent<Unit>().teamColor)
            {
                PotentialTargets.Add(GetUnitOnTile(nextTilePosition));
            }
            if(RemaningMoves > 0)
            {
                EnemyRadar(RemaningMoves - 1, nextTilePosition);
            }
        }
    }

    //Selects a target from the potential target list for the AI.
    void SelectTarget()
    {
        if(PotentialTargets.Count > 0)
        {
            targetUnit = PotentialTargets[0];
            //Compare each target to find a good one to select.
            if (PotentialTargets.Count > 1)
            {
                for (int i = 1; i < PotentialTargets.Count; i++)
                {
                    //Check for the least amount of health
                    if(PotentialTargets[i].GetComponent<Unit>().health < targetUnit.GetComponent<Unit>().health)
                    {
                        targetUnit = PotentialTargets[i];
                    }

                }
            }
        }
        else
        {
            targetUnit = null;
        }
    }

    //Finds the closest tile for the AI to move their unit to their target.
    void MoveUnitToTile(Vector3 tilePosition)
    {
        movePoint = new Vector3(tilePosition.x + unitOffset, tilePosition.y + unitOffset, 0f);
        selectedUnit.GetComponent<Animator>().SetBool("walking", true);
        gameState = GameState.UnitWalking;
        Debug.Log("Moving the selected unit...");
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
     * Will check if a unit is at the position
     */
    bool UnitOnTile(Vector3Int position)
    {
        Collider2D unit = Physics2D.OverlapBox(new Vector2(position.x + unitOffset, position.y + unitOffset), new Vector2(0.1f, 0.1f), 0.0f);

        if(unit != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    GameObject GetUnitOnTile(Vector3Int position)
    {
        Collider2D unit = Physics2D.OverlapBox(new Vector2(position.x + unitOffset, position.y + unitOffset), new Vector2(0.1f, 0.1f), 0.0f);
        return unit.gameObject;
    }

    Team UnitTeamColor(Vector3Int position)
    {
        Collider2D unit = Physics2D.OverlapBox(new Vector2(position.x + unitOffset, position.y + unitOffset), new Vector2(0.1f, 0.1f), 0.0f);

        if(unit != null)
        {
            return unit.GetComponent<Unit>().teamColor;
        }
        else
        {
            return Team.None;
        }
    }

    void SetTargetUnit(Vector3 position)
    {
        Collider2D unit = Physics2D.OverlapBox(new Vector2(position.x + unitOffset, position.y + unitOffset), new Vector2(0.1f, 0.1f), 0.0f);

        if(unit != null)
        {
            targetUnit = unit.gameObject;
            Debug.Log($"Selected {unit.name} as a target.");
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
        else if (tilemap.GetTile<Tile>(position) == lavaTile.tile)
        {
            return lavaTile;
        }
        else
        {
            return grassTile;
        }
    }

    public void IsUnitMoving()
    {
        if(updateGameState == GameState.MovingUnit)
        {
            gameState = GameState.MovingUnit;
            updateGameState = GameState.Menu;
        }
    }

    public void SetUnitMoving()
    {
        updateGameState = GameState.MovingUnit;
    }

    public void IncreaseBlueUnitCount()
    {
        blueUnitCount++;
        Debug.Log($"There are {blueUnitCount} blue units total.");
    }
    public void IncreaseRedUnitCount()
    {
        redUnitCount++;
        Debug.Log($"There are {redUnitCount} red units total.");
    }
    public void DecreaseBlueUnitCount()
    {
        blueUnitCount--;
        Debug.Log($"There are {blueUnitCount} blue units total.");
    }
    public void DecreaseRedUnitCount()
    {
        redUnitCount--;
        Debug.Log($"There are {redUnitCount} red units total.");
    }
    public void AddToUnitList(GameObject u)
    {
        if(u.GetComponent<Unit>().teamColor == Team.Blue)
        {
            BlueUnitList.Add(u);
        }
        else if(u.GetComponent<Unit>().teamColor == Team.Red)
        {
            RedUnitList.Add(u);
        }
        else
        {
            Debug.LogError($"Unit {u.name} does not have a team color.");
        }
    }
    public void AddToInactiveList(Unit unit)
    {
        inactiveUnits.Add(unit);
    }
    public void GameOverCheck()
    {
        if(blueUnitCount <= 0 || redUnitCount <= 0)
        {
            if (blueUnitCount <= 0 && redUnitCount <= 0)
            {
                tieText.SetActive(true);
            }
            else if (blueUnitCount <= 0)
            {
                //Red Wins
                redWinsText.SetActive(true);
            }
            else if (redUnitCount <= 0)
            {
                //Blue Wins
                blueWinsText.SetActive(true);
            }
            gameState = GameState.GameOver;
        }
    }
}