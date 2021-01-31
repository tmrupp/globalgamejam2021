using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public string[] fillText;
    public Text text;
    public GameObject panel;

    public void Show(AgentType atype, MonsterType type)
    {
        switch (atype)
        {
            case AgentType.hunter:
                text.text = fillText[0];
                break;
            case AgentType.victim:
                text.text = fillText[1];
                break;
            case AgentType.monster:
                text.text = fillText[1 + (int)type];
                break;
        }
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
