using TMPro;
using UnityEngine;

public class AmmoDisplay_UI : Base_UI
{
    [SerializeField] TextMeshProUGUI currentAmmoText;
    [SerializeField] TextMeshProUGUI reserveAmmoText;

    public void OnChangeReserveAmmo(int reserveAmmo)
    {
        reserveAmmoText.text = reserveAmmo.ToString();
    }

    public void OnChangeCurrentAmmo(int currentAmmo)
    {
        currentAmmoText.text = currentAmmo.ToString();
    }
}
