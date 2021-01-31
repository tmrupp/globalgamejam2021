using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSplatter : MonoBehaviour
{
    ParticleSystem ps;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    public bool MakeSplatter()
    {
        if (ps.isPlaying)
        {
            return false;
        }
        else
        {
            ps.Play();
            return true;
        }
    }

    void Update()
    {
        //Debug
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ps.Play();
        }
    }
}
