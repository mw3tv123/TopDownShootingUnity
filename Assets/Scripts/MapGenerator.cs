using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

public class MapGenerator : MonoBehaviour {
    [Serializable]
    public struct Coordinate : IEquatable<Coordinate> {
        [SerializeField]
        private int _x;
        public int X { get => _x; set => _x = value; }

        [SerializeField]
        private int _y;
        public int Y { get => _y; set => _y = value; }

        public static bool operator == (Coordinate a, Coordinate b) {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator != (Coordinate a, Coordinate b) {
            return a.X != b.X || a.Y != b.Y;
        }
        public bool Equals (Coordinate other) => this == other;

        public override bool Equals (object obj) {
            return obj is Coordinate other && Equals (other);
        }

        public override int GetHashCode () {
            return (X * 397) ^ Y;
        }
    }

    public Transform TitlePrefab;
    public Transform ObstaclePrefab;
    public Transform NavMeshFloor;
    public Transform WallPrefab;
    public Map[] Maps;
    public int MapIndex;
    public float TileSize;
    [Range (0, 1)]
    public float OutLine;

    private Map currentMap;
    private Transform[,] tilesMap;
    private Transform mapHolder;
    private List<Coordinate> allTileCoordinates;
    private Queue<Coordinate> shuffledTileCoordinates;
    private Queue<Coordinate> shuffledOpenTileCoordinates;

    public void GenerateMap () {
        CreateMapHolderObject ();
        InitialiseTileCoordinate ();
        InitialiseTilesPosition ();
        GenerateRandomObstacle ();
        SetNavMeshFloorSize ();
        GenerateWalls ();
    }

    public Transform GetTileFromPosition (Vector3 position) {
        int x = Mathf.RoundToInt (position.x / TileSize + (currentMap.MapSize.X - 1) / 2f);
        int y = Mathf.RoundToInt (position.z / TileSize + (currentMap.MapSize.Y - 1) / 2f);

        x = Mathf.Clamp (x, 0, tilesMap.GetLength (0) - 1);
        y = Mathf.Clamp (y, 0, tilesMap.GetLength (1) - 1);

        return tilesMap[x, y];
    }

    public Transform GetRandomOpenTile () {
        Coordinate randomCoordinate = shuffledOpenTileCoordinates.Dequeue ();
        shuffledOpenTileCoordinates.Enqueue (randomCoordinate);
        return tilesMap[randomCoordinate.X, randomCoordinate.Y];
    }

    private void Awake () {
        FindObjectOfType<Spawner> ().OnNewWave += OnNewWave;
    }

    private void OnNewWave (int waveNumber) {
        MapIndex = waveNumber - 1;
        GenerateMap ();
    }

    // Stack all objects (tiles, obstacles) under the object name "Generated Map"
    private void CreateMapHolderObject () {
        currentMap = Maps[MapIndex];
        tilesMap = new Transform[currentMap.MapSize.X, currentMap.MapSize.Y];
        GetComponent<BoxCollider> ().size = new Vector3 (currentMap.MapSize.X * TileSize, 0.05f, currentMap.MapSize.Y * TileSize);
        const string holderName = "Generated Map";
        if (transform.Find (holderName)) {
            if (Application.isPlaying)
                Destroy (transform.Find (holderName).gameObject);
            else
                DestroyImmediate (transform.Find (holderName).gameObject);
        }

        mapHolder = new GameObject (holderName).transform;
        mapHolder.parent = transform;
    }

    private void InitialiseTileCoordinate () {
        allTileCoordinates = new List<Coordinate> ();
        for (int x = 0; x < currentMap.MapSize.X; x++) {
            for (int y = 0; y < currentMap.MapSize.Y; y++) {
                allTileCoordinates.Add (new Coordinate { X = x, Y = y });
            }
        }
        shuffledTileCoordinates = new Queue<Coordinate> (
            Utility.ShuffleArray (allTileCoordinates.ToArray (), currentMap.Seed)
        );
    }

