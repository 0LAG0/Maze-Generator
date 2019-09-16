using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MazeGenerator : MonoBehaviour
{
    //algorithm
    AlgorithmHelper algorithmHelper = new AlgorithmHelper();
    public int Width;
    public int Height;
    private MazeCell[,] grid;

    //mazeDrawer
    public int[,] inGameGrid;
    private static GameObject mazeContainer;
    public GameObject Wall;
    public GameObject Ground;

    //ingame-maze-elements
    public Vector2 goal;

    //usable algorithms
    private bool HuntKill = false;
    private bool RecursiveBacktrack = true;
    
    //UI-Elements
    public InputField WidthInput;
    public InputField HeightInput;
    public Dropdown DropdownAlgorithm;
    
    //ingame grid-dimensions
    private float cellWidth = 0.69f;
    private float cellHeight = 0.52f;

    //initial setup for a 10x10-maze generated with recusive-Backtracking algorithm
    void Start()
    {
        Width = 10;
        Height = 10;
        grid = algorithmHelper.InitializeGrid(Width, Height);
        algorithmHelper.CalculateRecursiveMaze();
        DrawMazeFromGrid(grid);
        goal = new Vector2(21 * cellWidth, 20 * cellHeight);
    }

    public void Generate()
    {
        //set height and width incase the user input is not usable
        int height, width;

        //take input from the GUI
        if (int.TryParse(HeightInput.text, out height))
        {
            Height = Mathf.Max(2, height); ;
        }
        if (int.TryParse(WidthInput.text, out width))
        {
            Width = Mathf.Max(2, width);
        }

        //set new goal of the maze in the upper right corner
        goal = new Vector2((Width * 2 + 1) * cellWidth, Height * 2 * cellHeight);

        SetCameraToMaze();

        //setup grid to use for the algorithms
        grid = algorithmHelper.InitializeGrid(Width,Height);

        //choose algorithm to use
        if (RecursiveBacktrack)
        {
            algorithmHelper.CalculateRecursiveMaze();
        }
        else
        {
            algorithmHelper.CalculateHuntKillMaze();
        }
        DrawMazeFromGrid(grid);
    }

    //camera controller-method
    public void SetCameraToMaze()
    {
        float ingameWidth = ((Width * 2 + 2) * cellWidth);
        float ingameHeight = ((Height * 2 + 2) * cellHeight);
        //set camera position to center of maze with a small offset so there is room for the GUI
        Vector3 camPos = Camera.main.transform.position;
        camPos.x = Width * cellWidth;
        camPos.y = Height * cellHeight + 0.07f + ingameWidth/80;
        Camera.main.transform.position = camPos;

        //calculate ingame dimensions of grid
        /*calculate the screen and maze ratios in case the maze is wider than it is heigh
         *since the orthographicSize is entirely dependent on the height*/
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float TargetRatio = ingameWidth / ingameWidth;
        if (screenRatio >= TargetRatio)
        {
            //calculate orthografic´Size through the dimensions and sprite sizes
            Camera.main.orthographicSize = ((Height * 2 + 1) / 2 + cellHeight + 0.07f) * 0.6f;
        }
        else
        {
            //calculate orthograficSize through the dimensions, sprite sizes AND ratios
            float differenceInSize = TargetRatio / screenRatio;
            Camera.main.orthographicSize = ((Height * 2 + 1) / 2 + cellHeight + 0.07f) * 0.6f * differenceInSize;
        }
    }

    //method called by the dropdown menu to choose algorithm
    public void changeAlgorithm()
    {
        if (DropdownAlgorithm.value == 0)
        {
            HuntKill = false;
            RecursiveBacktrack = true;
        }
        else
        {
            HuntKill = true;
            RecursiveBacktrack = false;
        }
    }

    public void DrawMazeFromGrid(MazeCell[,] grid)
    {
        //create new maze container
        if (mazeContainer == null)
        {
            mazeContainer = new GameObject("MazeContainer");
        }
        //clear the old/previous maze to generate a new one
        else
        {
            ClearMaze();
        }

        /*
         *setup ingame grid to draw it and to be later used by the mazesolver.
         *the ingame grid is size*2+1 big because it is bigger than the grid 
         *used to calculate the original ingame grid since i used 
         *wall-sprites that have the same dimensions as the floor-tiles.
         * 
         *1 means its floor, 2 means its wall and later the mazesolver sets 
         *the floor tiles to 2, to indicate the tile has been visited 
         *(the mazesolver uses recusive backtracking to solve the maze)
         */
        inGameGrid = new int[Width * 2 + 1, Height * 2 + 1];

        //setup start and finish since they basically are holes in the surrounding walls
        GameObject start = Instantiate(Ground, new Vector2(0, cellHeight), Quaternion.identity);
        start.transform.parent = mazeContainer.transform;
        start.name = "Start";
        inGameGrid[0, 1] = 1;

        GameObject goal = Instantiate(Ground, new Vector2(Width * 2 * cellWidth, (Height * 2 - 1) * cellHeight), Quaternion.identity);
        goal.transform.parent = mazeContainer.transform;
        goal.name = "Goal";
        inGameGrid[Width * 2, Height * 2 - 1] = 1;

        for (int x = 0; x < Width * 2 + 1; x++)
        {
            for (int y = 0; y < Height * 2 + 1; y++)
            {
                if (x % 2 != 0 && y % 2 != 0)
                {
                    //instantiate initial cells
                    GameObject groundRegular = Instantiate(Ground, new Vector2(x * cellWidth, y * cellHeight), Quaternion.identity);
                    groundRegular.transform.parent = mazeContainer.transform;
                    groundRegular.name = "Ground_" + x + "_" + y;
                    inGameGrid[x, y] = 1;
                    //initialize the connections to the accessible neighbors
                    foreach (Vector2Int d in grid[x / 2, y / 2].directions)
                    {
                        GameObject groundExtra = Instantiate(Ground, new Vector2((x + d.x) * cellWidth, (y + d.y) * cellHeight), Quaternion.identity);
                        groundExtra.transform.parent = mazeContainer.transform;
                        groundExtra.name = "Ground_" + (x + d.x) + "_" + (y + d.y);
                        inGameGrid[x + d.x, y + d.y] = 1;
                    }
                }
                else if (inGameGrid[x, y] != 1)
                {
                    //initialize the walls
                    GameObject wall = Instantiate(Wall, new Vector2(x * cellWidth, y * cellHeight + 0.07f), Quaternion.identity);
                    wall.GetComponent<SpriteRenderer>().sortingOrder = -y;
                    wall.transform.parent = mazeContainer.transform;
                    wall.name = "Wall_" + x + "_" + y;
                    inGameGrid[x, y] = 0;
                }
            }
        }
    }

    //method used to clear/delete old maze to draw a new one
    public void ClearMaze()
    {
        foreach (Transform transform in mazeContainer.transform)
        {
            Destroy(transform.gameObject);
        }
    }
}