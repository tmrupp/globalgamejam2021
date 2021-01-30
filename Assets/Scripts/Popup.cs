using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    [SerializeField] private RectTransform _panel = null;
    [SerializeField] private Text _text = null;
    [SerializeField] private Text _continue = null;

    private void Awake()
    {
        StartCoroutine(Show(_text.text));
    }

    private IEnumerator<Object> Show(string text)
    {
        var width = 600f;
        var maxHeight = 400f;
        var speed = 2400f;
        var height = 0f;
        _panel.rect.Set(0, 0, width, height);
        _panel.gameObject.SetActive(true);
        _text.text = text;
        while (height < maxHeight)
        {
            height += speed * Time.deltaTime;
            _panel.sizeDelta = new Vector2(width, height);
            yield return null;
        }
        height = maxHeight;
        _panel.sizeDelta = new Vector2(width, height);
        _text.gameObject.SetActive(true);
        _continue.gameObject.SetActive(true);
        var continueTimer = 0f;
        while (!Input.GetKeyDown(KeyCode.Z))
        {
            continueTimer += Time.deltaTime;
            _continue.gameObject.SetActive((int)continueTimer % 2 == 0);
            yield return null;
        }
        _continue.gameObject.SetActive(false);
        _text.gameObject.SetActive(false);
        while (_panel.rect.height > 0f)
        {
            height -= speed * Time.deltaTime;
            _panel.sizeDelta = new Vector2(width, height);
            yield return null;
        }
        _panel.gameObject.SetActive(false);
    }
}