    private void InitialiseTilesPosition () {
        for (int x = 0; x < currentMap.MapSize.X; x++) {
            for (int y = 0; y < currentMap.MapSize.Y; y++) {
                Transform newTile = InitialisePrefab (
                    TitlePrefab,
                    // Choose position for new tile
                    GetPositionFromCoordinate (x, y),
                    // Instantiate new tile at the chosen position and rotate 90* by X axis
                    Quaternion.Euler (Vector3.right * 90),
                    // To create outline, simply scale the tile smaller by variable [Outline]
                    // [Outline] = 0 : Full scale, no outline
                    // [Outline] = 1 : Full outline, no scale
                    Vector3.one * (1 - OutLine) * TileSize
                );
                tilesMap[x, y] = newTile;
            }
        }
    }

    private void GenerateRandomObstacle () {
        Random pseudoRandomGenerator = new Random (currentMap.Seed);
        bool[,] obstaclesMap = new bool[currentMap.MapSize.X, currentMap.MapSize.Y];
        int obstacleCount = (int) (currentMap.ObstacleDensity * currentMap.MapSize.X * currentMap.MapSize.Y);
        int currentoObstacleCount = 0;
        List<Coordinate> allOpenCoordinates = new List<Coordinate> (allTileCoordinates);

        for (int i = 0; i < obstacleCount; i++) {
            Coordinate randomCoordinate = GetRandomCoordinate ();
            obstaclesMap[randomCoordinate.X, randomCoordinate.Y] = true;
            currentoObstacleCount++;

            if (randomCoordinate != currentMap.MapCenter && MapIsFullyAccessible (obstaclesMap, currentoObstacleCount)) {
                Vector3 obstaclePosition = GetPositionFromCoordinate (randomCoordinate.X, randomCoordinate.Y);
                float obstacleHeight = Mathf.Lerp (
                    Mathf.Clamp (
                        currentMap.MinObstacleHeight,
                        0.1f,
                        currentMap.MaxObstacleHeight
                    ),
                    currentMap.MaxObstacleHeight,
                    (float) pseudoRandomGenerator.NextDouble ()
                );
                Transform newObstacle = InitialisePrefab (
                    ObstaclePrefab,
                    obstaclePosition + Vector3.up * obstacleHeight / 2,
                    Quaternion.identity,
                    new Vector3 ((1 - OutLine) * TileSize, obstacleHeight, (1 - OutLine) * TileSize)
                );
                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer> ();
                Material obstacleMaterial = new Material (obstacleRenderer.sharedMaterial) {
                    color = currentMap.ObstacleColor
                };
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenCoordinates.Remove (randomCoordinate);
            }
            else {
                obstaclesMap[randomCoordinate.X, randomCoordinate.Y] = false;
                currentoObstacleCount--;
            }
        }
        shuffledOpenTileCoordinates = new Queue<Coordinate> (
            Utility.ShuffleArray (
                allOpenCoordinates.ToArray (),
                currentMap.Seed)
            );
    }

    private Vector3 GetPositionFromCoordinate (int x, int y) => new Vector3 (
        - currentMap.MapSize.X / 2f + 0.5f + x,
        0,
        - currentMap.MapSize.Y / 2f + 0.5f + y
    ) * TileSize;

    private Coordinate GetRandomCoordinate () {
        Coordinate randomCoordinate = shuffledTileCoordinates.Dequeue ();
        shuffledTileCoordinates.Enqueue (randomCoordinate);
        return randomCoordinate;
    }

    private Transform InitialisePrefab (Transform prefab, Vector3 position, Quaternion rotation, Vector3 scale) {
        Transform newInstance = Instantiate (prefab, position, rotation, mapHolder);
        newInstance.localScale = scale;
        return newInstance;
    }

