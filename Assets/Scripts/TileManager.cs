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
};

public class TileManager : MonoBehaviour {

    public static Dictionary<Terrain, List<(string, string)>> terrainFiles = new Dictionary<Terrain, List<(string, string)>>() {
        {Terrain.four, new List<(string, string)>() {("cross1", "cross2")}},

        {Terrain.ritual, new List<(string, string)>() {("ritual1", "ritual2")}},

        {Terrain.t, new List<(string, string)>() {
            ("tbotright1", "tbotright2"),
            ("tbotleft1", "tbotleft2"),
            ("ttopleft1", "ttopleft2"),
            ("ttopright2", "ttopright1")
        }},

        {Terrain.straight, new List<(string, string)>() {
            ("bar2.1", "bar2.2"),
            ("bar1.1", "bar1.2")
        }},

        {Terrain.bent, new List<(string, string)>() {
            ("cornerr1", "cornerr2"),
            ("cornerbot1", "cornerbot2"),
            ("cornerl2", "cornerl1"),
            ("cornertop1", "cornertop2")
        }},
    };

    // (terrain, rotation) => (front sprite, back sprite)
    public static Dictionary<Terrain, List<(Sprite, Sprite)>> terrainSprites = new Dictionary<Terrain, List<(Sprite, Sprite)>>();

    static Vector2Int up = new Vector2Int(0, 1), right = new Vector2Int(1, 0), down = new Vector2Int(0, -1), left = new Vector2Int(-1, 0);

    static public readonly List<Vector2Int> directions = new List<Vector2Int>() {up, right, down, left};

    public static Dictionary<Terrain, List<int>> terrainDirections = new Dictionary<Terrain, List<int>>() {
        {Terrain.four, new List<int>() {0, 1, 2, 3}},
        {Terrain.t, new List<int>() {1, 2, 3}},
        {Terrain.straight, new List<int>() {0, 2}},
        {Terrain.bent, new List<int>() {1, 2}},
        {Terrain.ritual, new List<int>()}
    };
    
    // public static Dictionary<Terrain, Sprite> terrainSprites = new Dictionary<Terrain, Sprite> ();
    List<List<GameObject>> tiles = new List<List<GameObject>>();
    public List<Terrain> terrains = new List<Terrain>();
    
    public int length = 9, width = 9;
    public float spacing = 1f; // makes it easier to keep it 1...
    public bool doublyConnected;

    public int points = 0;

    public int turnNumber = 0;
    public int victimCap = 1;
    public int hunterCap = 0;
    public int monsterCap = 0;
    private List<Vector2Int> candidates = new List<Vector2Int>();

    public List<GameObject> agents = new List<GameObject>();

    public bool ResolvingMovement = false;

    GameObject swapTile = null;
    GameObject secondSwapTile = null;
    bool shownShufflePopup;
    List<bool> spawnedYet;
    public List<Popup> popup = new List<Popup>();
    public Popup shufflePopup;
    public Tooltip tooltip;

    public ScreenSplatter ss; //set in Start via searching the hierarchy

