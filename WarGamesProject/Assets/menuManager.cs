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
    
    // Start is called before the first frame update
    void Start()
    {
        levelsA.enabled = false;
        levelsB.enabled = false;
        instructions.enabled = false;
        controls.enabled = false;
    }

    public void StartMenu()
    {
        title.enabled = false;
        levelsA.enabled = true;
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
}
