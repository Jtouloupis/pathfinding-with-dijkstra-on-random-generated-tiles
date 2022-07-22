using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public bool isWater;
    public bool isDirt;
    public bool hasObject;
    public bool hasConsumable;
    public bool isPartVillage;

    public Cell(bool isWater, bool isDirt, bool hasObject, bool hasConsumable, bool isPartVillage)
    {
        this.isWater = isWater;
        this.isDirt = isDirt;
        this.hasObject = hasObject;
        this.hasConsumable = hasConsumable;
        this.isPartVillage = isPartVillage;
    }
}
