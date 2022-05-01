using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class menuManager : MonoBehaviour
{
    public Canvas title;
    public Canvas instructions;
    public Canvas levelsA;
    public Canvas levelsB;
    public Canvas controls;
    public Canvas gamemode;
    public Canvas teamColor;
    public GameManager AIControl;
    
    // Start is called before the first frame update
    void Start()
    {
        levelsA.enabled = false;
        levelsB.enabled = false;
        instructions.enabled = false;
        controls.enabled = false;
        gamemode.enabled = false;
        teamColor.enabled = false;
    }

    public void StartMenu()
    {
        title.enabled = false;
        gamemode.enabled = true;
    }

    public void levelSelect(string level)
    {
        SceneManager.LoadScene(level);
    }

    public void turnPage(string page)
    {
        if (page == "1")
        {
            levelsA.enabled = false;
            levelsB.enabled = true;
        }
        else if(page == "2")
        {
            levelsA.enabled = true;
            levelsB.enabled = false;

        }
    }
    public void Instructions(string back)
    {
        if (back == "0")
        {
            title.enabled = false;
            instructions.enabled = true;
        }
        else if (back == "1")
        {
            instructions.enabled = false;
            title.enabled = true;
        }

    }

    public void Controls(string back)
    {
        if(back == "0")
        {
            title.enabled = false;
            controls.enabled = true;
        }
        else if (back == "1")
        {
            title.enabled = true;
            controls.enabled = false;
        }
    }
    
    public void GameMode(string players)
    {
        if (players == "1")
        {
            teamColor.enabled = true;
            gamemode.enabled = false;
        }
        else if (players == "2")
        {
            AIControl.setAI(players);
            gamemode.enabled = false;
            levelsA.enabled = true;
        }
    }

    public void TeamColor(string team)
    {
        if (team == "0")
        {
            AIControl.setAI(team);
            teamColor.enabled = false;
            levelsA.enabled = true;
        }
        else if (team == "1")
        {
            AIControl.setAI(team);
            teamColor.enabled = false;
            levelsA.enabled = true;
        }
    }
}
