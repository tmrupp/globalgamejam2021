using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

enum Terrain
{
    four,
    t,
    straight,
    bent
}

public class TileManager : MonoBehaviour
{
    
    
    Dictionary<Terrain, string> terrainFiles = new Dictionary<Terrain, string> () 
    {
        {Terrain.four, "path_four_way"},
        {Terrain.t, "path_t"},
        {Terrain.straight, "path_straight"},
        {Terrain.bent, "path_bent"}
    };
    
    Dictionary<Terrain, Sprite> terrainSprites = new Dictionary<Terrain, Sprite> ();
    List<List<GameObject>> tiles = new List<List<GameObject>>();
    
    GameObject tilePrefab;
    public int length = 9, width = 9;

    public Sprite GetRandomSprite ()
    {
        var sprites = terrainSprites.Values.ToList();
        return sprites[Random.Range(0, sprites.Count)];
    }

    void Start()
    {
        foreach (var t in terrainFiles)
        {
            terrainSprites[t.Key] = (Sprite) Resources.Load("Sprites/" + t.Value, typeof(Sprite));
        }

        tilePrefab = (GameObject) Resources.Load("Prefabs/GameTile", typeof(GameObject));

        for (int i = 0; i < width; i++)
        {
            var row = new List<GameObject>();
            for (int j = 0; j < length; j++)
            {
                row.Add(GameObject.Instantiate(tilePrefab, new Vector3(i-4f, j-4f, 0), Quaternion.identity));
                row[j].GetComponent<SpriteRenderer>().sprite = GetRandomSprite();
            }
            tiles.Add(row);
        }
    }
}
