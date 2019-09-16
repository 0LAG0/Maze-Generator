using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MazeSolver : MonoBehaviour
{
    public Animator animator;
    public GameObject MazeGenerator;

    //Path
    private Vector2Int[] path;
    private Vector2Int currentCell;
    private int posOnPath = 0;
    private GameObject pathContainer;
    private Vector3 mazeGoal;
    public GameObject Dot;
    public GameObject Fireworks;


    //States
    private bool mazeSolved = false;
    private bool isSolving = false;
    public bool isManual = false;
    private bool isCelebrating = false;
    private bool hasWon = false;

    //Movement
    private Rigidbody2D rb;
    private Vector2 moveVelocity;
    private float speed = 2;

    //Level
    private float cellWidth = 0.69f;
    private float cellHeight = 0.52f;

    public Toggle ManualModeCheckbox;
    public Slider speedSlider;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mazeGoal = MazeGenerator.GetComponent<MazeGenerator>().goal;
    }
    
    void Update()
    {
        //manage layer so the hero doesn't overlap with walls
        int layerOrder = (int)((-transform.position.y / cellHeight) + 0.3f);
        this.GetComponent<SpriteRenderer>().sortingOrder = layerOrder;
        //movement and animation for automated solving of maze
        if (isSolving)
        {
            Vector3 target = new Vector3(path[posOnPath].x * cellWidth, path[posOnPath].y * cellHeight + 0.48f, -0.1f);

            Vector2 movement = target - transform.position;
            animator.SetFloat("horizontal", movement.x);
            animator.SetFloat("vertical", movement.y);
            animator.SetFloat("magnitude", movement.magnitude);


            // Move our position a step closer to the target.
            float step = speed * Time.deltaTime;
            // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, target, step);

            //instantiate next mark of path
            if (Vector3.Distance(transform.position, target) < 0.001f)
            {
                GameObject dot = Instantiate(Dot, new Vector3(path[posOnPath].x * cellWidth, path[posOnPath].y * cellHeight, -0.01f), Quaternion.identity);
                dot.transform.parent = pathContainer.transform;
                dot.name = "Poop_" + posOnPath;
                transform.position = target;
                posOnPath++;
            }
            //end of the path reached -> dance animation starts
            if (posOnPath == path.Length)
            {
                isSolving = false;
                isCelebrating = true;
                animator.SetFloat("magnitude", 0);
                animator.SetBool("solved", true);
            }

        }
        //movement and animation for manual/play mode
        if (isManual)
        {
            speed = 4;
            Vector3 movementManual = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            animator.SetFloat("horizontal", movementManual.x);
            animator.SetFloat("vertical", movementManual.y);
            animator.SetFloat("magnitude", movementManual.magnitude);
            //movement
            moveVelocity = movementManual.normalized * speed;
            rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);

            Vector3 relativPos = transform.position - new Vector3(0, -0.48f, 0.1f);
            //goal was reached solving it by oneself -> fireworks
            if (Vector2.Distance(transform.position, mazeGoal) < 1f)
            {
                ManualModeCheckbox.isOn = false;
                if (!isCelebrating)
                {
                    GameObject fireworks = Instantiate(Fireworks, transform.position + new Vector3(0.1f, 0.5f, 0.05f), Quaternion.identity);
                    fireworks.GetComponent<SpriteRenderer>().sortingOrder = layerOrder;
                }
                isManual = false;
                isCelebrating = true;
                hasWon = true;
                animator.SetFloat("magnitude", 0);
                animator.SetBool("solved", true);
            }
        }
    }

    //method for switching into manual/play mode
    public void switchMode()
    {
        if (ManualModeCheckbox.isOn)
        {
            isManual = true;
            if (isSolving)
            {
                isSolving = false;
                ClearPath();
                posOnPath = 0;
            }
        }
        else
        {
            isManual = false;
        }
    }

    //method to accelerate solving process (does not work with manual mode)
    public void setSpeed()
    {
        if (!isManual)
        {
            speed = speedSlider.value;
        }

    }

    //reset both path and position of character
    public void Reset()
    {
        if (hasWon)
        {
            GameObject[] gos = GameObject.FindGameObjectsWithTag("Fireworks");
            foreach (GameObject fire in gos)
            {
                Destroy(fire);
            }
            hasWon = false;
        }

        animator.SetFloat("horizontal", 0);
        animator.SetFloat("vertical", 0);
        animator.SetFloat("magnitude", 0);
        isSolving = false;
        isCelebrating = false;
        posOnPath = 0;
        ClearPath();
        animator.SetBool("solved", false);
        transform.position = new Vector3(0, 1, -0.1f);
    }

    //starts the solving process and deactivates the manual mode
    public void startSolving()
    {
        speed = speedSlider.value;
        if (!isCelebrating)
        {
            if (isManual)
            {
                ManualModeCheckbox.isOn = false;
                transform.position = new Vector3(0, 1, -0.1f);
                isManual = false;
            }
            if (!mazeSolved)
            {
                SolveMaze();
            }
            if (pathContainer == null)
            {
                pathContainer = new GameObject("PathContainer");
            }
            isSolving = true;
        }
    }

    //sets a new goal for the newly generated maze
    public void newMazeDetected()
    {
        mazeGoal = MazeGenerator.GetComponent<MazeGenerator>().goal;
        mazeSolved = false;
    }

    //removes old path
    public void ClearPath()
    {
        if (pathContainer != null)
        {
            foreach (Transform transform in pathContainer.transform)
            {
                Destroy(transform.gameObject);
            }
        }
    }

    //also uses the recursive backtracking algorithm as used in the mazeGenerator
    public void SolveMaze()
    {
        int[,] maze = MazeGenerator.GetComponent<MazeGenerator>().inGameGrid;
        int width = MazeGenerator.GetComponent<MazeGenerator>().Width * 2 + 1;
        int height = MazeGenerator.GetComponent<MazeGenerator>().Height * 2 + 1;
        Vector2Int goal = new Vector2Int(width - 1, height - 2);

        currentCell = new Vector2Int(0, 1);
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        while (true)
        {
            maze[currentCell.x, currentCell.y] = 2;
            //calculate all neighbors (plus edge detection)
            List<Vector2Int> neighbors = new List<Vector2Int>();
            if (currentCell.x > 0 && maze[currentCell.x - 1, currentCell.y] == 1)
            {
                neighbors.Add(new Vector2Int(currentCell.x - 1, currentCell.y));
            }
            if (currentCell.x < width - 1 && maze[currentCell.x + 1, currentCell.y] == 1)
            {
                neighbors.Add(new Vector2Int(currentCell.x + 1, currentCell.y));
            }
            if (currentCell.y > 0 && maze[currentCell.x, currentCell.y - 1] == 1)
            {
                neighbors.Add(new Vector2Int(currentCell.x, currentCell.y - 1));
            }
            if (currentCell.y < height - 1 && maze[currentCell.x, currentCell.y + 1] == 1)
            {
                neighbors.Add(new Vector2Int(currentCell.x, currentCell.y + 1));
            }

            if (neighbors.Count != 0 && currentCell != goal)
            {
                stack.Push(new Vector2Int(currentCell.x, currentCell.y));
                int nextCell = Random.Range(0, neighbors.Count);
                currentCell = neighbors[nextCell];
            }
            else if (currentCell != goal)
            {
                Vector2Int cell = stack.Pop();
                currentCell = cell;
            }
            if (currentCell == goal)
            {
                stack.Push(currentCell);
                break;
            }
        }

        path = stack.ToArray();
        System.Array.Reverse(path);

        mazeSolved = true;
    }
}