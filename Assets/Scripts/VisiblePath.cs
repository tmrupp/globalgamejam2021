using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisiblePath : MonoBehaviour
{
    [SerializeField] Sprite spriteToUse = null; //set in editor
    List<GameObject> lines = new List<GameObject>();

    public void DrawLine(Vector2 start, Vector2 end, Color color)
    {
        string debugSummary = string.Empty;

        GameObject line = new GameObject();
        SpriteRenderer sr = line.AddComponent<SpriteRenderer>();
        sr.sprite = spriteToUse;
        sr.color = color;
        sr.sortingLayerName = "Agent";
        sr.sortingOrder = 1;
        Vector2 midpoint = (Vector2)(start + end) / 2f;
        line.transform.position = midpoint;

        debugSummary += "start: " + start;
        debugSummary += "midpoint: " + midpoint;
        debugSummary += "end: " + end;

        //stretch the sprite
        Vector3 scale = line.transform.localScale;
        scale.x *= (end - start).magnitude * 25f;
        scale.y *= 2f;
        line.transform.localScale = scale;

        debugSummary += "set length to: " + (end - start).magnitude;

        //rotate the object so that the endpoints of the object are at the start and end
        float angle = Mathf.Atan2(end.y - midpoint.y, end.x - midpoint.x);
        line.transform.Rotate(Vector3.forward, angle * Mathf.Rad2Deg);

        debugSummary += "angle: " + angle * Mathf.Rad2Deg;

        lines.Add(line);

        //Debug.Log(debugSummary);
    }

    public void Clear()
    {
        foreach (var line in lines)
        {
            Destroy(line);
        }
        lines.Clear();
    }
}
