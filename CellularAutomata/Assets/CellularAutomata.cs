using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellularAutomata : MonoBehaviour {

    public GameObject Wall, Floor;
    public float Chance=.45f;
    public Vector2 size;
    public int DeathLimit;
    public int BirthLimit;
    public int Steps;

    int[,] map;

    List<GameObject> tiles = new List<GameObject>();
    List<Vector3> Floors = new List<Vector3>();

	// Use this for initialization
	void Start () {
        StartCoroutine(Generate());
    }
	
	// Update is called once per frame
	void Update () {
        if (!Generating)
        {
            if (Input.GetButtonDown("Jump"))
            {
                StartCoroutine(Generate());
            }
        }
	}

    bool Generating = false;
    IEnumerator Generate()
    {
        Generating = true;

        //Clear the list first
        if (tiles.Count > 0) {
            foreach (GameObject go in tiles) {
                Destroy(go);
            }
            tiles.Clear();
            Floors.Clear();
        }

        Time.timeScale = 100;

        //create new instance of map
        map = new int[(int)size.x, (int)size.y];

        //this will initialize the map[,] values, setting either 1 (true) or 0 (false)
        //the map is pretty messy but will be fixed once we smooth it
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (Random.value < Chance)
                {
                    map[x, y] = 1;
                }
            }
        }

        //smooth the tiles
        for (int i = 0; i < Steps; i++)
        {
            map = DoSimulationStep(map);
        }

        //this will instantiate the blocks based on map[,] values
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (map[x, y] == 1)
                {
                    GameObject go = Instantiate(Wall, new Vector3(x, y, 0), Quaternion.identity);
                    tiles.Add(go);
                }
                else
                {
                    GameObject go = Instantiate(Floor, new Vector3(x, y, 0), Quaternion.identity);
                    tiles.Add(go);
                    Floors.Add(go.transform.position);
                }
                yield return new WaitForFixedUpdate();
            }
        }
        
        PlaceTreasure();

        Time.timeScale = 1;
        Generating = false;
    }

    int[,] DoSimulationStep(int[,] OldMap) {
        int[,] NewMap = new int[(int)size.x, (int)size.y];
        for (int x = 0; x < OldMap.GetLength(0); x++)
        {
            for (int y = 0; y < OldMap.GetLength(1); y++)
            {
                int nbs = CountAliveNeighbors(OldMap, new Vector3(x,y));
                if (OldMap[x, y] == 1)
                {
                    if (nbs < DeathLimit)
                    {
                        NewMap[x, y] = 0;
                    }
                    else
                    {
                        NewMap[x, y] = 1;
                    }
                }
                else
                {
                    if (nbs > BirthLimit)
                    {
                        NewMap[x, y] = 1;
                    }
                    else
                    {
                        NewMap[x, y] = 0;
                    }
                }
            }
        }
        return NewMap;
    }

    int CountAliveNeighbors(int[,]_map, Vector2 _size)
    {
        int count = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++) {
                Vector2 Neighbor = new Vector2(_size.x + i, _size.y + j);

                //if looking on the middle
                if (i == 0 && j == 0)
                {
                    //then do nothing
                }
                //if looking on the edge of the grid
                else if (Neighbor.x < 0 || Neighbor.y < 0 || Neighbor.x >= _map.GetLength(0) || Neighbor.y >= _map.GetLength(1))
                {
                    //then count it as alive
                    count++;
                }
                //if the map has marked 1 or true
                else if(_map[(int)Neighbor.x, (int)Neighbor.y] == 1)
                {
                    //then count it as alive as well
                    count++;
                }
            }
        }
        return count;
    }

    public GameObject TreasurePrefab;
    public int HiddenTreasuresLimit = 5;
    void PlaceTreasure()
    {
        for (int x = 0; x < HiddenTreasuresLimit; x++)
        {
            GameObject treasure = Instantiate(TreasurePrefab, Floors[Random.Range(0, Floors.Count)], Quaternion.identity);
            tiles.Add(treasure);
        }
    }

}
