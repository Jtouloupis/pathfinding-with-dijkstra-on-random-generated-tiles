using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumables : MonoBehaviour
{
    public enum ConsumableType
    {
        Gold,
        EnergyPot
    }

    [SerializeField]
    public float cooldown;
    [SerializeField]
    public ConsumableType type;

    public bool isInCooldown = false;

}
