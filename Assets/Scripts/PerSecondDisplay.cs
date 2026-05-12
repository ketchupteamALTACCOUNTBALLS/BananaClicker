using TMPro;
using UnityEngine;

/// <summary>
/// Listens to BananaClickerManager and updates a TextMeshProUGUI label
/// with the current automatic coins-per-second rate.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class PerSecondDisplay : MonoBehaviour
{
    private TextMeshProUGUI _label;

    private void Awake()
    {
        _label = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (BananaClickerManager.Instance == null) return;

        Subscribe();
        UpdateLabel(BananaClickerManager.Instance.CoinsPerSecond);
    }

    private void OnDisable()
    {
        if (BananaClickerManager.Instance != null)
            BananaClickerManager.Instance.OnCoinsPerSecondChanged -= UpdateLabel;
    }

    private void Subscribe()
    {
        BananaClickerManager.Instance.OnCoinsPerSecondChanged -= UpdateLabel;
        BananaClickerManager.Instance.OnCoinsPerSecondChanged += UpdateLabel;
    }

    /// <summary>Updates the label text whenever the per-second rate changes.</summary>
    private void UpdateLabel(int perSecond) =>
        _label.text = $"$ Per Second: {perSecond}";
}
