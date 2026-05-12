using TMPro;
using UnityEngine;

/// <summary>
/// Keeps a world-space sprite (coin pixel_0) anchored to the right edge of
/// a TextMeshProUGUI element so it always sits right beside the text,
/// regardless of how long the number gets.
/// </summary>
public class CoinIconFollower : MonoBehaviour
{
    [Tooltip("The TMP text element the coin should follow.")]
    [SerializeField] private TextMeshProUGUI _coinText;

    [Tooltip("Horizontal gap in screen pixels between the text and the coin.")]
    [SerializeField] private float _gapScreenPixels = 18f;

    [Tooltip("Fine-tune offset in screen pixels applied on top of the gap. X = horizontal nudge, Y = vertical nudge.")]
    [SerializeField] private Vector2 _offsetScreenPixels = Vector2.zero;

    private Camera _mainCamera;
    private RectTransform _textRect;

    private void Awake()
    {
        _mainCamera = Camera.main;

        if (_coinText != null)
            _textRect = _coinText.GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (_textRect == null || _mainCamera == null)
            return;

        PositionCoinBesideText();
    }

    private void PositionCoinBesideText()
    {
        // GetWorldCorners returns screen pixels for a Screen Space Overlay canvas.
        // corners[0] = bottom-left, [1] = top-left, [2] = top-right, [3] = bottom-right
        Vector3[] corners = new Vector3[4];
        _textRect.GetWorldCorners(corners);

        // Text is left-aligned, so content starts at the rect's left edge.
        // Right edge of rendered text = left edge of rect + preferredWidth.
        float textRightEdge = corners[0].x + _coinText.preferredWidth;
        float rectCenterY   = (corners[0].y + corners[1].y) * 0.5f;

        float coinScreenX = textRightEdge + _gapScreenPixels + _offsetScreenPixels.x;
        float coinScreenY = rectCenterY   + _offsetScreenPixels.y;

        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(
            new Vector3(coinScreenX, coinScreenY, -_mainCamera.transform.position.z));

        transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
    }
}