    public static (Sprite, Sprite) GetSpriteOfTerrain (Terrain t, int rotation) {
        var ts = terrainSprites[t];
        return ts[rotation % ts.Count];
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
                    // Debug.Log("n=" + n.ToString());
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

    // width = 1.05f
    // height = 0.82f
    public Vector3 GridToActual (Vector2 v) { 
        float hw = 1.06f * 0.5f, hh = 0.61f * 0.5f;
        float x = v.x * hw - v.y * hw;
        float y = v.x * hh + v.y * hh;

        return new Vector3(x, y, 0); //TODO!!! 
    }

    public Vector2Int ActualToGrid (Vector3 v) {
        var pos = transform.position;
        return new Vector2Int((int) (v.x - pos.x), (int) (v.y - pos.y));
    }

    //This is a dumb method.
    // > yes the location is already associated with the gametile (.location)

    public IEnumerator<YieldInstruction> AnimateTileSwap (GameTile firstTile, GameTile secondTile, float speed) {
        var firstActual = GridToActual(firstTile.Location);
        var secondActual = GridToActual(secondTile.Location);
        var progress = 0f;
        while (progress < 1f)
        {
            yield return null;
            progress += speed * Time.deltaTime;
            firstTile.transform.position = Vector3.Lerp(firstActual, secondActual, progress);
            secondTile.transform.position = Vector3.Lerp(secondActual, firstActual, progress);
        }
        firstTile.transform.position = secondActual;
        secondTile.transform.position = firstActual;
        var temp = firstTile.Location;
        firstTile.Location = secondTile.Location;
        secondTile.Location = temp;
        tiles[firstTile.Location.x][firstTile.Location.y] = firstTile.gameObject;
        tiles[secondTile.Location.x][secondTile.Location.y] = secondTile.gameObject;
    }

    public List<(AgentType, Vector2Int)> GetAgentLocations () {
        return agents.Select(
            x => x.GetComponent<AgentManager>()).Select(
                x => (x.agentType, x.position)).ToList();
    }

    //Swap the position of two passed in tiles
    private IEnumerator<YieldInstruction> SwapTiles(GameTile first, GameTile second)
    {
        ResolvingMovement = true;
        Vector2Int firstCoord = first.Location;
        Vector2Int secondCoord = second.Location;
        yield return StartCoroutine(ResolveAllAgentsMovement());
        first.SetColor(Color.white);
        second.SetColor(Color.white);
        yield return StartCoroutine(AnimateTileSwap(first, second, 3f));
        ResolvingMovement = false;
        agents.ForEach(a => a.GetComponent<AgentManager>().FindNextMove());
    }

    private IEnumerator<YieldInstruction> ResolveAllAgentsMovement()
    {
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
            yield return StartCoroutine(agent.GetComponent<AgentManager>().Move());
        }
    }

    void SetTileAt (Vector2Int v, GameTile newTile) {
        tiles[v.x][v.y] = newTile.gameObject;
    }

    public void ReplaceGameTile (Vector2Int v, GameTile newTile) {
        var formerTile = GetTileAt(v);
        SetTileAt(v, newTile);
        Destroy(formerTile.gameObject);
    }

    Sprite LoadSprite (string name) {
        Sprite s = (Sprite) Resources.Load("Sprites/backgroundtiles/" + name, typeof(Sprite));
        return s;
    }

    private void Awake()
    {
        tooltip = FindObjectOfType<Tooltip>();
    }

