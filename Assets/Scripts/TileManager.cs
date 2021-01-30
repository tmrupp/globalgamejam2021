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
    public bool doublyConnected;

    public List<GameObject> agents = new List<GameObject>();

    public bool ResolvingMovement = false;

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
        return GetNeighborsAtNoBounds(v).Where(
                x => !(GetTileAt(x) is null)).ToList();
    }

    public List<Vector2Int> GetNeighborsAtNoBounds (Vector2Int v) {
        if (!doublyConnected) {
            if (GetTileAt(v) is null)
                return new List<Vector2Int>();
            else
                return GetTileAt(v).Directions.Select(
                    x => v + directions[x]).ToList();
        } else {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            if (GetTileAt(v) is null)
                return neighbors;
            else {
                foreach (var n in GetTileAt(v).Directions.Select(x => v + directions[x])) {
                    Debug.Log("n=" + n.ToString());
                    if (n == GetRitualLocation() || GetEdges().Contains(n) || ((GetTileAt(n) != null) && GetTileAt(n).Directions.Select(x => n + directions[x]).Contains(v))) 
                        neighbors.Add(n);
                }
            }

            return neighbors;
        }
    }

    public List<Vector2Int> GetEdges () {
        List<Vector2Int> edges = new List<Vector2Int>();

        for (int i = -1; i <= width; i++) {
            edges.Add(new Vector2Int(i, -1));
            edges.Add(new Vector2Int(i, length));
        }

        for (int i = 0; i < width; i++) {
            edges.Add(new Vector2Int(-1, i));
            edges.Add(new Vector2Int(width, i));
        }

        // edges.ForEach(x => Debug.Log(x.ToString()));

        return edges;
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
        tile.Location = v;
        tiles[v.x][v.y] = tile.gameObject;
        tile.gameObject.transform.position = GridToActual(tile.Location);
    }

    public List<(AgentType, Vector2Int)> GetAgentLocations () {
        return agents.Select(
            x => x.GetComponent<AgentManager>()).Select(
                x => (x.agentType, x.position)).ToList();
    }

    //Swap the position of two passed in tiles
    void SwapTiles(GameTile first, GameTile second)
    {
        Vector2Int firstCoord = first.Location; 
        Vector2Int secondCoord = second.Location; 

        SetTileLocation(first, secondCoord);
        SetTileLocation(second, firstCoord);

        ResolveAllAgentsMovement();
    }

    void ResolveAllAgentsMovement()
    {
        ResolvingMovement = true;

        //hunters move, then victims move, then monsters move
        agents.Sort((GameObject lhs, GameObject rhs) => {
            AgentManager am_lhs = lhs.GetComponent<AgentManager>();
            AgentManager am_rhs = rhs.GetComponent<AgentManager>();

            switch(am_lhs.agentType)
            {
                case AgentType.hunter:
                    if (am_rhs.agentType == AgentType.hunter) return 0;
                    else return -1;
                case AgentType.victim:
                    if (am_rhs.agentType == AgentType.hunter) return 1;
                    if (am_rhs.agentType == AgentType.victim) return 0;
                    if (am_rhs.agentType == AgentType.monster) return -1;
                    break;
                case AgentType.monster:
                    if (am_rhs.agentType == AgentType.monster) return 0;
                    else return 1;
            }

            return 0;
        });

        foreach (var agent in agents.ToList())
        {
            agent.GetComponent<AgentManager>().Move();
        }

        ResolvingMovement = false;
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

        //Debug: take stock of tiles we've created
        Dictionary<string, int> uniqueTiles = new Dictionary<string, int>();
        foreach (var row in tiles)
        {
            foreach (var tile in row)
            {
                GameTile gt = tile.GetComponent<GameTile>();
                if (gt.Terrain != Terrain.ritual)
                {
                    string uniqueString = terrainFiles[gt.Terrain] + ":" + (gt.Directions.Sum() + gt.Directions.Aggregate<int>((product, next) => product *= next));
                    if (uniqueTiles.ContainsKey(uniqueString))
                    {
                        uniqueTiles[uniqueString]++;
                    }
                    else
                    {
                        uniqueTiles.Add(uniqueString, 1);
                    }
                }
            }
        }
        string outstring = string.Empty;
        foreach (var tile in uniqueTiles)
        {
            outstring += tile.Key + ":\t\t" + tile.Value + "\n";
        }
        Debug.Log(outstring);

        // create a hunter
        agents.Add(AgentManager.Create(AgentType.hunter, 0, 0, gameObject));
        agents.Add(AgentManager.Create(AgentType.victim, 3, 5, gameObject));
        agents.Add(AgentManager.Create(AgentType.monster, 5, 5, gameObject));
    }

    GameObject swapTile = null;
    public void SwapThis (GameObject o) {
        if (swapTile is null)
            swapTile = o;
        else {
            SwapTiles(swapTile.GetComponent<GameTile>(), o.GetComponent<GameTile>());
            swapTile = null;
        }
    }

    public void RemoveAgent (GameObject a) {
        agents.Remove(a);
    }

    void Update () {
        // ...
    }
}
