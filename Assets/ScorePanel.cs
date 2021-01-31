using System;
using UnityEngine;
using UnityEngine.UI;

public class ScorePanel : MonoBehaviour
{
    public Text scoreNum;
    public Text turnNum;

    private TileManager tileManager;

    void Awake()
    {
        tileManager = FindObjectOfType<TileManager>();
    }

    public void SetScore(int score)
    {
        scoreNum.text = String.Format("{0}", score);
    }

    public void SetTurn(int turn)
    {
        turnNum.text = turn >= tileManager.endTurn ? "Morning" : String.Format("{0} / {1}", turn + 1, tileManager.endTurn);
    }

    private void Update()
    {
        SetScore(tileManager.points);
        SetTurn(tileManager.turnNumber);
    }
}
