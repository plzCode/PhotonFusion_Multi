using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHUDUnit : MonoBehaviour
{
    public CharacterHUDContainer_UI HUDContainer;

    [SerializeField] Image _portraitImage;
    [SerializeField] Image _healthBarImage;
    [SerializeField] Slider _healthBarSlider;
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] TextMeshProUGUI _healthText;
    public PlayerController player;
    float _maxHealth = 100;
    float _currentHealth = 100;

    float _percent => _maxHealth > 0 ? (float)_currentHealth / _maxHealth : 0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //OnChangeHealth(_currentHealth - 1);
        }
    }

    public void SetCharacter(PlayerController player)
    {
        // SetName();
        // SetPortraitImage();
        // Register OnChangeHealth and OnChangeMaxHealth events

        // Initialize health values and Invoke OnChangeHealth and OnChangeMaxHealth
    }

    void SetPortraitImage(Sprite sprite)
    {
        _portraitImage.sprite = sprite;
    }

    void SetName(string name)
    {
        _nameText.text = name;
    }

    public void OnChangeHealth(float health)
    {
        _currentHealth = health;
        _healthBarImage.color = HUDContainer.HealthGradient.Evaluate(_percent);
        UpdateHealth();
    }

    void OnChangeMaxHealth(int maxHealth)
    {
        _maxHealth = maxHealth;
        _healthBarImage.color = HUDContainer.HealthGradient.Evaluate(_percent);
        UpdateHealth();
    }

    void UpdateHealth()
    {
        if (_healthText != null)
            _healthText.text = $"{_currentHealth} / {_maxHealth}";

        _healthBarSlider.value = _percent;
    }
}
