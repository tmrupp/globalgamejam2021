using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue
{
    public List<string> Lines { get; private set; }

    public Dialogue(List<string> lines)
    {
        Lines = lines;
    }
}

public class DialogueParser : MonoBehaviour
{
    private Dictionary<string, Dialogue> _dialogue = new Dictionary<string, Dialogue>();
    [SerializeField] private GameObject _dialogueBox = null;
    [SerializeField] private Text _text = null;

    void Awake()
    {
        foreach (var file in Resources.LoadAll<TextAsset>("Dialogue"))
        {
            string name = "";
            List<string> lines = new List<string>();
            foreach (var line in Regex.Split(file.text, "\r\n|\n|\r"))
            {
                if (line == "")
                {
                    _dialogue[name] = new Dialogue(lines);
                    name = "";
                    lines = new List<string>();
                }
                else
                {
                    if (name == "") { name = line; }
                    else { lines.Add(line); }
                }
            }
            if (name != "")
            {
                _dialogue[name] = new Dialogue(lines);
            }
        }
    }

    private IEnumerator<Object> DialogueCoroutine(Dialogue dialogue)
    {
        _dialogueBox.SetActive(true);
        foreach (var line in dialogue.Lines)
        {
            float progress = 0f;
            while (progress < line.Length || !Input.GetKeyDown(KeyCode.Z))
            {
                if (Input.GetKeyDown(KeyCode.X)) { progress = line.Length; }
                progress += (Input.GetKey(KeyCode.Z) ? 48f : 16f) * Time.deltaTime;
                _text.text = line.Substring(0, Mathf.Min((int)progress, line.Length));
                yield return null;
            }
        }
        _dialogueBox.SetActive(false);
        _text.text = "";
    }

    public bool Ready()
    {
        return !_dialogueBox.activeSelf;
    }

    public void ShowDialogue(string name)
    {
        var dialogue = _dialogue[name];
        StartCoroutine(DialogueCoroutine(dialogue));
    }
}
