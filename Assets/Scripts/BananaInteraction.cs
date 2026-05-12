using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles the banana idle rotation, hover shrink, and click shrink animations,
/// then notifies BananaClickerManager on click.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BananaInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Idle Rotation")]
    [SerializeField] private float _rotationAmplitudeDeg = 8f;
    [SerializeField] private float _rotationSpeedHz = 0.35f;

    [Header("Hover Scale")]
    [SerializeField] private float _hoverScaleMultiplier = 0.88f;
    [SerializeField] private float _hoverSmoothTime = 0.12f;

    [Header("Click Scale")]
    [SerializeField] private float _clickScaleMultiplier = 0.78f;
    [SerializeField] private float _clickDuration = 0.5f;
    [Tooltip("How quickly the banana springs back to normal size after a click (lower = faster).")]
    [SerializeField] private float _clickRecoverSmoothTime = 0.08f;

    [Header("Coins per Click")]
    [SerializeField] private int _coinsPerClick = 1;

    [Header("Click Sound")]
    [SerializeField] private AudioClip _clickSound;
    [SerializeField] private float _clickSoundVolume = 1f;

    private AudioSource _audioSource;
    private Vector3 _baseScale;
    private float _targetScaleMultiplier = 1f;
    private float _currentScaleMultiplier = 1f;
    private float _scaleVelocity;

    private bool _isHovered;
    private bool _isClicking;
    private Coroutine _clickRoutine;

    // Physics 2D collider or a PolygonCollider is not used here — we rely on
    // a Camera with PhysicsRaycaster2D / a Canvas with GraphicRaycaster being
    // present. Since the banana is a world-space sprite we add a Collider2D.
    private void Awake()
    {
        _baseScale = transform.localScale;
        EnsureCollider();
        EnsureAudioSource();
    }

    private void EnsureAudioSource()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        _audioSource.playOnAwake = false;
    }

    private void EnsureCollider()
    {
        if (GetComponent<Collider2D>() == null)
            gameObject.AddComponent<BoxCollider2D>();
    }

    private void Update()
    {
        ApplyIdleRotation();
        ApplyScale();
    }

    // ──────────────────────────────────────────────
    // Idle rotation
    // ──────────────────────────────────────────────

    private void ApplyIdleRotation()
    {
        float angle = Mathf.Sin(Time.time * _rotationSpeedHz * Mathf.PI * 2f) * _rotationAmplitudeDeg;
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    // ──────────────────────────────────────────────
    // Scale
    // ──────────────────────────────────────────────

    private void ApplyScale()
    {
        float target = _isClicking ? _clickScaleMultiplier
                      : _isHovered  ? _hoverScaleMultiplier
                      : 1f;

        // Use a faster smooth time when recovering from a click so it snaps back quickly.
        float smoothTime = _isClicking ? _hoverSmoothTime : _clickRecoverSmoothTime;

        _currentScaleMultiplier = Mathf.SmoothDamp(
            _currentScaleMultiplier, target, ref _scaleVelocity, smoothTime);

        transform.localScale = _baseScale * _currentScaleMultiplier;
    }

    // ──────────────────────────────────────────────
    // Pointer events (requires Physics2DRaycaster on Camera)
    // ──────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData _) => _isHovered = true;
    public void OnPointerExit(PointerEventData _)  => _isHovered = false;

    public void OnPointerDown(PointerEventData _)
    {
        if (_clickRoutine != null)
            StopCoroutine(_clickRoutine);
        _clickRoutine = StartCoroutine(ClickRoutine());
        int coins = _coinsPerClick * (BananaClickerManager.Instance?.CoinsMultiplier ?? 1);
        BananaClickerManager.Instance?.AddCoins(coins);

        if (_clickSound != null)
            _audioSource.PlayOneShot(_clickSound, _clickSoundVolume);
    }

    public void OnPointerUp(PointerEventData _) { /* handled by coroutine */ }

    private IEnumerator ClickRoutine()
    {
        _isClicking = true;
        yield return new WaitForSeconds(_clickDuration);
        _isClicking = false;
        _clickRoutine = null;
    }
}
