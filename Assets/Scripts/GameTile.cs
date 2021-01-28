using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameTile : MonoBehaviour
{
    // TODO: Indicate viable paths out!
    Terrain terrain;
    // setup the tile prefab
    static GameObject tilePrefab;
    List<int> directions;
    TileManager tileManager;
    Vector2Int location;

    public static void LoadPrefabs ()
    {
        if (tilePrefab is null)
            tilePrefab = (GameObject) Resources.Load("Prefabs/GameTile", typeof(GameObject));
    }

    // +n => rotate n times 90 clockwise, - => rotate n times 90 coutner-clockwise
    public void Rotate (int rotation)
    {
        transform.Rotate(transform.eulerAngles + new Vector3(0, 0, 90f * rotation));
        directions = directions.Select(x => (x + rotation) % TileManager.directions.Count).ToList();
    }

    public List<int> GetDirections ()
    {
        return directions;
    }
    
    public static GameObject Create (Terrain t, int i, int j, GameObject caller)
    {
        LoadPrefabs();
        var gameTile = GameObject.Instantiate(tilePrefab, new Vector3(i, j, 0), Quaternion.identity);
        gameTile.GetComponent<SpriteRenderer>().sprite = TileManager.GetSpriteOfTerrain(t);

        var gt = gameTile.GetComponent<GameTile>();
        gt.terrain = t;
        gt.directions = TileManager.GetDirectionsOfTerrain(t);
        gt.Rotate(Random.Range(0,3));
        gt.tileManager = caller.GetComponent<TileManager>();
        gt.location = new Vector2Int(i, j);

        gameTile.transform.SetParent(caller.transform);
        return gameTile;
    }

    private void OnMouseDown() {
        string s = "neighbors for " + location.ToString() + ": ";
        tileManager.GetNeighborsAt(location).ForEach(x => s = s + x.ToString() + " ");
        Debug.Log(s);
    }
}
