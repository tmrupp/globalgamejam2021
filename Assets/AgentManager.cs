using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    Vector2Int position, nextPosition;
    List<Vector2Int> targets = new List<Vector2Int>();
    static GameObject agentPrefab;

    public static void LoadPrefabs () {
        if (agentPrefab is null)
            agentPrefab = (GameObject) Resources.Load("Prefabs/Agent", typeof(GameObject));
    }

    public static GameObject Create (int i, int j, GameObject caller=null) {
        LoadPrefabs();
        var agent = GameObject.Instantiate(agentPrefab, new Vector3(i, j, 0), Quaternion.identity);

        // assume hunter to begin with
        agent.GetComponent<AgentManager>().targets.Add(caller.GetComponent<TileManager>().GetRitualLocation());
        return agent;
    }

    public void FindNextMove () {

    }

    public void Move () {
        transform.position = new Vector3(nextPosition.x, nextPosition.y, 0);
    }

    public void Update () {
        if (Input.GetKeyDown(KeyCode.Return)) {
            Move();
        }
    }

    
}
