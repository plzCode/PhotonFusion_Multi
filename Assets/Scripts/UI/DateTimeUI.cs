using TMPro;
using UnityEngine;

public class DateTimeUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _dateText;
    [SerializeField] TextMeshProUGUI _timeText;

    void Update()
    {
        _dateText.text = System.DateTime.Now.ToString("MMM. dd yyyy", System.Globalization.CultureInfo.InvariantCulture).ToUpper();
        _timeText.text = System.DateTime.Now.ToString("tt hh:mm", System.Globalization.CultureInfo.InvariantCulture).ToUpper();
    }
}
