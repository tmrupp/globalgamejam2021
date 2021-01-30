using System.Collections.Generic;
using UnityEngine;

public class AnimatorMap : MonoBehaviour
{
    [SerializeField] AnimatorOverrideController victim = null;
    [SerializeField] AnimatorOverrideController hunter = null;
    [SerializeField] AnimatorOverrideController monster = null;

    public AnimatorOverrideController GetAnimator(AgentType type)
    {
        switch (type)
        {
            case AgentType.victim: return victim;
            case AgentType.hunter: return hunter;
            case AgentType.monster: return monster;
        }
        return null;
    }
}
