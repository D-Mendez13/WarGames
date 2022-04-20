using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider healthBar;
    public Unit unit;

    // Start is called before the first frame update
    void Start()
    {
        healthBar = GetComponent<Slider>();
        healthBar.maxValue = unit.unitType.maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.value = unit.health;
    }
}
