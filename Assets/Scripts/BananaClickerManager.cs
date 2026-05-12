using System;
using UnityEngine;

/// <summary>
/// Singleton that owns the global coin count, coins-per-click multiplier,
/// coins-per-second rate, and broadcasts changes.
/// </summary>
public class BananaClickerManager : MonoBehaviour
{
    public static BananaClickerManager Instance { get; private set; }

    public event Action<int> OnCoinCountChanged;

    /// <summary>Fired whenever the total coins-per-second rate changes.</summary>
    public event Action<int> OnCoinsPerSecondChanged;

    public int CoinCount { get; private set; }

    /// <summary>Multiplier applied to every banana click (starts at 1).</summary>
    public int CoinsMultiplier { get; private set; } = 1;

    /// <summary>Total coins awarded automatically every second.</summary>
    public int CoinsPerSecond { get; private set; }

    private float _secondTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (CoinsPerSecond <= 0) return;

        _secondTimer += Time.deltaTime;
        if (_secondTimer >= 1f)
        {
            _secondTimer -= 1f;
            AddCoins(CoinsPerSecond);
        }
    }

    /// <summary>Adds the given amount of coins and fires the change event.</summary>
    public void AddCoins(int amount)
    {
        CoinCount += amount;
        OnCoinCountChanged?.Invoke(CoinCount);
    }

    /// <summary>
    /// Attempts to spend the given amount. Returns true and deducts coins on success,
    /// returns false without touching the count if funds are insufficient.
    /// </summary>
    public bool TrySpend(int amount)
    {
        if (CoinCount < amount)
            return false;

        CoinCount -= amount;
        OnCoinCountChanged?.Invoke(CoinCount);
        return true;
    }

    /// <summary>Multiplies the per-click coin yield by the given factor.</summary>
    public void ApplyClickMultiplier(int factor)
    {
        CoinsMultiplier *= factor;
    }

    /// <summary>Adds the given amount to the automatic coins-per-second rate.</summary>
    public void AddCoinsPerSecond(int amount)
    {
        CoinsPerSecond += amount;
        OnCoinsPerSecondChanged?.Invoke(CoinsPerSecond);
    }
}
