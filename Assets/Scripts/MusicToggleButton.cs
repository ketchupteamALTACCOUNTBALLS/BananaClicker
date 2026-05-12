using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Toggles background music mute state and swaps between the open/mute speaker sprites.
/// Supports the same click-scale animation as the banana.
/// Attach to either speaker button — assign both renderers and the AudioSource in the Inspector.
/// </summary>
public class MusicToggleButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private SpriteRenderer _openSpeakerRenderer;
    [SerializeField] private SpriteRenderer _muteSpeakerRenderer;
    [SerializeField] private AudioSource _bgMusic;

    [Header("Music Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float _musicVolume = 1f;

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
    private bool _isMuted;
    private Coroutine _clickRoutine;    private void Awake()
    {
        _baseScale = transform.localScale;
        EnsureCollider();
        ApplyVolume();
    }

    private void OnValidate()
    {
        // Lets you hear the volume change immediately while tweaking the slider in the Editor.
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        if (_bgMusic != null)
            _bgMusic.volume = _musicVolume;
    }

    private void EnsureCollider()
    {
        if (GetComponent<Collider2D>() == null)
            gameObject.AddComponent<BoxCollider2D>();
    }

    private void Update()
    {
        ApplyScale();
    }

    // ──────────────────────────────────────────────
    // Scale animation (same pattern as BananaInteraction)
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
        if (_clickRoutine != null)
            StopCoroutine(_clickRoutine);
        _clickRoutine = StartCoroutine(ClickRoutine());

        ToggleMute();
    }

    private IEnumerator ClickRoutine()
    {
        _isClicking = true;
        yield return new WaitForSeconds(_clickDuration);
        _isClicking = false;
        _clickRoutine = null;
    }

    // ──────────────────────────────────────────────
    // Mute toggle
    // ──────────────────────────────────────────────

    private void ToggleMute()
    {
        if (_bgMusic == null) return;

        // Read current state from the source so both button instances stay in sync.
        bool nowMuted = !_bgMusic.mute;
        _bgMusic.mute = nowMuted;

        if (_openSpeakerRenderer != null)
            _openSpeakerRenderer.enabled = !nowMuted;

        if (_muteSpeakerRenderer != null)
            _muteSpeakerRenderer.enabled = nowMuted;
    }
}
