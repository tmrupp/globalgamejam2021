using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoreCube : MonoBehaviour
{
    GameObject cubeGuy;
    public static int toSpawn = 100;
    public float spawnSpeed = 0.1f;

    // Start is called before the first frame update
    void Start ()
    {
        cubeGuy = (GameObject) Resources.Load("Prefabs/CubeGuy", typeof(GameObject));
        if (toSpawn > 0)
        {
            StartCoroutine(SpawnCubeCoroutine(spawnSpeed));
            toSpawn--;
            Debug.Log("left to spawn=" + toSpawn.ToString());
        }
    }

    IEnumerator SpawnCubeCoroutine (float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        var cG = GameObject.Instantiate(cubeGuy);
        cG.transform.position = transform.position + new Vector3(Random.Range(-1, 1), Random.Range(0, 3), Random.Range(-1, 1));
    }
}
