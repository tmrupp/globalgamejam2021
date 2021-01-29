using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameTile : MonoBehaviour
{
    Terrain terrain;
    static GameObject tilePrefab;
    List<int> directions;
    TileManager tileManager;
    Vector2Int location;


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

    public List<int> GetDirections () {
        return directions;
    }
    
    public void UpdateLocation (Vector2Int v)
    {
        location = v;
    }

    public Vector2Int GetLocation () {
        return location;
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
        string s = "neighbors for " + location.ToString() + ": ";
        tileManager.GetNeighborsAt(location).ForEach(x => s = s + x.ToString() + " ");
        Debug.Log(s);

        // no swappa-da ritual
        if (terrain != Terrain.ritual)
            tileManager.SwapThis(gameObject);
    }
}
