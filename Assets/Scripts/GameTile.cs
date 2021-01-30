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
    public List<int> Directions { get { return directions; } }

    TileManager tileManager;

    Vector2Int location;
    public Vector2Int Location { get { return location; } set { location = value; } }


    // setup the tile prefab
    public static void LoadPrefabs () {
        if (tilePrefab is null)
            tilePrefab = (GameObject) Resources.Load("Prefabs/GameTile", typeof(GameObject));
    }

    // +n => rotate n times 90 clockwise, - => rotate n times 90 counter-clockwise
    public void Rotate (int rotation) {
        transform.Rotate(transform.eulerAngles + new Vector3(0, 0, -90f * rotation));
        directions = directions.Select(x => (x + rotation) % TileManager.directions.Count).ToList();
    }

    public static GameObject Create (Terrain t, int i, int j, GameObject caller) {
        LoadPrefabs();
        Vector2Int v =  new Vector2Int(i, j);
        TileManager tm =  caller.GetComponent<TileManager>();

        var gameTile = GameObject.Instantiate(tilePrefab, tm.GridToActual(v), Quaternion.identity);
        gameTile.GetComponent<SpriteRenderer>().sprite = TileManager.GetSpriteOfTerrain(t);

        var gt = gameTile.GetComponent<GameTile>();
        gt.terrain = t;
        gt.directions = TileManager.GetDirectionsOfTerrain(t);
        if (t != Terrain.ritual)
            gt.Rotate(Random.Range(0, 4));
        gt.tileManager = tm;
        gt.location = v;

        gameTile.transform.SetParent(caller.transform);
        return gameTile;
    }

    private void OnMouseDown() {
        // string s = "neighbors for " + location.ToString() + ": ";
        // tileManager.GetNeighborsAt(location).ForEach(x => s = s + x.ToString() + " ");
        // Debug.Log(s);

        // no swappa-da ritual
        if (terrain != Terrain.ritual)
            tileManager.SwapThis(gameObject);
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
    }
}
