﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum Terrain
{
    four,
    t,
    straight,
    bent
}

public class TileManager : MonoBehaviour
{
    
    
    public static Dictionary<Terrain, string> terrainFiles = new Dictionary<Terrain, string> () 
    {
        {Terrain.four, "path_four_way"},
        {Terrain.t, "path_t"},
        {Terrain.straight, "path_straight"},
        {Terrain.bent, "path_bent"}
    };

    static Vector2Int up = new Vector2Int(0, 1), right = new Vector2Int(1, 0), down = new Vector2Int(0, -1), left = new Vector2Int(-1, 0);

    static public readonly List<Vector2Int> directions = new List<Vector2Int>() {up, right, down, left};

    public static Dictionary<Terrain, List<int>> terrainDirections = new Dictionary<Terrain, List<int>> () 
    {
        {Terrain.four, new List<int>() {0, 1, 2, 3}},
        {Terrain.t, new List<int>() {1, 2, 3}},
        {Terrain.straight, new List<int>() {0, 2}},
        {Terrain.bent, new List<int>() {1, 2}}
    };
    
    public static Dictionary<Terrain, Sprite> terrainSprites = new Dictionary<Terrain, Sprite> ();
    List<List<GameObject>> tiles = new List<List<GameObject>>();
    List<Terrain> terrains = new List<Terrain>();
    
    public int length = 9, width = 9;

    public static Sprite GetSpriteOfTerrain (Terrain t)
    {
        return terrainSprites[t];
    }

    public static List<int> GetDirectionsOfTerrain (Terrain t)
    {
        return new List<int> (terrainDirections[t]);
    }

    public Terrain GetRandomTerrain ()
    {
        return terrains[Random.Range(0, terrains.Count)];
    }

    public GameTile GetTileAt (Vector2Int v)
    {
        if (v.x >= 0 && v.x < width && v.y >= 0 && v.y < length)
            return tiles[v.x][v.y].GetComponent<GameTile>();
        else
        {
            Debug.LogError("v out of range " + v.ToString());
            return null;
        }
    }

    public List<Vector2Int> GetNeighborsAt (Vector2Int v)
    {
        return GetTileAt(v).GetDirections().Select(
            x => v + directions[x]).Where(
                x => !(GetTileAt(x) is null)).ToList();
    }

    void Start()
    {
        // load in the sprites
        foreach (var t in terrainFiles)
        {
            terrainSprites[t.Key] = (Sprite) Resources.Load("Sprites/" + t.Value, typeof(Sprite));
        }

        terrains = System.Enum.GetValues(typeof(Terrain)).OfType<Terrain>().ToList();

        // populate the map of tiles with the prefabs
        for (int i = 0; i < width; i++)
        {
            var row = new List<GameObject>();
            for (int j = 0; j < length; j++)
            {
                row.Add(GameTile.Create(GetRandomTerrain(), i, j, gameObject));
            }
            tiles.Add(row);
        }
    }
}