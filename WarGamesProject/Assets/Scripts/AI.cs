using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