    void Start() {
        spawnedYet = new List<bool>() { false, false, false };
        // load in the sprites
        foreach (var t in terrainFiles) {
            terrainSprites[t.Key] = t.Value.Select(x => (LoadSprite(x.Item1), LoadSprite(x.Item2))).ToList();
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
        
        for (int i = 1; i < 8; ++i)
        {
            for (int j = 1; j < 8; ++j)
            {
                if (Mathf.Abs(i - 4) > 1 && Mathf.Abs(j - 4) > 1) { candidates.Add(new Vector2Int(i, j)); }
            }
        }

        // create a hunter
        StartCoroutine(Spawn(AgentType.victim, 4, 2, gameObject));
        /*
        agents.Add(AgentManager.Create(AgentType.hunter, 0, 0, gameObject));
        agents.Add(AgentManager.Create(AgentType.hunter, 8, 8, gameObject));
        agents.Add(AgentManager.Create(AgentType.victim, 3, 5, gameObject));
        agents.Add(AgentManager.Create(AgentType.victim, 3, 3, gameObject));
        agents.Add(AgentManager.Create(AgentType.victim, 5, 5, gameObject));
        agents.Add(AgentManager.Create(AgentType.victim, 5, 3, gameObject));
        agents.Add(AgentManager.Create(AgentType.monster, 0, 8, gameObject));
        agents.Add(AgentManager.Create(AgentType.monster, 8, 0, gameObject));
        */
        ss = GameObject.FindObjectOfType<ScreenSplatter>();
    }

    public IEnumerator<YieldInstruction> Spawn(AgentType type, int x, int y, GameObject caller)
    {
        agents.Add(AgentManager.Create(type, x, y, caller));
        if (!spawnedYet[(int)type])
        {
            yield return StartCoroutine(popup[(int)type].Show());
            spawnedYet[(int)type] = true;
        }
        yield return null;
    }

    public GameObject GetSwapTile()
    {
        return swapTile;
    }

    public GameObject GetSecondSwapTile()
    {
        return secondSwapTile;
    }

    public IEnumerator<YieldInstruction> SwapThis (GameObject o) {
        o.GetComponent<GameTile>().SetColor(Color.red);
        if (swapTile is null) {
            swapTile = o;
        } else {
            if (swapTile == o) {
                swapTile = null;
                yield return null;
            } else {
                secondSwapTile = o;
                var st = swapTile.GetComponent<GameTile>();
                yield return StartCoroutine(SwapTiles(st, o.GetComponent<GameTile>()));
                ++turnNumber;
                if (turnNumber == 5) { ++hunterCap; }
                if (turnNumber == 7) { ++monsterCap; }
                if (turnNumber == 12) { ++victimCap; }
                if (turnNumber == 20) { ++victimCap; ++hunterCap; ++monsterCap; }
                if (turnNumber == 30) { ++victimCap; }
                if (turnNumber == 40) { ++victimCap; ++hunterCap; }
                if (turnNumber == 50) { throw new System.Exception("GAME OVER"); }
                if (turnNumber >= 20 && turnNumber % 7 == 0)
                {
                    ResolvingMovement = true;
                    if (!shownShufflePopup)
                    {
                        shownShufflePopup = true;
                        yield return StartCoroutine(shufflePopup.Show());
                    }
                    for (var i = 0; i < 5; ++i)
                    {
                        var src = new Vector2Int(Random.Range(0, 8), Random.Range(0, 8));
                        var dst = new Vector2Int(Random.Range(0, 8), Random.Range(0, 8));
                        var center = new Vector2Int(4, 4);
                        if (src != center && dst != center) { yield return StartCoroutine(AnimateTileSwap(tiles[src.x][src.y].GetComponent<GameTile>(), tiles[dst.x][dst.y].GetComponent<GameTile>(), 5f)); }
                    }
                    ResolvingMovement = false;
                }
                int victimCount = 0, hunterCount = 0, monsterCount = 0;
                foreach (var agentObject in agents)
                {
                    var agent = agentObject.GetComponent<AgentManager>();
                    if (agent)
                    {
                        switch (agent.agentType)
                        {
                            case AgentType.victim: ++victimCount; break;
                            case AgentType.hunter: ++hunterCount; break;
                            case AgentType.monster: ++monsterCount; break;
                        }
                    }
                }
                if (victimCount < victimCap) { yield return StartCoroutine(TrySpawn(AgentType.victim)); }
                if (hunterCount < hunterCap) { yield return StartCoroutine(TrySpawn(AgentType.hunter)); }
                if (monsterCount < monsterCap) { yield return StartCoroutine(TrySpawn(AgentType.monster)); }
                swapTile = null;
                secondSwapTile = null;
            }
        }
    }

    private IEnumerator<YieldInstruction> TrySpawn(AgentType type)
    {
        for (int i = 0; i < candidates.Count; i++)
        {
            var swap = Random.Range(0, candidates.Count);
            var temp = candidates[swap];
            candidates[swap] = candidates[i];
            candidates[i] = temp;
        }
        foreach (var pos in candidates)
        {
            var valid = true;
            foreach (var g in agents)
            {
                var agent = g.GetComponent<AgentManager>();
                var diff = agent.position - pos;
                if (Mathf.Abs(diff.x) + Mathf.Abs(diff.y) < 2)
                {
                    valid = false;
                    break;
                }
            }
            if (valid)
            {
                yield return StartCoroutine(Spawn(type, pos.x, pos.y, gameObject));
                break;
            }
        }
    }

    public void RemoveAgent (GameObject a) {
        agents.Remove(a);
        if (agents.Select(x => x.GetComponent<AgentManager>().agentType).Where(x => x != AgentType.monster).ToList().Count == 0) {
            Debug.Log("all humans gone tally up score!");
        }
    }

    void Update () {
        // ...
    }
}
