using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    [SerializeField] private RectTransform _panel = null;
    [SerializeField] private GameObject _text = null;
    [SerializeField] private Text _continue = null;
    [SerializeField] private float _maxHeight = 0f;

    public IEnumerator<Object> Show()
    {
        var width = 600f;
        var speed = 2400f;
        var height = 0f;
        _panel.rect.Set(0, 0, width, height);
        _panel.gameObject.SetActive(true);
        while (height < _maxHeight)
        {
            height += speed * Time.deltaTime;
            _panel.sizeDelta = new Vector2(width, height);
            yield return null;
        }
        height = _maxHeight;
        _panel.sizeDelta = new Vector2(width, height);
        _text.SetActive(true);
        _continue.gameObject.SetActive(true);
        var continueTimer = 0f;
        while (!Input.GetKeyDown(KeyCode.Z))
        {
            continueTimer += Time.deltaTime;
            _continue.gameObject.SetActive((int)continueTimer % 2 == 0);
            yield return null;
        }
        _continue.gameObject.SetActive(false);
        _text.SetActive(false);
        while (_panel.rect.height > 0f)
        {
            height -= speed * Time.deltaTime;
            _panel.sizeDelta = new Vector2(width, height);
            yield return null;
        }
        _panel.gameObject.SetActive(false);
    }
}
