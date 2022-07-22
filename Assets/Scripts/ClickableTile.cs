using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTile : MonoBehaviour
{
    public int tileX;
    public int tileY;
    public Grid map;



    private void OnMouseUp()
    {

        map.GeneratePathTo(tileX, tileY);

        Material[] list;
        list = gameObject.GetComponents<Material>();

        foreach(Material m in list)
        {
            Color c = m.color;
            c.a = 0f;
            m.color = c;
        }

    }


}
