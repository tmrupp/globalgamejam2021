using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AgentType {
    hunter,
    victim,
    monster
}

public class AgentManager : MonoBehaviour
{
    public Vector2Int position, nextPosition, prevPosition;
    List<Vector2Int> targets = new List<Vector2Int>();
    static GameObject agentPrefab;
    TileManager tileManager;

    public static Dictionary<AgentType, Color> agentColors = new Dictionary<AgentType, Color>() {
        {AgentType.hunter, Color.red},
        {AgentType.victim, Color.magenta},
        {AgentType.monster, Color.green},
    };

    public static void LoadPrefabs () {
        if (agentPrefab is null)
            agentPrefab = (GameObject) Resources.Load("Prefabs/Agent", typeof(GameObject));
    }

    private void GetHunterTargets () {
        targets = new List<Vector2Int>();
        targets.Add(tileManager.GetRitualLocation());
    }

    private void GetMonsterTargets () {
        targets = new List<Vector2Int>();
        targets.Add(tileManager.GetRitualLocation());
    }



    private void SetTargets () {

    }

    // must be called from a tilemanager
    public static GameObject Create (AgentType type, int i, int j, GameObject caller) {
        LoadPrefabs();
        var gO = GameObject.Instantiate(agentPrefab, new Vector3(i, j, 0), Quaternion.identity);

        var agent = gO.GetComponent<AgentManager>();
        agent.tileManager = caller.GetComponent<TileManager>();

        gO.GetComponent<SpriteRenderer>().color = agentColors[type];

        // assume hunter to begin with
        agent.targets.Add(agent.tileManager.GetRitualLocation());
        agent.FindNextMove();
        return gO;
    }

    List<Vector2Int> GetPath (Dictionary<Vector2Int, Vector2Int> vs, Vector2Int target)
    {  
        Vector2Int next = vs[target], current = target;
        List<Vector2Int> path = new List<Vector2Int>() {target};
        while (next != current) {
            path.Add(current);
            Vector2Int temp = next;
            next = vs[next];
            current = temp;
        }
        return path;
    }

    void Face (Vector2Int v) {
        transform.up = new Vector3(v.x, v.y, 0) - transform.position;
        //Debug.DrawLine(transform.position, new Vector3(v.x, v.y, 0) - transform.position, Color.red, float.MaxValue);
    }

    int Manhattan (Vector2Int a, Vector2Int b) {
        Vector2Int d = a - b;
        return Mathf.Abs(d.x) + Mathf.Abs(d.y);
    }

    int GetClosest (Vector2Int a, List<Vector2Int> xs) {
        int closest = int.MaxValue;
        
        foreach (var x in xs) {
            int d = Manhattan(a, x);
            if (d < closest)
                closest = d;
        }

        return closest;
    }

    public void FindNextMove () {
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Queue<Vector2Int> search = new Queue<Vector2Int>();
        (Vector2Int, int) closest = (position, int.MaxValue/*Manhattan(position, targets[0])*/);
        search.Enqueue(position);
        cameFrom[position] = position;

        while (search.Count != 0) {
            var v = search.Dequeue();

            // if this position is closer, but also not the previous position or the current one
            if (GetClosest(v, targets) < closest.Item2 && v != prevPosition && v != position) {
                closest = (v, GetClosest(v, targets));
            }

            if (targets.Contains(v)) {
                // found target return path
                break;
            }

            foreach (var n in tileManager.GetNeighborsAt(v)) {
                if (!cameFrom.ContainsKey(n)) {
                    cameFrom[n] = v;
                    search.Enqueue(n);
                }
            }
        }

        var path = GetPath(cameFrom, closest.Item1);
        // path.ForEach(x => Debug.Log(x.ToString()));
        nextPosition = path[path.Count-1];
        Face(nextPosition);

        // Debug.Log("did not find a path, despair!");
    }

    public void Move () {
        transform.position = new Vector3(nextPosition.x, nextPosition.y, 0);
        prevPosition = position;
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
