using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformGenerator : Singleton<PlatformGenerator>
{
    private PlatformManager _platformManager;

	// Grid representation of IDs for texture placement.
    private int[,] map;
    private bool mapFiltered;
    private int width, height;
	// Number of cells in each room.
    public int roomWidth, roomHeight;

    // IDs used for representing borders and platforms from the textures in PlatformManager.
    public int topLayerID;
    public int lowerLayerID;

    public class Room
    {
        // The position in the larger grid of rooms
        public int gridX, gridY;

        // cell positions in the micro level map
        public int x, y, w, h;
        public int x2, y2;
        public Vector2Int center;

        private bool randSet = false;
        public Vector2Int randPoint;

        public Room(int gridX, int gridY, int w, int h)
        {
            this.gridX = gridX;
            this.gridY = gridY;
            x = gridX * w;
            y = gridY * h;
            this.w = w;
            this.h = h;

            x2 = x + w;
            y2 = y + h;

            center = new Vector2Int((x + x2) / 2,
                (y + y2) / 2);
            randPoint = new Vector2Int(-1, -1);
        }

		// Sets the position on the grid and then calculates the rooms actual map bounds
        public void setGridPos(int gridX, int gridY)
        {
            this.gridX = gridX;
            this.gridY = gridY;
            x = gridX * w;
            y = gridY * h;
            x2 = x + (w - 1);
            y2 = y + (h - 1);

            center = new Vector2Int((x + x2) / 2,
                (y + y2) / 2);
        }

		// Gets a random point inside the room. This is only generated
		// the first time it is called, and then the value is reused after.
		// The bounds are shifted to not be within 2 of the edge of the room.
        public Vector2Int getRandPoint()
        {
            if (!randSet)
            {
                randSet = true;
                randPoint.x = Random.Range(x + 2, x2 - 2);
                randPoint.y = Random.Range(y + 2, y2 - 2);
            }
            return randPoint;
        }

		// Tests if the given X and Y are inside the room.
        public bool isPointInRoom(int testX, int testY)
        {
            return testX >= x && testX <= x2 && testY >= y && testY <= y2;
        }
    }

	// The min/max range of room positions. This will vary the size of the entire map
    public int minRoomX, maxRoomX, minRoomY, maxRoomY;

	// A list of all generated rooms
    public List<Room> rooms;
	// The room at the end of the sequence as the goal.
    public Vector2Int goalRoom;
	// The room where the player spawns.
    public Vector2Int startRoom;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        mapFiltered = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            //Debug.Log("Starting Platform Generation...");
            PlatformManager.instance.destroyLevel();
            //populateRooms();
            createMapFromRooms();
            createSolutionPath();
            printMap();
            spawnMap();
            //Debug.Log("Platform Generation Complete!");
        }
    }

	// Creates a list of rooms by choosing a goal room to get to with start room set to 0,0.
	// Finds a sequence of rooms using findPath() to create a sequence stored in rooms.
    public void populateRooms()
    {
        //Random.InitState(System.DateTime.Now.Millisecond);

        rooms = new List<Room>();
        startRoom = new Vector2Int(0, 0);
        if (minRoomX == startRoom.x && minRoomY == startRoom.y && maxRoomX == startRoom.x && maxRoomY == startRoom.y)
        {
            Debug.Log("ERROR: min/max Room values must be not all be equal to the start point.");
            return;
        }
        goalRoom = randomRangeVector2Except(new Vector2Int(minRoomX, minRoomY), new Vector2Int(maxRoomX, maxRoomY), startRoom);
        int minDistance = (int)(Mathf.Abs(startRoom.x - goalRoom.x) + Mathf.Abs(startRoom.y - goalRoom.y));
        int maxDistance = (int)((minDistance + 2) * 1.4f);
        print("Start: " + startRoom + " minDistance: " + minDistance + " maxDistance: " + maxDistance + " Goal: " + goalRoom);
        findPath(startRoom.x, startRoom.y, minDistance, maxDistance);
        printRoomSequence();
        printRooms();
        fixRoomOffset();
        printRoomSequence();
    }

	// Finds a random position excluding the specified "except".
    private Vector2Int randomRangeVector2Except(Vector2Int min, Vector2Int max, Vector2Int except)
    {
        Vector2Int result = min;
        if (min.x == max.x && min.y == max.y) return result;
        do
        {
            result = new Vector2Int(Random.Range(min.x, max.x + 1), Random.Range(min.y, max.y + 1));
        } while (result.x == except.x && result.y == except.y);
        return result;
    }

	// Returns true, if the position x, y is avaliable as a room to move to.
	// Verifies it is a valid room on the grid and then checks all rooms created so far.
    private bool isOpen(int x, int y)
    {
        if (x > maxRoomX || x < minRoomX || y > maxRoomY || y < minRoomY) return false;

        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].gridX == x && rooms[i].gridY == y)
            {
                return false;
            }
        }
        return true;
    }

	// Finds the minimum X and Y rooms then offsets all the rooms using these values.
    private void fixRoomOffset()
    {
        int minPosX = rooms[0].gridX, minPosY = rooms[0].gridY;
        for (int i = 1; i < rooms.Count; i++)
        {
            if (rooms[i].gridX < minPosX) minPosX = rooms[i].gridX;
            if (rooms[i].gridY < minPosY) minPosY = rooms[i].gridY;
        }

        int offsetX = -minPosX;
        int offsetY = -minPosY;
        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].setGridPos(rooms[i].gridX + offsetX, rooms[i].gridY + offsetY);
        }
    }

	// Prints a line to Debug.Log with the rooms showing grid coordinates.
    private void printRoomSequence()
    {
        if (rooms == null || rooms.Count == 0)
        {
            Debug.Log("Tried to print empty room list!");
            return;
        }

        string result = "Rooms:\n";
        foreach (Room room in rooms)
        {
            result += "(" + room.gridX + "," + room.gridY + ")";
        }
        Debug.Log(result);
    }

	// Generates a grid of 0s based on the min/max extents of rooms for the size.
	// Then inserts a sequence of 1 to N showing the sequence of rooms from the start to the goal.
    private void printRooms()
    {
        int minPosX = rooms[0].gridX, minPosY = rooms[0].gridY, maxPosX = rooms[0].gridX, maxPosY = rooms[0].gridY;
        for (int i = 1; i < rooms.Count; i++)
        {
            if (rooms[i].gridX < minPosX) minPosX = rooms[i].gridX;
            if (rooms[i].gridX > maxPosX) maxPosX = rooms[i].gridX;
            if (rooms[i].gridY < minPosY) minPosY = rooms[i].gridY;
            if (rooms[i].gridY > maxPosY) maxPosY = rooms[i].gridY;
        }

        int offsetX = -minPosX;
        int offsetY = -minPosY;

        int realWidth = maxPosX - minPosX + 1;
        int realHeight = maxPosY - minPosY + 1;

        int[,] roomMap = new int[realWidth, realHeight];
        for (int y = realHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < realWidth; x++)
            {
                roomMap[x, y] = 0;
            }
        }
        for (int i = 0; i < rooms.Count; i++)
        {
            roomMap[rooms[i].gridX + offsetX, rooms[i].gridY + offsetY] = i + 1;
        }

        string result = "Numbered Rooms:\n";
        for (int y = realHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < realWidth; x++)
            {
                result += roomMap[x, y] + "\t";
            }
            result += "\n";
        }
        Debug.Log(result);
    }

	// Gets a list of points adjacent to the specified x and y.
	// The list is randomly ordered.
    private List<Vector2Int> getAdjacentPoints(int x, int y)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        result.Add(new Vector2Int(x, y + 1));
        result.Add(new Vector2Int(x + 1, y));
        result.Add(new Vector2Int(x - 1, y));
        result.Add(new Vector2Int(x, y - 1));
        shuffleList(result);
        return result;
    }

	// Randomly shuffles elements between random points within the list.
    private void shuffleList(List<Vector2Int> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }

	// Searches for a path that goes from x, y to the goal room with constraints on the min/max distance travelled to reach it.
	// This is called recursively.
    private bool findPath(int x, int y, int minDistance, int maxDistance)
    {
        // Prevent path from going too far
        if (rooms.Count >= maxDistance) return false;
        // If the goal has been reached and the minimum distance has been satisfied
        if (x == goalRoom.x && y == goalRoom.y && rooms.Count >= minDistance)
        {
            rooms.Add(new Room(x, y, roomWidth, roomHeight));
            return true;
        }
        // If the point is within the min/max x/y and the point is not already used
        if (!isOpen(x, y)) { return false; }
        // Mark as part of the possible solution path
        rooms.Add(new Room(x, y, roomWidth, roomHeight));

        // Get a random ordered list with the adjacent cell positions
        List<Vector2Int> nextPosList = getAdjacentPoints(x, y);
        foreach (Vector2Int nextPos in nextPosList)
        {
            // Test path continued in a random direction
            if (findPath(nextPos.x, nextPos.y, minDistance, maxDistance))
                return true;
        }

        // This point was not part of the solution path
        rooms.RemoveAt(rooms.Count - 1);
        return false;
    }

	// Creates a map with width and height with "1"s on the border and filled with 0s otherwise.
    public void createMap(int width, int height)
    {
        mapFiltered = false;
        this.width = width;
        this.height = height;
        map = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = 0;
                }
            }
        }
    }

	// Creates a map by calculating the width/height of the map.
	// Then filling it with 0s and populating each room into the map individually.
    public void createMapFromRooms()
    {
        int minPosX = rooms[0].gridX, minPosY = rooms[0].gridY, maxPosX = rooms[0].gridX, maxPosY = rooms[0].gridY;
        for (int i = 1; i < rooms.Count; i++)
        {
            if (rooms[i].gridX < minPosX) minPosX = rooms[i].gridX;
            if (rooms[i].gridX > maxPosX) maxPosX = rooms[i].gridX;
            if (rooms[i].gridY < minPosY) minPosY = rooms[i].gridY;
            if (rooms[i].gridY > maxPosY) maxPosY = rooms[i].gridY;
        }

        int offsetX = -minPosX;
        int offsetY = -minPosY;

        int realWidth = (maxPosX - minPosX + 1) * roomWidth;
        int realHeight = (maxPosY - minPosY + 1) * roomHeight;

        mapFiltered = false;
        this.width = realWidth;
        this.height = realHeight;
        PlatformManager.instance.width = realWidth;
        PlatformManager.instance.height = realHeight;

        map = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = 0;
            }
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            createRoomBorder(i);
        }
    }

	// Calculates positions on the map to place the room and adds in borders to the room with 1s if there is a border
    private void createRoomBorder(int index)
    {
        // Create room borders
        Vector2Int roomBeforeOffset = getOffsetFromRoom(index, index - 1);
        Vector2Int roomAfterOffset = getOffsetFromRoom(index, index + 1);
        if (roomBeforeOffset.x != 1 && roomAfterOffset.x != 1) // right
        {
            for (int y = rooms[index].y; y <= rooms[index].y2; y++)
            {
                map[rooms[index].x2, y] = 1;
            }
        }
        if (roomBeforeOffset.x != -1 && roomAfterOffset.x != -1) // left
        {
            for (int y = rooms[index].y; y <= rooms[index].y2; y++)
            {
                map[rooms[index].x, y] = 1;
            }
        }
        if (roomBeforeOffset.y != -1 && roomAfterOffset.y != -1) // up
        {
            for (int x = rooms[index].x; x <= rooms[index].x2; x++)
            {
                map[x, rooms[index].y2] = 1;
            }
        }
        if (roomBeforeOffset.y != 1 && roomAfterOffset.y != 1) // down
        {
            for (int x = rooms[index].x; x <= rooms[index].x2; x++)
            {
                map[x, rooms[index].y] = 1;
            }
        }
    }

	// Randomly adds platforms to create a sequence of platforms moving toward the goal
    public void createSolutionPath()
    {
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            Vector2Int p1 = rooms[i].getRandPoint();
            Vector2Int p2 = rooms[i + 1].getRandPoint();
            Vector2Int c = new Vector2Int(p1.x, p1.y);

            //map[p1.x, p1.y] = 1;
            //[p2.x, p2.y] = 1;

            while (c.x != p2.x || c.y != p2.y)
            {
                if (Random.Range(0, 2) == 0)
                {
                    addPlatform(new Vector2Int(c.x, c.y), new Vector2Int(p2.x, c.y));
                    c.x = p2.x;
                }
                else
                {
                    addPlatform(new Vector2Int(c.x, c.y), new Vector2Int(c.x, p2.y));
                    c.y = p2.y;
                }
            }
        }

        Vector2Int startRoomPos = rooms[0].getRandPoint();
        addPlatform(new Vector2Int(startRoomPos.x - 2, startRoomPos.y), new Vector2Int(startRoomPos.x + 2, startRoomPos.y));
    }

	// Calculates a position to spawn the player at in the start room randomly by trying to check in a range around that point for an empty position.
    public Vector2Int findPlayerSpawn()
    {
        Vector2Int startRoomPos = rooms[0].getRandPoint();
        for (int y = startRoomPos.y + 1; y <= startRoomPos.y + 3; y++)
        {
            for (int x = startRoomPos.x - 2; x <= startRoomPos.x + 2; x++)
            {
                if (map[x, y] == 0) return new Vector2Int(x, y);
            }
        }
        Debug.Log("Player spawn overrided and spawning in middle. This would be an error...");
        return rooms[0].center;
    }

	// Adds a platform to the map given the start/end by placing 1s on the map
    private void addPlatform(Vector2Int start, Vector2Int end)
    {
        Vector2Int diff = end - start;
        if (diff.x != 0) diff.x = diff.x / Mathf.Abs(diff.x);
        if (diff.y != 0) diff.y = diff.y / Mathf.Abs(diff.y);
        int rand = Random.Range(0, 3);
        while (start.x != end.x || start.y != end.y)
        {
            if (diff.x != 0 || start.y % 3 == rand) map[start.x, start.y] = 1;
            //if (diff.y != 0) map[start.x, start.y] = 1;
            //else if (start.y % 3 == 0) map[start.x, start.y] = 1;
            start = start + diff;
        }
    }

	// Gets the direction offset of the room where X(1=right, -1=left), Y(1=up,-1=down)
    private Vector2Int getOffsetFromRoom(int index, int otherRoom)
    {
        Vector2Int result = new Vector2Int(0, 0);
        if (otherRoom < 0 || otherRoom >= rooms.Count) return result;
        result.x = rooms[otherRoom].gridX - rooms[index].gridX; // 1 = right, -1 = left
        result.y = rooms[index].gridY - rooms[otherRoom].gridY; // 1 = up, -1 = down
        return result;
    }

	// Converts map for every position from 1 to either lowerLayerID or topLayerID.
	// This is used to convert from a collision view of the map to a textureID mapped one.
    public void applyIDMapping()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 0) continue;

                if (y == height - 1 || map[x, y + 1] == 1)
                {
                    map[x, y] = lowerLayerID;
                }
                else
                {
                    map[x, y] = topLayerID;
                }
            }
        }
        mapFiltered = true;
    }

	// Iterates over every position in the map and spawns platforms using their texture IDs.
    public void spawnMap()
    {
        if (_platformManager == null)
        {
            _platformManager = PlatformManager.Instance;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] > 0)
                {
                    _platformManager.spawnPlatform(map[x, y], x, y);
                }
            }
        }
    }

	// Spawns the objectID some chanceToSpawn times in every room.
    public void spawnObjectsInRooms(int objectID, float chanceToSpawn)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            spawnObjectsInRoom(i, objectID, chanceToSpawn);
        }
    }

	// Spawns objects randomly by finding positions near the platform route
    public void spawnObjectsInRoom(int roomID, int objectID, float chanceToSpawn)
    {
        Room r = rooms[roomID];
        for (int x = r.x + 2; x < r.x2 - 2; x++)
        {
            for (int y = r.y + 2; y < r.y2 - 2; y++)
            {
                if (map[x, y] == 0 && (map[x - 1, y - 1] == 1 || map[x, y - 1] == 1 || map[x + 1, y - 1] == 1) && Random.value < chanceToSpawn)
                {
                    PlatformManager.instance.spawnObject(objectID, x, y);
                }
            }
        }
    }

	// Spawns any random valid objects in the room.
    public void spawnRandomObjectsInRooms(float chanceToSpawn)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            spawnRandomObjectsInRoom(i, chanceToSpawn);
        }
    }

	// Spawns objects randomly by finding positions near the platform route
	// Objects are selected randomly from the available options.
    public void spawnRandomObjectsInRoom(int roomID, float chanceToSpawn)
    {
        Room r = rooms[roomID];
        for (int x = r.x + 2; x < r.x2 - 2; x++)
        {
            for (int y = r.y + 2; y < r.y2 - 2; y++)
            {
                if (map[x, y] == 0 && (map[x - 1, y - 1] == 1 || map[x, y - 1] == 1 || map[x + 1, y - 1] == 1) && Random.value < chanceToSpawn)
                {
                    PlatformManager.instance.spawnObject(Random.Range(0, PlatformManager.instance.objectPrefabs.Count), x, y);
                }
            }
        }
    }

	// Prints the map by iterating over the grid and printing it out.
    public void printMap()
    {
        string result = "Map Filtered: " + mapFiltered + "\n";
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                result += map[x, y];
            }
            result += "\n";
        }
        Debug.Log(result);
    }
}
