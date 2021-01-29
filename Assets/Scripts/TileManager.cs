using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//TODO! Create a "gamemanager" to manage some of this stuff

public enum Terrain {
    four,
    t,
    straight,
    bent,
    ritual
}

public class TileManager : MonoBehaviour {
    public static Dictionary<Terrain, string> terrainFiles = new Dictionary<Terrain, string>() {
        {Terrain.four, "path_four_way"},
        {Terrain.t, "path_t"},
        {Terrain.straight, "path_straight"},
        {Terrain.bent, "path_bent"},
        {Terrain.ritual, "ritual"}
    };

    static Vector2Int up = new Vector2Int(0, 1), right = new Vector2Int(1, 0), down = new Vector2Int(0, -1), left = new Vector2Int(-1, 0);

    static public readonly List<Vector2Int> directions = new List<Vector2Int>() {up, right, down, left};

    public static Dictionary<Terrain, List<int>> terrainDirections = new Dictionary<Terrain, List<int>>() {
        {Terrain.four, new List<int>() {0, 1, 2, 3}},
        {Terrain.t, new List<int>() {1, 2, 3}},
        {Terrain.straight, new List<int>() {0, 2}},
        {Terrain.bent, new List<int>() {1, 2}},
        {Terrain.ritual, new List<int>()}
    };
    
    public static Dictionary<Terrain, Sprite> terrainSprites = new Dictionary<Terrain, Sprite> ();
    List<List<GameObject>> tiles = new List<List<GameObject>>();
    List<Terrain> terrains = new List<Terrain>();
    
    public int length = 9, width = 9;
    public float spacing = 1f; // makes it easier to keep it 1...

    List<GameObject> agents = new List<GameObject>();

    public static Sprite GetSpriteOfTerrain (Terrain t) {
        return terrainSprites[t];
    }

    public static List<int> GetDirectionsOfTerrain (Terrain t) {
        return new List<int> (terrainDirections[t]);
    }

    public Terrain GetRandomTerrain () {
        return terrains[Random.Range(0, terrains.Count-1)];
    }

    public Vector2Int GetRitualLocation () {
        return new Vector2Int(width/2, length/2);
    }

    public GameTile GetTileAt (Vector2Int v) {
        if (v.x >= 0 && v.x < width && v.y >= 0 && v.y < length)
            return tiles[v.x][v.y].GetComponent<GameTile>();
        else {
            return null;
        }
    }

    public List<Vector2Int> GetNeighborsAt (Vector2Int v) {
        return GetTileAt(v).GetDirections().Select(
            x => v + directions[x]).Where(
                x => !(GetTileAt(x) is null)).ToList();
    }


    public Vector3 GridToActual (Vector2Int v) { 
        return new Vector3(v.x, v.y, 0); //TODO!!! 
    }

    public Vector2Int ActualToGrid (Vector3 v) {
        var pos = transform.position;
        return new Vector2Int((int) (v.x - pos.x), (int) (v.y - pos.y));
    }

    //This is a dumb method.
    // > yes the location is already associated with the gametile (.location)

    public void SetTileLocation (GameTile tile, Vector2Int v) {
        tile.UpdateLocation(v);
        tiles[v.x][v.y] = tile.gameObject;
        tile.gameObject.transform.position = GridToActual(tile.GetLocation());
    }

    public List<Vector2Int> GetAgentLocations () {
        return agents.Select(x => x.GetComponent<AgentManager>().position).ToList();
    }

    //Swap the position of two passed in tiles
    void SwapTiles(GameTile first, GameTile second)
    {
        Vector2Int firstCoord = first.GetLocation(); 
        Vector2Int secondCoord = second.GetLocation(); 

        SetTileLocation(first, secondCoord);
        SetTileLocation(second, firstCoord);
    }

    void Start() {
        // load in the sprites
        foreach (var t in terrainFiles) {
            terrainSprites[t.Key] = (Sprite) Resources.Load("Sprites/" + t.Value, typeof(Sprite));
        }

        terrains = System.Enum.GetValues(typeof(Terrain)).OfType<Terrain>().ToList();

        // populate the map of tiles with the prefabs
        for (int i = 0; i < width; i++) {
            var row = new List<GameObject>();
            for (int j = 0; j < length; j++) {
                Terrain t = GetRandomTerrain();

                // the center is a ritual
                if (width/2 == i && length/2 == j)
                    t = Terrain.ritual;

                row.Add(GameTile.Create(t, i, j, gameObject));
            }
            tiles.Add(row);
        }

        // create a hunter
        agents.Add(AgentManager.Create(AgentType.hunter, 0, 0, gameObject));
    }

    GameObject swapTile = null;
    public void SwapThis (GameObject o) {
        Debug.Log("swapping");
        if (swapTile is null)
            swapTile = o;
        else {
            SwapTiles(swapTile.GetComponent<GameTile>(), o.GetComponent<GameTile>());
            swapTile = null;
        }
    }

    void Update () {
        // ...
    }
}
