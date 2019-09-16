using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgorithmHelper
{
    private Vector2Int currentCell;
    private MazeCell[,] grid;
    private bool fin = false;
    private int Width, Height;

    public MazeCell[,] InitializeGrid(int width,int height)
    {
        fin = false;
        Width = width;
        Height = height;
        grid = new MazeCell[Width, Height];

        //fill grid with "empty" cells
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                grid[x, y] = new MazeCell();
            }
        }
        return grid;
    }

    //Recursive-Backtracking algorithms as described here: http://weblog.jamisbuck.org/2010/12/27/maze-generation-recursive-backtracking.html
    public void CalculateRecursiveMaze()
    {
        //set random starting location
        currentCell = new Vector2Int(Random.Range(0, grid.GetLength(0)), Random.Range(0, grid.GetLength(1)));
        //initialize stack for backtracking
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        while (true)
        {
            //set current cell as part of the maze (visited)
            grid[currentCell.x, currentCell.y].visited = true;

            //calculate all unvisited neighbors (plus edge detection) to carve a random path in the maze
            List<Vector2Int> neighbors = calcNeighbors(grid, currentCell, false);

            //if unvisited neighbors are found choose a random one and set it as the current cell (step into it)
            if (neighbors.Count != 0)
            {
                stack.Push(new Vector2Int(currentCell.x, currentCell.y));
                Vector2Int nextCell = neighbors[Random.Range(0, neighbors.Count)];
                Step(nextCell);
            }
            /*if no unvisited neighbors are found and stack is not empty (maze is not finished), 
             *backtrack through stack until you find a cell that has unvisited neighbors*/
            else if (stack.Count > 0)
            {
                Vector2Int cell = stack.Pop();
                currentCell = cell;
            }
            //stack is empty -> maze is finished
            else
            {
                break;
            }
        }
    }

    //Hunt-and-Kill algorithms as described here: http://weblog.jamisbuck.org/2011/1/24/maze-generation-hunt-and-kill-algorithm.html
    public void CalculateHuntKillMaze()
    {
        currentCell = new Vector2Int(Random.Range(0, grid.GetLength(0)), Random.Range(0, grid.GetLength(1)));
        while (true)
        {
            grid[currentCell.x, currentCell.y].visited = true;
            //calculate all unvisited neighbors (plus edge detection) to carve a random path in the maze
            List<Vector2Int> neighbors = calcNeighbors(grid, currentCell, false);

            //if unvisited neighbors are found choose a random one and set it as the current cell (step into it)
            if (neighbors.Count != 0)
            {
                Vector2Int nextCell = neighbors[Random.Range(0, neighbors.Count)];
                Step(nextCell);
            }
            //no adjacent cells found and maze not finished -> initialize hunt mode
            else if (!fin)
            {
                Hunt();
            }
            //hunt loop ran through complete grid and didn't find any unvisited cells -> maze is finished
            else
            {
                break;
            }
        }
    }

    //method used to cennect two cells and set the next one as the current one (step into it)
    public void Step(Vector2Int next)
    {
        //connection from current to next
        Vector2Int direction = next - new Vector2Int(currentCell.x, currentCell.y);
        //connection from next to current
        Vector2Int reverseDirection = Vector2Int.zero - direction;
        //set connections/direction
        grid[currentCell.x, currentCell.y].directions.Add(direction);
        grid[next.x, next.y].directions.Add(reverseDirection);
        //step into next cell
        currentCell = next;
    }

    /*method used in the Hunt-and-Kill algorithm to find 
     * an unvisited cell to carve a new random path in the maze*/
    public void Hunt()
    {
        //start iteration through full grid
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                //unvisited cell found
                if (!grid[x, y].visited)
                {
                    //calculate all neighbors that are already visited
                    List<Vector2Int> neighborsHunt = calcNeighbors(grid, new Vector2Int(x, y), true);
                    //if unvisited neighbors are found
                    if (neighborsHunt.Count != 0)
                    {
                        //connect/step into randomly found cell
                        currentCell = new Vector2Int(x, y);
                        Vector2Int chosenNeighbor = neighborsHunt[Random.Range(0, neighborsHunt.Count)];
                        Step(chosenNeighbor);
                        //Step back into the formerly found cell to start carving a path from that point
                        currentCell = new Vector2Int(x, y);
                        return;
                    }
                }
            }
        }
        //set maze to finished so the CalculateHuntKillMaze() method can exit its loop
        fin = true;
    }

    /*method used to calculate all adjacent cells of current cell (up, down, right, left) 
     *and doing a boundary test to ensure to not step outside of the maze*/
    public List<Vector2Int> calcNeighbors(MazeCell[,] grid, Vector2Int currentCell, bool type)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        if (currentCell.x > 0 && grid[currentCell.x - 1, currentCell.y].visited == type)
        {
            neighbors.Add(new Vector2Int(currentCell.x - 1, currentCell.y));
        }
        if (currentCell.x < Width - 1 && grid[currentCell.x + 1, currentCell.y].visited == type)
        {
            neighbors.Add(new Vector2Int(currentCell.x + 1, currentCell.y));
        }
        if (currentCell.y > 0 && grid[currentCell.x, currentCell.y - 1].visited == type)
        {
            neighbors.Add(new Vector2Int(currentCell.x, currentCell.y - 1));
        }
        if (currentCell.y < Height - 1 && grid[currentCell.x, currentCell.y + 1].visited == type)
        {
            neighbors.Add(new Vector2Int(currentCell.x, currentCell.y + 1));
        }

        return neighbors;
    }
}
