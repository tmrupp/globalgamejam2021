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
        scoreNum.text = String.Format("{0}", score * 1000);
    }

    public void SetTurn(int turn)
    {
        turnNum.text = String.Format("{0} / 50", turn + 1);
    }

    private void Update()
    {
        SetScore(tileManager.points);
        SetTurn(tileManager.turnNumber);
    }
}
