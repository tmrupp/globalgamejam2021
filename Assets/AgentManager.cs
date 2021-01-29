using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    Vector2Int position, nextPosition;
    List<Vector2Int> targets = new List<Vector2Int>();
    static GameObject agentPrefab;
    TileManager tileManager;

    public static void LoadPrefabs () {
        if (agentPrefab is null)
            agentPrefab = (GameObject) Resources.Load("Prefabs/Agent", typeof(GameObject));
    }

    // must be called from a tilemanager
    public static GameObject Create (int i, int j, GameObject caller) {
        LoadPrefabs();
        var gO = GameObject.Instantiate(agentPrefab, new Vector3(i, j, 0), Quaternion.identity);

        var agent = gO.GetComponent<AgentManager>();
        agent.tileManager = caller.GetComponent<TileManager>();

        // assume hunter to begin with
        agent.GetComponent<AgentManager>().targets.Add(agent.tileManager.GetRitualLocation());
        agent.FindNextMove();
        return gO;
    }

    List<Vector2Int> GetPath (Dictionary<Vector2Int, Vector2Int> vs, Vector2Int target)
    {  
        Vector2Int next = vs[target], current = target;
        List<Vector2Int> path = new List<Vector2Int>() {target};
        while (next != current)
        {
            path.Add(current);
            Vector2Int temp = next;
            next = vs[next];
            current = temp;
        }
        return path;
    }

    void Face (Vector2Int v) {
        transform.LookAt(new Vector3 (v.x, v.y, 0));
    }

    public void FindNextMove () {
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Queue<Vector2Int> search = new Queue<Vector2Int>();
        search.Enqueue(position);
        cameFrom[position] = position;

        while (search.Count != 0) {
            var v = search.Dequeue();

            if (targets.Contains(v)) {
                // found target return path
                var path = GetPath(cameFrom, v);
                // path.ForEach(x => Debug.Log(x.ToString()));
                nextPosition = path[path.Count-1];
                Face(nextPosition);
                return;
            }

            foreach (var n in tileManager.GetNeighborsAt(v)) {
                if (!cameFrom.ContainsKey(n)) {
                    cameFrom[n] = v;
                    search.Enqueue(n);
                }
            }
        }

        Debug.Log("did not find a path, despair!");
    }

    public void Move () {
        transform.position = new Vector3(nextPosition.x, nextPosition.y, 0);
        position = nextPosition;
        FindNextMove();
    }

    public void Update () {
        if (Input.GetKeyDown(KeyCode.Return)) {
            // Debug.Log("got a return");
            Move();
        }
    }

    
}
