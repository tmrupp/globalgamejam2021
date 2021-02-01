using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    public void AdvanceScene()
    {
        Debug.Log("Beginning next scene");
        SceneManager.LoadScene(1);
    }

    private void Update () {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
            SceneManager.LoadScene(1);
    }
}
