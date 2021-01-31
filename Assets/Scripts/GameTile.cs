using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameTile : MonoBehaviour
{
    Terrain terrain;
    public Terrain @Terrain { get { return terrain; } }

    static GameObject tilePrefab;

    List<int> directions;
    public List<int> Directions { get { return directions.Select(x => (x + rotation) % 4).ToList(); } }

    TileManager tileManager;

    Vector2Int location;
    public Vector2Int Location { get { return location; } set { location = value; } }
    public int rotation = 0;

    VisiblePath vp;

    // setup the tile prefab
    public static void LoadPrefabs () {
        if (tilePrefab is null)
            tilePrefab = (GameObject) Resources.Load("Prefabs/GameTile", typeof(GameObject));
    }

    // +n => rotate n times 90 clockwise, - => rotate n times 90 counter-clockwise
    public void Rotate (int r) {
        // transform.Rotate(transform.eulerAngles + new Vector3(0, 0, -90f * r));
        // directions = directions.Select(x => (x + r) % TileManager.directions.Count).ToList();
        rotation = r;
    }

    void SetSprite () {
        var sprites = TileManager.GetSpriteOfTerrain(terrain, rotation);
        var renderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
        gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = sprites.Item1;
        gameObject.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = sprites.Item2;
        // renderers[1].sprite = sprites.Item2;
    }

    public static GameObject Create (Terrain t, int i, int j, GameObject caller) {
        LoadPrefabs();
        Vector2Int v =  new Vector2Int(i, j);
        TileManager tm =  caller.GetComponent<TileManager>();

        var gameTile = GameObject.Instantiate(tilePrefab, tm.GridToActual(v), Quaternion.identity);

        var gt = gameTile.GetComponent<GameTile>();
        gt.terrain = t;
        gt.directions = TileManager.GetDirectionsOfTerrain(t);
        if (t != Terrain.ritual)
            gt.Rotate(Random.Range(0, 4));
        gt.tileManager = tm;
        gt.location = v;
        gt.SetSprite();

        gameTile.transform.SetParent(caller.transform);
        return gameTile;
    }

    private void OnMouseDown() {
        // string s = "neighbors for " + location.ToString() + ": ";
        // tileManager.GetNeighborsAt(location).ForEach(x => s = s + x.ToString() + " ");
        // Debug.Log(s);

        // no swappa-da ritual
        if (terrain != Terrain.ritual && !tileManager.ResolvingMovement) {
            tileManager.SwapThis(gameObject);
        }
    }

    public void SetColor (Color c) {
        gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color = c;
        gameObject.transform.GetChild(1).GetComponent<SpriteRenderer>().color = c;

    }

    public void DrawTileType()
    {
        for (int i = 0; i < Directions.Count; i++)
        {
            Vector2Int dir;
            if (Directions[i] == 0)
            {
                dir = Vector2Int.up;
            }
            else if (Directions[i] == 1)
            {
                dir = Vector2Int.right;
            }
            else if (Directions[i] == 2)
            {
                dir = Vector2Int.down;
            }
            else //if (Directions[i] == 3)
            {
                dir = Vector2Int.left;
            }

            vp?.DrawLine(tileManager.GridToActual(location), tileManager.GridToActual(location + dir), Color.white, 1);
        }
    }

    private void OnMouseEnter()
    {
        foreach (var agent in tileManager.agents)
        {
            AgentManager am = agent.GetComponent<AgentManager>();
            if (am.position == location)
            {
                am.DrawPath();
            }
        }

        SetColor(Color.yellow);

        DrawTileType();
    }

    private void OnMouseExit()
    {
        foreach (var agent in tileManager.agents)
        {
            AgentManager am = agent.GetComponent<AgentManager>();
            if (am.position == location)
            {
                am.ClearPath();
            }
        }

        if ((tileManager.GetSwapTile() is null) || gameObject != tileManager.GetSwapTile())
            SetColor(Color.white);

        vp?.Clear();
    }

    private void Start()
    {
        vp = GetComponent<VisiblePath>();
    }
}
