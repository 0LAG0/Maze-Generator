using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCell
{
    //whether the cell is already part of the maze or not
    public bool visited = false;
    //directions to neigboring cells that are accessible from this cell
    public List<Vector2Int> directions = new List<Vector2Int>();
}
