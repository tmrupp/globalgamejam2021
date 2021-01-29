using UnityEngine;

public class Popup : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) { gameObject.SetActive(false); }
    }
}
