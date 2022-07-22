using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Objective[] objectivePrefabs;
    public GameObject[] cosmeticPrefabs;
    public Consumables[] consumablesPrefabs;
    public GameObject[] terrainPrefabs;
    public GameObject waterPrefab;
    public GameObject[] villagesPrefabs;
    public float waterLevel = .1f;
    public float dirtLevel = .25f;
    public float scale = .1f;

    Cell[,] grid;

    private GameManager gm;

    private void Awake()
    {
        gm = FindObjectOfType<GameManager>();
    }


    void Start()
    {
        //setup selectedUnit's variable
        selectedUnit.GetComponent<Unit>().tileX = (int)selectedUnit.transform.position.x;
        selectedUnit.GetComponent<Unit>().tileY = (int)selectedUnit.transform.position.z;
        selectedUnit.GetComponent<Unit>().map = this;



        float[,] noiseMap = new float[gm.sizeN, gm.sizeM];
        (float xOffset, float yOffset) = (Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        for (int y = 0; y < gm.sizeM; y++)
        {
            for (int x = 0; x < gm.sizeN; x++)
            {
                float noiseValue = Mathf.PerlinNoise(x * scale + xOffset, y * scale + yOffset);
                noiseMap[x, y] = noiseValue;
            }
        }

        float[,] falloffMap = new float[gm.sizeN, gm.sizeM];
        for (int y = 0; y < gm.sizeM; y++)
        {
            for (int x = 0; x < gm.sizeN; x++)
            {
                float xv = x / (float)gm.sizeN * 2 - 1;
                float yv = y / (float)gm.sizeM * 2 - 1;
                float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));
                falloffMap[x, y] = Mathf.Pow(v, 3f) / (Mathf.Pow(v, 3f) + Mathf.Pow(2.2f - 2.2f * v, 3f));
            }
        }

        grid = new Cell[gm.sizeN, gm.sizeM];
        for (int y = 0; y < gm.sizeM; y++)
        {
            for (int x = 0; x < gm.sizeN; x++)
            {
                float noiseValue = noiseMap[x, y];
                noiseValue -= falloffMap[x, y];
                bool isWater = noiseValue < waterLevel;
                bool isDirt = noiseValue < dirtLevel && noiseValue > waterLevel;
                Cell cell = new Cell(isWater,isDirt,false,false,false);
                grid[x, y] = cell;
            }
        }

        DrawTerrain(grid);

        GeneratePathfindingGraph();

        SpawnVillages(grid);
        SpawnObjectives(grid);
        SpawnConsumambles(grid);
        SpawnCosmetics(grid);

    }

    void DrawTerrain(Cell[,] grid)
    {
        for(int y = 0;y < gm.sizeM; y++)
        {
            for (int x = 0; x < gm.sizeN; x++)
            {
                Cell cell = grid[x,y];
                if (cell.isWater)
                {
                    GameObject prefab = waterPrefab;
                    GameObject water = Instantiate(prefab, transform);
                    water.transform.position = new Vector3(x, 0, y);
                    water.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                }
                else
                {
                    int index;

                    if (cell.isDirt)
                        index = 0;
                    else
                        index = 1;
                    GameObject prefab = terrainPrefabs[index];

                    //
                    GameObject terrain = (GameObject)Instantiate(prefab, transform);

                    ClickableTile clicker = terrain.GetComponent<ClickableTile>();
                    clicker.tileX = x;
                    clicker.tileY = y;
                    clicker.map = this;


                    terrain.transform.position = new Vector3(x, 0, y);
                    terrain.transform.localScale = new Vector3(0.5f,0.5f,0.5f);

                }
            }
        }

    }



    /////////////////////////////////////////////////

    /////////////////////////////////////////////////
    public GameObject selectedUnit;
    List<Node> currentPath = null;
    Node[,] graph;



    // public Unit[] units;  //for more units

    public Vector3 TileCoordToWorldCoord(int x, int y)
    {
        return new Vector3(x, 0.7f, y);
    }




    //cost of tiles
    public float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY)
    {
        float tile_cost = 1; //set basic movement

        Cell cost = grid[targetX, targetY];

        //set cost for tiles
        if (cost.isWater)
        {
            tile_cost = 20000000;
        }




        //if (UnitCanEnterTile(targetX, targetY) == false)
        //  return Mathf.Infinity;


        //prefer not moving diagonally
        if (sourceX != targetX && sourceY != targetY)
        {
            // We are moving diagonally!  Fudge the cost for tie-breaking
            // Purely a cosmetic thing!
            tile_cost += 0.001f;
        }

        return tile_cost;

    }




    public void GeneratePathTo(int x, int y)
    {
        //selectedUnit.GetComponent<Unit>().tileX = x;
        //selectedUnit.GetComponent<Unit>().tileY = y;
        //selectedUnit.transform.position = TileCoordToWorldCoord(x, y);

        //clear unit's old path
        selectedUnit.GetComponent<Unit>().currentPath = null;



        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        List<Node> unvisited = new List<Node>();


        //Node source = graph[selectedUnit.GetComponent<Unit>().tileX, selectedUnit.GetComponent<Unit>().tileY];
        Node source = graph[selectedUnit.GetComponent<Unit>().tileX, selectedUnit.GetComponent<Unit>().tileY];


        Node target = graph[x, y];


        dist[source] = 0;
        prev[source] = null;


        foreach (Node v in graph)
        {
            if (v != source)
            {

                dist[v] = Mathf.Infinity;
                prev[v] = null;


            }

            unvisited.Add(v);
        }

        while (unvisited.Count > 0)
        {
            //This is not fast but short
            //u is going to ve tge unvisited node witgh the smallest distance


            Node u = null;

            foreach (Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }

            }

            //Node u = unvisited.OrderBy(n => dist[n]).First();


            if (u == target)
            {
                break; //EXIT THE WHILE LOOP
            }

            unvisited.Remove(u);

            foreach (Node v in u.neighbours)
            {
                float alt = dist[u] + CostToEnterTile(u.x, u.y, v.x, v.y);
                if (alt < dist[v])
                {
                    dist[v] = alt;
                    prev[v] = u;
                }
            }
        }

        //if we get here, either we found the shortest route 
        //to our target or there is no route to him at all

        if (prev[target] == null)
        {

            // no route between the target and the source
            return;
        }

        List<Node> currentPath = new List<Node>();

        Node curr = target;
        while (curr != null)
        {
            currentPath.Add(curr);
            curr = prev[curr];

        }

        currentPath.Reverse();


        selectedUnit.GetComponent<Unit>().currentPath = currentPath;
    }







    void GeneratePathfindingGraph()
    {
        graph = new Node[gm.sizeN, gm.sizeM];

        //Initialize the array
        for (int y = 0; y < gm.sizeM; y++)
        {
            for (int x = 0; x < gm.sizeN; x++)
            {
                graph[x, y] = new Node();

                graph[x, y].x = x;
                graph[x, y].y = y;
            }
        }



        for (int y = 0; y < gm.sizeM; y++)
        {
            for (int x = 0; x < gm.sizeN; x++)
            {

                /*
                 * without diagonal movement
                if (x > 0)
                {
                    graph[x, y].neighbours.Add(graph[x - 1, y]);

                }
                if (x < size-1)
                {
                    graph[x, y].neighbours.Add(graph[x + 1, y]);

                }
                if (y > 0)
                {
                    graph[x, y].neighbours.Add(graph[x , y-1]);

                }
                if (y < size - 1)
                {
                    graph[x, y].neighbours.Add(graph[x, y+1]);

                }
                */


                // This is the 8-way connection version (allows diagonal movement)
                // Try left
                if (x > 0)
                {
                    graph[x, y].neighbours.Add(graph[x - 1, y]);
                    if (y > 0)
                        graph[x, y].neighbours.Add(graph[x - 1, y - 1]);
                    if (y < gm.sizeM - 1)
                        graph[x, y].neighbours.Add(graph[x - 1, y + 1]);
                }

                // Try Right
                if (x < gm.sizeN - 1)
                {
                    graph[x, y].neighbours.Add(graph[x + 1, y]);
                    if (y > 0)
                        graph[x, y].neighbours.Add(graph[x + 1, y - 1]);
                    if (y < gm.sizeM - 1)
                        graph[x, y].neighbours.Add(graph[x + 1, y + 1]);
                }

                // Try straight up and down
                if (y > 0)
                    graph[x, y].neighbours.Add(graph[x, y - 1]);
                if (y < gm.sizeM - 1)
                    graph[x, y].neighbours.Add(graph[x, y + 1]);

                // This also works with 6-way hexes and n-way variable areas (like EU4)


            }
        }



    }


    /////////////////////////////////////////////////

    /////////////////////////////////////////////////
    ///

    /////////////////////////////////////////////////













    void SpawnVillages(Cell[,] grid)
    {
        List<(int,int)> avalPos = new List<(int, int)>();

        for (int y = 0; y < gm.sizeM; y++)
        {
            for (int x = 0; x < gm.sizeN; x++)
            {
                avalPos.Add((x, y));
            } 
        }


        while (avalPos.Count > 0)
        {
            int index = Random.Range(0, avalPos.Count);
            (int x, int y) = avalPos[index];
            Cell cell = grid[x, y];

            if (!cell.isWater)
            {
                GameObject prefab = villagesPrefabs[0];

                bool canBuild = true;

                for (int i = 0; i < prefab.GetComponent<BoxCollider>().size.x; i++)
                {
                    for (int j = 0; j < prefab.GetComponent<BoxCollider>().size.z; j++)
                    {
                        Cell tmpCell = grid[x + i, y + j];

                        if (tmpCell.isWater || tmpCell.isPartVillage || tmpCell.hasObject || tmpCell.hasConsumable)
                        {
                            canBuild = false;
                            avalPos.RemoveAt(index);
                            break;
                        }
                    }
                    if (!canBuild)
                        break;
                }

                if (canBuild)
                {
                    for (int i = 0; i < prefab.GetComponent<BoxCollider>().size.x; i++)
                    {
                        for (int j = 0; j < prefab.GetComponent<BoxCollider>().size.z; j++)
                        {
                            grid[x + i, y + j].isPartVillage = true;
                            grid[x + i, y + j].hasObject = true;
                        }
                    }
                    GameObject village = Instantiate(prefab, transform);
                    village.transform.position = new Vector3(x, 0.5f, y);
                    village.transform.localScale = Vector3.one;

                    break;
                }
            }
        }

        
        while (avalPos.Count > 0)
        {
            int index = Random.Range(0, avalPos.Count);
            (int x, int y) = avalPos[index];
            Cell cell = grid[x, y];

            if (!cell.isWater)
            {
                GameObject prefab = villagesPrefabs[1];

                bool canBuild = true;
                for (int i = 0; i < prefab.GetComponent<BoxCollider>().size.x; i++)
                {
                    for (int j = 0; j < prefab.GetComponent<BoxCollider>().size.z; j++)
                    {
                        Cell tmpCell = grid[x+i, y+j];

                        if (tmpCell.isWater || tmpCell.isPartVillage || tmpCell.hasObject || tmpCell.hasConsumable)
                        {
                            canBuild = false;
                            avalPos.RemoveAt(index);
                            break;
                        }
                    }
                    if (!canBuild)
                        break;
                }

                if (canBuild)
                {
                    for (int i = 0; i < prefab.GetComponent<BoxCollider>().size.x; i++)
                    {
                        for (int j = 0; j < prefab.GetComponent<BoxCollider>().size.z; j++)
                        {
                            grid[x + i, y + j].isPartVillage = true;
                            grid[x + i, y + j].hasObject = true;
                        }
                    }
                    GameObject village = Instantiate(prefab, transform);
                    village.transform.position = new Vector3(x, 0.5f, y);
                    village.transform.localScale = Vector3.one;
                    return;
                }
            }
        }
    }

    void SpawnObjectives(Cell[,] grid)
    {
        List<(int, int)> avalPos = new List<(int, int)>();

        for (int x = 0; x < gm.sizeN; x++)
        {
            for (int y = 0; y < gm.sizeM; y++)
            {
                Cell cell = grid[x, y];
                if(!cell.hasObject && !cell.isWater && !cell.isPartVillage && !cell.hasConsumable)
                {
                    avalPos.Add((x, y));

                }
            }
        }

        foreach(Objective obj in objectivePrefabs)
        {
            int amount = 0;
            switch (obj.type)
            {
                case Objective.ObjectiveType.Rock:
                    amount = gm.amountOfRocks;
                    break;
                case Objective.ObjectiveType.Wood:
                    amount = gm.amountOfWood;
                    break;
                case Objective.ObjectiveType.Seeds:
                    amount = gm.amountOfSeeds;
                    break;
                case Objective.ObjectiveType.Ores:
                    amount = gm.amountOfOres;
                    break;
            }
            for(int i=0; i < amount *2; i++)
            {
                if(avalPos.Count > 0)
                {
                    int index = Random.Range(0, avalPos.Count);
                    (int x, int y) = avalPos[index];
                    avalPos.RemoveAt(index);
                    grid[x, y].hasObject = true;
                    GameObject prefab = obj.gameObject;
                    GameObject gameObject = Instantiate(prefab, transform);
                    gameObject.transform.position = new Vector3(x, gameObject.transform.position.y + .5f, y);
                }
                else
                {
                    return;
                }
            }
        }
    }

    void SpawnCosmetics(Cell[,] grid)
    {
        List<(int, int)> avalPos = new List<(int, int)>();

        for (int x = 0; x < gm.sizeN; x++)
        {
            for (int y = 0; y < gm.sizeM; y++)
            {
                Cell cell = grid[x, y];
                if (!cell.hasObject && !cell.isWater && !cell.isPartVillage && !cell.hasConsumable)
                {
                    avalPos.Add((x, y));

                }
            }
        }

        foreach (GameObject obj in cosmeticPrefabs)
        {
            int amount = ((avalPos.Count / 3) - cosmeticPrefabs.Length);
            for (int i = 0; i < amount * obj.GetComponent<Cosmetic>().percentage; i++)
            {
                if (avalPos.Count > 0)
                {
                    int index = Random.Range(0, avalPos.Count);
                    (int x, int y) = avalPos[index];
                    avalPos.RemoveAt(index);
                    grid[x, y].hasObject = true;
                    GameObject prefab = obj;
                    GameObject gameObject = Instantiate(prefab, transform);
                    gameObject.transform.position = new Vector3(x, gameObject.transform.position.y+.5f, y);
                }
                else
                {
                    return;
                }
            }
        }
    }

    void SpawnConsumambles(Cell[,] grid)
    {
        List<(int, int)> avalPos = new List<(int, int)>();

        for (int x = 0; x < gm.sizeN; x++)
        {
            for (int y = 0; y < gm.sizeM; y++)
            {
                Cell cell = grid[x, y];
                if (!cell.hasObject && !cell.isWater && !cell.isPartVillage && !cell.hasConsumable)
                {
                    avalPos.Add((x, y));

                }
            }
        }

        foreach (Consumables obj in consumablesPrefabs)
        {
            int amount = 0;

            switch (obj.type){
                case Consumables.ConsumableType.Gold:
                    amount = gm.amountOfGold;
                    break;
                case Consumables.ConsumableType.EnergyPot:
                    amount = gm.amountOfEnergyPots;
                    break;

            }
            for (int i = 0; i < amount; i++)
            {
                if (avalPos.Count > 0)
                {
                    int index = Random.Range(0, avalPos.Count);
                    (int x, int y) = avalPos[index];
                    avalPos.RemoveAt(index);
                    grid[x, y].hasObject = true;
                    grid[x, y].hasConsumable = true;
                    GameObject prefab = obj.gameObject;
                    GameObject gameObject = Instantiate(prefab, transform);
                    gameObject.transform.position = new Vector3(x, gameObject.transform.position.y + .5f, y);
                }
                else
                {
                    return;
                }
            }
        }
    }

}
