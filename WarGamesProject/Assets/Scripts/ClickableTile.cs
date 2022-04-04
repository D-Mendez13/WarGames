using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTile : MonoBehaviour
{
    public TileType tileType;
    public GameObject tileHighlight;
    private bool mouseOver = false;
    private int tileX;
    private int tileY;

    private void OnMouseEnter()
    {
        mouseOver = true;
        tileHighlight.SetActive(true);
    }

    private void OnMouseExit()
    {
        mouseOver=false;
        tileHighlight.SetActive(false);
    }

    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = tileType.tileImage;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (mouseOver)
            {
                Debug.Log("Clicked "+ gameObject.name);
            }
        }
    }
}
