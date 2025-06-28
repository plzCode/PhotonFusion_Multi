using TMPro;
using UnityEngine;

public class TextFlicker : MonoBehaviour
{
    TextMeshProUGUI _text;

    void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (_text != null)
        {
            _text.enabled = Mathf.FloorToInt(Time.time * 2) % 2 == 0;
        }
    }
}
