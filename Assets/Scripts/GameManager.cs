using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Base Parameters")]
    [Range(100,10000)]
    [Tooltip("Width of map")]
    public int sizeN;
    [Range(100, 10000)]
    [Tooltip("Height of map")]
    public int sizeM;
    [Range(4, 10)]
    public int numberOfAgentsPerVillage;


    [Space]
    [Header("Objectives")]

    [Range(1,100)]
    public int amountOfRocks;
    [Range(1, 100)]
    public int amountOfSeeds;
    [Range(1, 100)]
    public int amountOfWood;
    [Range(1, 100)]
    public int amountOfOres;

    [Space]
    [Header("Consumables")]
    public int amountOfGold;
    public int amountOfEnergyPots;


    [Space]
    [Header("Item Cost / Values")]
    public int energyPotsValue;
    public int mapValue;

}
