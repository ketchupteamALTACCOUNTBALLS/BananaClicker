using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A purchasable upgrade button with hover and click scale animations.
/// On a successful purchase it applies its effect once and disables itself.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class UpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("Upgrade Settings")]
    [Tooltip("Coin cost to purchase this upgrade.")]
    [SerializeField] private int _cost = 250;
    [Tooltip("Multiplier applied to coins-per-click on purchase.")]
    [SerializeField] private int _clickMultiplier = 2;

    [Header("Sound Effects")]
    [Tooltip("Played when the upgrade is purchased successfully.")]
    [SerializeField] private AudioClip _successSound;
    [Tooltip("Played when the player can't afford the upgrade.")]
    [SerializeField] private AudioClip _rejectedSound;
    [Tooltip("Volume scale for sound effects. Values above 1 make them louder than normal.")]
    [SerializeField] private float _soundVolume = 1f;
    [Header("Hover Scale")]
    [SerializeField] private float _hoverScaleMultiplier = 0.88f;
    [SerializeField] private float _hoverSmoothTime = 0.12f;

    [Header("Click Scale")]
    [SerializeField] private float _clickScaleMultiplier = 0.78f;
    [SerializeField] private float _clickDuration = 0.15f;
    [SerializeField] private float _clickRecoverSmoothTime = 0.08f;

    private Vector3 _baseScale;
    private float _currentScaleMultiplier = 1f;
    private float _scaleVelocity;
    private bool _isHovered;
    private bool _isClicking;
    private bool _purchased;
    private Coroutine _clickRoutine;
    private SpriteRenderer _spriteRenderer;
    private AudioSource _audioSource;

    private void Awake()
    {
        _baseScale = transform.localScale;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        EnsureCollider();
        EnsureAudioSource();
    }

    private void EnsureAudioSource()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0f; // Full 2D — not affected by distance.
    }

    private void EnsureCollider()
    {
        if (GetComponent<Collider2D>() == null)
            gameObject.AddComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (_purchased) return;
        ApplyScale();
    }

    // ──────────────────────────────────────────────
    // Scale animation
    // ──────────────────────────────────────────────

    private void ApplyScale()
    {
        float target = _isClicking ? _clickScaleMultiplier
                      : _isHovered  ? _hoverScaleMultiplier
                      : 1f;

        float smoothTime = _isClicking ? _hoverSmoothTime : _clickRecoverSmoothTime;

        _currentScaleMultiplier = Mathf.SmoothDamp(
            _currentScaleMultiplier, target, ref _scaleVelocity, smoothTime);

        transform.localScale = _baseScale * _currentScaleMultiplier;
    }

    // ──────────────────────────────────────────────
    // Pointer events
    // ──────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData _) => _isHovered = true;
    public void OnPointerExit(PointerEventData _)  => _isHovered = false;

    public void OnPointerDown(PointerEventData _)
    {
        if (_purchased) return;

        // Sound and purchase fire immediately on press — no delay.
        TryPurchase();

        if (_clickRoutine != null)
            StopCoroutine(_clickRoutine);
        _clickRoutine = StartCoroutine(ClickRoutine());
    }

    private IEnumerator ClickRoutine()
    {
        _isClicking = true;
        yield return new WaitForSeconds(_clickDuration);
        _isClicking = false;
        _clickRoutine = null;
    }

    // ──────────────────────────────────────────────
    // Purchase
    // ──────────────────────────────────────────────

    private void TryPurchase()
    {
        if (BananaClickerManager.Instance == null) return;

        if (!BananaClickerManager.Instance.TrySpend(_cost))
        {
            PlaySound(_rejectedSound);
            return;
        }

        BananaClickerManager.Instance.ApplyClickMultiplier(_clickMultiplier);
        PlaySound(_successSound);
        _purchased = true;

        // Fade out to signal the upgrade is consumed.
        _spriteRenderer.color = new Color(1f, 1f, 1f, 0.35f);
        transform.localScale = _baseScale;

        if (GetComponent<Collider2D>() is Collider2D col)
            col.enabled = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && _audioSource != null)
            _audioSource.PlayOneShot(clip, _soundVolume);
    }
}
