using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Direction
{
    right = 0,
    left = 1,
    down = 2, 
    up = 3
}

public class LevelGenerator : MonoBehaviour
{
    public Transform[] startingPositions;
    public GameObject[] rooms; // index 0 --> LR, index 1 --> LRB, index 2 --> LRT, index 3 --> LRBT
    public Transform levelParent;

    private Direction dir;
    public float moveAmount;
    public int maxRooms;
    public int nrSpawnedRooms = 0;

    private float timeBtwRoom;
    public float startTimeBtwRoom = 0.25f;

    private float minX;
    private float maxX;
    private float minY;
    private float maxY;
    public bool stopGeneration = true;

    public GameObject killRoom;
    public int nrKillzoneLayers;
    public bool levelGenFinished = false;
    private bool roomGenFinished = false;


    public LayerMask room;

    private int downCounter;
    private int upCounter;

    private List<Vector3> killPositions = new List<Vector3>();

    public void StartGeneration(int nrRooms)
    {
        maxRooms = nrRooms;

        // Start with a random StartRoom at a random StartPosition
        int randStartingPos = Random.Range(0, startingPositions.Length);
        transform.position = startingPositions[randStartingPos].position;
        SpawnRoom(4);

        AddOpenMovesToKillSpots(transform.position);

        // Calculate min/max positions based on MoveAmount & MaxRooms & startPos
        float maxOffset = (moveAmount * maxRooms / 2);
        minX = transform.position.x - maxOffset;
        maxX = transform.position.x + maxOffset;
        minY = transform.position.y - maxOffset;
        maxY = transform.position.y + maxOffset;

        // Pick a random cardinal direction (Everything is free at this point)
        dir = (Direction)Random.Range(0, 4);

        stopGeneration = false;
    }

    private void Update()
    {
        if (!stopGeneration)
        {
            if (timeBtwRoom <= 0)
            {
                Move();
                timeBtwRoom = startTimeBtwRoom;
            }
            else
            {
                timeBtwRoom -= Time.deltaTime;
            }
        }

        if (stopGeneration && !roomGenFinished)
        {
            roomGenFinished = true;
            Invoke("SpawnKillzones", 0.1f);
        }
    }

    private void Move()
    {
        if (dir == Direction.right)
        { // Move RIGHT!
            Vector2 newPos = new Vector2(transform.position.x + moveAmount, transform.position.y);

            downCounter = 0;
            upCounter = 0;

            transform.position = newPos;

            // Decide which roomType to spawn
            int rand = Random.Range(0, 4);
            SpawnRoom(rand);

            // Pick a new available direction
            List<Direction> avDirs = CheckAvailableMoves(dir);
            dir = avDirs[Random.Range(0, avDirs.Count)];
        }
        else if (dir == Direction.left)
        { // Move LEFT!
            Vector2 newPos = new Vector2(transform.position.x - moveAmount, transform.position.y);

            downCounter = 0;
            upCounter = 0;

            // Move the position of the LevelGen.
            transform.position = newPos;

            // Decide which roomType to spawn
            int rand = Random.Range(0, 4);
            SpawnRoom(rand);

            // Make sure we don't go back (if we come from right, we can't go left straight after.
            List<Direction> avDirs = CheckAvailableMoves(dir);
            dir = avDirs[Random.Range(0, avDirs.Count)];
        }
        else if (dir == Direction.down)
        { // Move DOWN!
            Vector2 newPos = new Vector2(transform.position.x, transform.position.y - moveAmount);
            downCounter++;

            // Check if the room we are in atm has a bottom opening
            // if not, destroy it and spawn one that has it
            Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, room);
            if (roomDetection)
            {
                RoomType roomType;
                roomDetection.TryGetComponent<RoomType>(out roomType);
                if (!roomType) Debug.LogWarning("No roomType detected");
                if (roomType.type != 1 && roomType.type != 3)
                {
                    // This fixes the bug where we move down twice in a row and create a barrier on the second motion
                    if (downCounter >= 2)
                    {
                        roomType.RoomDestruction();
                        nrSpawnedRooms--;
                        SpawnRoom(3);
                    }
                    else
                    {
                        roomType.RoomDestruction();
                        nrSpawnedRooms--;

                        int randBottomRoom = Random.Range(1, 4);
                        if (randBottomRoom == 2)
                        {
                            randBottomRoom = 1;
                        }
                        SpawnRoom(randBottomRoom);
                    }
                }
            }

            transform.position = newPos;

            // Decide which roomType to spawn
            int rand = Random.Range(2, 4);
            SpawnRoom(rand);

            List<Direction> avDirs = CheckAvailableMoves(dir);
            dir = avDirs[Random.Range(0, avDirs.Count)];

        }
        else if (dir == Direction.up)
        { // Move UP!
            Vector2 newPos = new Vector2(transform.position.x, transform.position.y + moveAmount);
            upCounter++;

            // Check if the room we are in atm has a top opening
            // if not, destroy it and spawn one that has it
            Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, room);
            if (roomDetection)
            {
                RoomType roomType = roomDetection.GetComponent<RoomType>();
                if (roomType.type != 2 && roomType.type != 3)
                {
                    // This fixes the bug where we move up twice in a row and create a barrier on the second motion
                    if (upCounter >= 2)
                    {
                        roomType.RoomDestruction();
                        nrSpawnedRooms--;
                        SpawnRoom(3);
                    }
                    else
                    {
                        roomType.RoomDestruction();
                        nrSpawnedRooms--;

                        int randTopRoom = Random.Range(2, 4);
                        SpawnRoom(randTopRoom);
                    }
                }
            }

