using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AI steps
 * 1. Find all of it's active units at the start of the turn.
 * 2. Select one unit from that list that is not inactive. If none are left active, End Turn.
 * 3. First check if there is an enemy unit within the move range of the unit.
 *  3A. If true, Step 4
 *  3B. If false, check if there is an enemy unit within the range of 2* the move amount.
 *      3BA. If true, 
 *      3BB. If false, set unit to Wait (inactive). Then return to step 2.
 * 4. Find the enemy unit that has the least amount of health and set that as the target.
 * 5. Move to a tile that is within attacking range to the targeted unit.
 * 6. Attack the target. Then return to step 2.
 */

public class AI : MonoBehaviour
{
    public GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if(gameManager == null)
        {
            Debug.LogError("AI: Could not find the Game Manager in the scene.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