    /// <summary>
    /// Implement Flood-Fill algorithm to check all non-obstacle tiles to if it accessible
    /// </summary>
    /// <param name="obstaclesMap"></param>
    /// <param name="currentObstacleCount"></param>
    /// <returns><b>True</b> if all non-obstacle tiles are accessible, otherwise return false</returns>
    private bool MapIsFullyAccessible (bool[,] obstaclesMap, int currentObstacleCount) {
        bool[,] mapFlags = new bool[obstaclesMap.GetLength (0), obstaclesMap.GetLength (1)];
        mapFlags[currentMap.MapCenter.X, currentMap.MapCenter.Y] = true;

        Queue<Coordinate> queue = new Queue<Coordinate> ();
        queue.Enqueue (currentMap.MapCenter);

        int accessibleTileCount = 1;

        while (queue.Count > 0) {
            Coordinate tile = queue.Dequeue ();
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    int neighbourX = tile.X + x;
                    int neighbourY = tile.Y + y;
                    if (x == 0 ^ y == 0) {
                        // Only run below codes if tile is:
                        // - West:  x = -1 | y =  0
                        // - East:  x =  1 | y =  0
                        // - North: x =  0 | y =  1 
                        // - South: x =  0 | y = -1
                        if ( // Check if tile coordinate is out of map
                            neighbourX >= 0 && neighbourX < obstaclesMap.GetLength (0) &&
                            neighbourY >= 0 && neighbourY < obstaclesMap.GetLength (1)
                        ) {
                            if ( // Check if tile is not obstacle
                                !mapFlags[neighbourX, neighbourY] &&
                                !obstaclesMap[neighbourX, neighbourY]
                            ) {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue (new Coordinate {
                                    X = neighbourX,
                                    Y = neighbourY
                                });
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }
        int targetAccessibleTiles = currentMap.MapSize.X * currentMap.MapSize.Y - currentObstacleCount;
        return targetAccessibleTiles == accessibleTileCount;
    }

    private void SetNavMeshFloorSize () {
        NavMeshFloor.localScale = new Vector3 (currentMap.MapSize.X + 0.25f, currentMap.MapSize.Y + 0.25f) * TileSize;
    }

    private void GenerateWalls () {
        Vector3 westWallPosition = Vector3.left * ((currentMap.MapSize.X * 2 + 2) / 4f * TileSize - 0.5f);
        Vector3 eastWallPosition = Vector3.right * ((currentMap.MapSize.X * 2 + 2) / 4f * TileSize - 0.5f);
        Vector3 northWallPosition = Vector3.forward * ((currentMap.MapSize.Y * 2 + 2) / 4f * TileSize - 0.5f);
        Vector3 southWallPosition = Vector3.back * ((currentMap.MapSize.Y * 2 + 2) / 4f * TileSize - 0.5f);

        Vector3 horizontalScale = new Vector3 (0.2f, 2f, currentMap.MapSize.Y + 0.4f) * TileSize;
        Vector3 verticalScale = new Vector3 (currentMap.MapSize.X, 2f, 0.2f) * TileSize;

        CreateWall (westWallPosition, horizontalScale);
        CreateWall (eastWallPosition, horizontalScale);
        CreateWall (northWallPosition, verticalScale);
        CreateWall (southWallPosition, verticalScale);
    }

    private void CreateWall (Vector3 position, Vector3 scale) {
        Instantiate (
            WallPrefab,
            position,
            Quaternion.identity,
            mapHolder
        )
        .localScale = scale;
    }

    [Serializable]
    public class Map {
        public Coordinate MapSize;
        [Range (0, 1)]
        public float ObstacleDensity;
        public int Seed;
        public float MinObstacleHeight;
        public float MaxObstacleHeight;
        public Color ObstacleColor;
        public Coordinate MapCenter => new Coordinate {
            X = MapSize.X / 2,
            Y = MapSize.Y / 2
        };
    }
}
