using TMPro;
using UnityEngine;

/// <summary>
/// Listens to BananaClickerManager and updates the coin count TextMeshPro label.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class CoinDisplay : MonoBehaviour
{
    private TextMeshProUGUI _label;

    private void Awake()
    {
        _label = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        // Defer subscription in case Manager Awake hasn't run yet.
        if (BananaClickerManager.Instance != null)
            Subscribe();
    }

    private void Start()
    {
        // Guaranteed to run after all Awakes.
        if (BananaClickerManager.Instance != null)
        {
            Subscribe();
            UpdateLabel(BananaClickerManager.Instance.CoinCount);
        }
    }

    private void OnDisable()
    {
        if (BananaClickerManager.Instance != null)
            BananaClickerManager.Instance.OnCoinCountChanged -= UpdateLabel;
    }

    private void Subscribe()
    {
        BananaClickerManager.Instance.OnCoinCountChanged -= UpdateLabel;
        BananaClickerManager.Instance.OnCoinCountChanged += UpdateLabel;
    }

    private void UpdateLabel(int count) => _label.text = count.ToString();
}
