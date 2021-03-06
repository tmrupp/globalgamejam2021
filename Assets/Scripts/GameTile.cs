﻿using System.Collections;
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

    public void SetSprite () {
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

    public static GameObject CreateAndReplace (Terrain t, Vector2Int v, int rot, GameObject caller) {
        var gt = Create (t, v.x, v.y, caller);

        if (t != Terrain.ritual)
            gt.GetComponent<GameTile>().Rotate(rot);
        caller.GetComponent<TileManager>().ReplaceGameTile(v, gt.GetComponent<GameTile>());

        return gt;
    }

    private void OnMouseDown() {
        // string s = "neighbors for " + location.ToString() + ": ";
        // tileManager.GetNeighborsAt(location).ForEach(x => s = s + x.ToString() + " ");
        // Debug.Log(s);

        // no swappa-da ritual
        if (tileManager.turnNumber < tileManager.endTurn && terrain != Terrain.ritual && !tileManager.ResolvingMovement) {
            StartCoroutine(tileManager.SwapThis(gameObject));
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
            Vector2 dir;
            if (Directions[i] == 0)
            {
                dir = Vector2.up;
            }
            else if (Directions[i] == 1)
            {
                dir = Vector2.right;
            }
            else if (Directions[i] == 2)
            {
                dir = Vector2.down;
            }
            else //if (Directions[i] == 3)
            {
                dir = Vector2.left;
            }

            vp?.DrawLine(tileManager.GridToActual(location), tileManager.GridToActual((Vector2)location + dir/2), new Color(1f, 1f, 1f, 0.5f), 1);
        }
    }

    public void ClearTileType()
    {
        vp?.Clear();
    }

    private void OnMouseEnter()
    {
        foreach (var agent in tileManager.agents)
        {
            AgentManager am = agent.GetComponent<AgentManager>();
            if (am.position == location)
            {
                am.DrawPath();
                tileManager.tooltip.transform.position = Camera.main.WorldToScreenPoint(transform.position);
                tileManager.tooltip.Show(am.agentType, am.monsterType);
            }
        }

        if (gameObject != tileManager.GetSwapTile() && gameObject != tileManager.GetSecondSwapTile())
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
        tileManager.tooltip.Hide();

        if (gameObject != tileManager.GetSwapTile() && gameObject != tileManager.GetSecondSwapTile())
            SetColor(Color.white);

        ClearTileType();
    }

    private void Start()
    {
        vp = GetComponent<VisiblePath>();
    }
}
