using System.Collections.Generic;
using UnityEngine;

public class AnimatorMap : MonoBehaviour
{
    [SerializeField] List<AnimatorOverrideController> victim = null;
    [SerializeField] List<AnimatorOverrideController> hunter = null;
    [SerializeField] List<AnimatorOverrideController> monster = null;

    public AnimatorOverrideController GetAnimator(AgentType type, int index)
    {
        switch (type)
        {
            case AgentType.victim: return victim[index % victim.Count];
            case AgentType.hunter: return hunter[index % hunter.Count];
            case AgentType.monster: return monster[index % monster.Count];
        }
        return null;
    }
}
