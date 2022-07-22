using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{

    public int tileX;
    public int tileY;
    public Grid map;

    public List<Node> currentPath = null;

 
  

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            MoveNextTile();
        }

        Debug.Log(currentPath);
        if (currentPath != null)
        {
            int currNode = 0;

            while( currNode < currentPath.Count-1)
            {
                Debug.Log(currentPath[currNode].x.ToString()+",,," + currentPath[currNode].y.ToString());

                Vector3 start = map.TileCoordToWorldCoord(currentPath[currNode].x, currentPath[currNode].y) +
                    new Vector3(0,0,-1f);
                Vector3 end = map.TileCoordToWorldCoord(currentPath[currNode+1].x, currentPath[currNode+1].y) +                 
                    new Vector3(0, 0, -1f);
                 


                //draw line
                Debug.DrawLine(start, end, Color.red);
                Debug.Log(tileX.ToString() + ",,," + tileY.ToString()+",,," + currNode.ToString()+",,," + (currentPath.Count - 1).ToString());

                currNode++;
            }
        }
    }
   
    public void MoveNextTile()
    {

        if (currentPath == null)
                return;

        //update tile location
        UpdateAgentLocation();

        // Remove the old "current" tile
        currentPath.RemoveAt(0);

        transform.position = map.TileCoordToWorldCoord(currentPath[0].x, currentPath[0].y);

         if (currentPath.Count == 1)
         {
             // We only have one tile left in the path, and that tile MUST be our ultimate
             // destination -- and we are standing on it!
             // So let's just clear our pathfinding info.
             currentPath = null;
         }
     }




    public void UpdateAgentLocation()
    {
        //update tile location
        tileX = currentPath[1].x;
        tileY = currentPath[1].y;
    }
}
