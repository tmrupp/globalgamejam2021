﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum AgentType {
    hunter,
    victim,
    monster
}

public class AgentManager : MonoBehaviour
{
    [SerializeField] private AnimatorMap animatorMap = null;
    [SerializeField] bool spriteReversed = false;  // For sprites which where originally drawn facing left
    public SpriteRenderer Indicator = null;
    private SpriteRenderer CharacterSR;
    private Animator animator;
    private int walkParamId;
    public Vector2Int position, nextPosition, prevPosition;
    List<Vector2Int> targets = new List<Vector2Int>();
    static GameObject agentPrefab;
    TileManager tileManager;
    private bool animatingMovement = false;

    public static Dictionary<AgentType, Color> agentColors = new Dictionary<AgentType, Color>() {
        {AgentType.hunter, Color.red},
        {AgentType.victim, Color.magenta},
        {AgentType.monster, Color.green},
    };

    public static Dictionary<AgentType, SetTargets> targetGetters = new Dictionary<AgentType, SetTargets>() {
        {AgentType.hunter, HunterSetTargets},
        {AgentType.victim, VictimSetTargets},
        {AgentType.monster, MonsterSetTargets},
    };

    public static Dictionary<AgentType, SatisfiesConditions> agentConditions = new Dictionary<AgentType, SatisfiesConditions>() {
        {AgentType.hunter, HumanCondition},
        {AgentType.victim, HumanCondition},
        {AgentType.monster, MonsterCondition},
    };

    public static void LoadPrefabs () {
        if (agentPrefab is null)
            agentPrefab = (GameObject) Resources.Load("Prefabs/Agent", typeof(GameObject));
    }

    public delegate void SetTargets (AgentManager a);
    public static void MonsterSetTargets (AgentManager a) {
        // only one that is dynamic
        a.targets = a.tileManager.GetAgentLocations().Where(
            x => x.Item1 != AgentType.monster).ToList().Select(
                x => x.Item2).ToList();
    }

    public static void HunterSetTargets (AgentManager a) {
        a.targets = new List<Vector2Int>() {a.tileManager.GetRitualLocation()};
    }

    public static void VictimSetTargets (AgentManager a) {
        a.targets = a.tileManager.GetEdges();
    }

    public void KillAgent () {
        tileManager.RemoveAgent(gameObject);
        Destroy(gameObject);
    }

    private AgentType privateAgentType = AgentType.hunter;
    public AgentType agentType
    {
        get { return privateAgentType; }
        set
        {
            privateAgentType = value;
            animator.runtimeAnimatorController = animatorMap.GetAnimator(privateAgentType);
            UpdateSpriteFlip();
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        walkParamId = Animator.StringToHash("Walk");
        CharacterSR = GetComponent<SpriteRenderer>();
        CharacterSR.flipX = (Random.Range(0, 2) == 0);
    }

    public delegate void SatisfiesConditions (AgentManager a);
    public static void MonsterCondition (AgentManager a) {
        //... a monster's work is never done
        foreach (var g in a.tileManager.agents.ToList()) {
            var agent = g.GetComponent<AgentManager>();
            if (agent.position == a.position && agent.agentType != AgentType.monster) {
                agent.KillAgent();
                Debug.Log("MUNCH");
            }
        }
    }

    public static void HumanCondition (AgentManager a) {
        if (a.targets.Contains(a.position)) {
            a.KillAgent();
            Debug.Log("got where I wanted to go");
            return;
        }

        if (a.position == a.tileManager.GetRitualLocation()) {
            a.KillAgent();
            Debug.Log("Victim consumed!");
            return;
        }
    }

    // must be called from a tilemanager
    public static GameObject Create (AgentType type, int i, int j, GameObject caller) {
        LoadPrefabs();
        Vector2Int v = new Vector2Int(i, j);
        TileManager tm = caller.GetComponent<TileManager>();
        var gO = GameObject.Instantiate(agentPrefab, tm.GridToActual(v), Quaternion.identity);

        var agent = gO.GetComponent<AgentManager>();
        agent.agentType = type;
        agent.tileManager = tm;
        agent.position = v;
        agent.Indicator.color = agentColors[type];

        // assume hunter to begin with
        targetGetters[type](agent);
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

    private void UpdateSpriteFlip()
    {
        var displacement = nextPosition - position;
        if (displacement.x != 0) { CharacterSR.flipX = (displacement.x == -1) ^ spriteReversed; }
    }

    void Face (Vector2Int v) {
        UpdateSpriteFlip();
        var displacement = v - position;
        Indicator.transform.up = new Vector3(displacement.x, displacement.y);
        //Debug.DrawLine(Indicator.transform.position, new Vector3(v.x, v.y, 0) - Indicator.transform.position, Color.red, float.MaxValue);
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
        (Vector2Int, int) closest = (position, int.MaxValue);
        search.Enqueue(position);
        cameFrom[position] = position;
        targetGetters[agentType](this);
        
        while (search.Count != 0) {
            var v = search.Dequeue();

            int distance = v == position ? int.MaxValue : GetClosest(v, targets);
            // if this position is closer, but also not the previous position or the current one
            if (distance < closest.Item2) {
                closest = (v, distance);
            }

            if (targets.Contains(v)) {
                // found target return path
                break;
            }

            foreach (var n in tileManager.GetNeighborsAtNoBounds(v)) {
                if (!cameFrom.ContainsKey(n) && 
                    n != position &&
                    n != prevPosition) {
                    cameFrom[n] = v;
                    search.Enqueue(n);
                }
            }
        }

        var path = GetPath(cameFrom, closest.Item1);
        nextPosition = path[path.Count-1];
        Face(nextPosition);
    }

    private IEnumerator<WaitForSeconds> AnimateMovement()
    {
        if (animatingMovement) { yield break; }
        animatingMovement = true;
        animator.SetBool(walkParamId, true);
        yield return new WaitForSeconds(1f);
        animator.SetBool(walkParamId, false);
        animatingMovement = false;
    }

    public void Move () {
        agentConditions[agentType](this);
        transform.position = tileManager.GridToActual(nextPosition);
        StartCoroutine(AnimateMovement());
        prevPosition = position;
        position = nextPosition;
        agentConditions[agentType](this);
        FindNextMove();
    }

    public void Update () {
        UpdateSpriteFlip();
        if (Input.GetKeyDown(KeyCode.Return)) {
            // Debug.Log("got a return");
            Move();
        }
    }   
}