            transform.position = newPos;

            // Decide which roomType to spawn
            int rand = Random.Range(1, 4);
            if (rand == 2) rand = 3;
            SpawnRoom(rand);

            List<Direction> avDirs = CheckAvailableMoves(dir);
            dir = avDirs[Random.Range(0, avDirs.Count)];
        }
    }

    void SpawnRoom(int roomIdx)
    {
        nrSpawnedRooms++;
        if (nrSpawnedRooms >= maxRooms)
        {
            stopGeneration = true;

            // Spawn end room
            Instantiate(rooms[5], transform.position, Quaternion.identity, levelParent);
        }
        else
        {
            Instantiate(rooms[roomIdx], transform.position, Quaternion.identity, levelParent);
        }

        // Remove the position we're spawning on from the list of killPositions
        killPositions.Remove(transform.position);
    }

    private bool isSpaceFree(Vector2 newPos)
    {
        Collider2D roomDetection = Physics2D.OverlapCircle(newPos, 1, room);
        return (roomDetection == null);
    }

    private List<Direction> CheckAvailableMoves(Direction currentDir)
    {
        List<Direction> possibleDirs = new List<Direction>();

        Vector2 newPos;

        // CHECK RIGHT
        if (currentDir != Direction.left)
        {
            newPos = new Vector2(transform.position.x + moveAmount, transform.position.y);
            if (!killPositions.Contains(newPos)) killPositions.Add(newPos);
            if (newPos.x < maxX)
            {
                if (isSpaceFree(newPos))
                {
                    possibleDirs.Add(Direction.right);
                }
                else
                {
                    killPositions.Remove(newPos);
                }
            }
        }

        // CHECK LEFT
        if (currentDir != Direction.right)
        {
            newPos = new Vector2(transform.position.x - moveAmount, transform.position.y);
            if (!killPositions.Contains(newPos)) killPositions.Add(newPos);
            if (newPos.x > minX)
            {
                if (isSpaceFree(newPos))
                {
                    possibleDirs.Add(Direction.left);
                }
                else
                {
                    killPositions.Remove(newPos);
                }
            }
        }

        // CHECK DOWN
        if (currentDir != Direction.up)
        {
            newPos = new Vector2(transform.position.x, transform.position.y - moveAmount);
            if (!killPositions.Contains(newPos)) killPositions.Add(newPos);
            if (newPos.y > minY)
            {
                if (isSpaceFree(newPos))
                {
                    possibleDirs.Add(Direction.down);
                }
                else
                {
                    killPositions.Remove(newPos);
                }
            }
        }
 
        // CHECK UP
        if (currentDir != Direction.down)
        {
            newPos = new Vector2(transform.position.x, transform.position.y + moveAmount);
            if (!killPositions.Contains(newPos)) killPositions.Add(newPos);
            if (newPos.y < maxY)
            {
                if (isSpaceFree(newPos))
                {
                    possibleDirs.Add(Direction.up);
                }
                else
                {
                    killPositions.Remove(newPos);
                }
            }
        }

        if (possibleDirs.Count == 0)
        {
            // pick a random position from the latest adjacent positions
            int rand = Random.Range(killPositions.Count - 5, killPositions.Count);

            transform.position = killPositions[rand];
            killPositions.RemoveAt(rand);
            possibleDirs = CheckAvailableMoves(currentDir);
        }

        return possibleDirs;
    }

    private void AddOpenMovesToKillSpots(Vector3 position)
    {
        Vector2 newPos;

        // CHECK RIGHT
        newPos = new Vector2(position.x + moveAmount, position.y);
        if (isSpaceFree(newPos))
        {
            if (!killPositions.Contains(newPos)) killPositions.Add(newPos);
        }

        // CHECK LEFT
        newPos = new Vector2(position.x - moveAmount, position.y);
        if (isSpaceFree(newPos))
        {
            if (!killPositions.Contains(newPos)) killPositions.Add(newPos);
        }

        // CHECK DOWN
        newPos = new Vector2(position.x, position.y - moveAmount);
        if (isSpaceFree(newPos))
        {
            if (!killPositions.Contains(newPos)) killPositions.Add(newPos);
        }

        // CHECK UP
        newPos = new Vector2(position.x, position.y + moveAmount);
        if (isSpaceFree(newPos))
        {
            if (!killPositions.Contains(newPos)) killPositions.Add(newPos);
        }
    }

    void SpawnKillzones()
    {
        int count = 0;

        for (int k = 0; k < nrKillzoneLayers; k++)
        {
            count = killPositions.Count;
            for (int i = 0; i < count; i++)
            {
                AddOpenMovesToKillSpots(killPositions[i]);
            }
        }

        for (int i = 0; i < killPositions.Count; i++)
        {
            GameObject inst = Instantiate(killRoom, killPositions[i], Quaternion.identity, levelParent);
        }

        levelGenFinished = true;
    }
}
