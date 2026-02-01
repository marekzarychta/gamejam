using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class Enlarge : MonoBehaviour
{
    [Header("Settings")]
    [Min(0.01f)] public float scaleFactor = 2.0f;
    [Min(0.01f)] public float duration = 0.5f;

    [Header("Anchor")]
    public bool keepBottomOnGround = true;

    [SerializeField] private Collider anchorCollider;
    [SerializeField] private Renderer anchorRenderer;

    private ScaleTweenHost _host;
    private Coroutine _tween;

    private Vector3 _baseScale;
    private bool _hasBaseScale;

    private void Reset()
    {
        anchorCollider = GetComponent<Collider>();
        if (anchorCollider == null) anchorCollider = GetComponentInChildren<Collider>();
        anchorRenderer = GetComponentInChildren<Renderer>();
    }

    private void Awake()
    {
        if (anchorCollider == null)
        {
            anchorCollider = GetComponent<Collider>();
            if (anchorCollider == null) anchorCollider = GetComponentInChildren<Collider>();
        }
        if (anchorRenderer == null) anchorRenderer = GetComponentInChildren<Renderer>();

        _host = GetComponent<ScaleTweenHost>();
        if (_host == null) _host = gameObject.AddComponent<ScaleTweenHost>();
    }

    private void OnEnable()
    {
        StopTween();

        if (!_hasBaseScale || IsClose(transform.localScale, _baseScale))
        {
            _baseScale = transform.localScale;
            _hasBaseScale = true;
        }

        Vector3 targetScale = _baseScale * scaleFactor;
        _tween = _host.StartCoroutine(TweenScale(transform.localScale, targetScale, clearBaseOnComplete: false));
    }

    private void OnDisable()
    {
        if (_host == null) return;

        StopTween();

        // Upewniamy się, że mamy zapisaną bazową skalę
        if (!_hasBaseScale)
        {
            _baseScale = transform.localScale;
            _hasBaseScale = true;
        }

        // --- POPRAWKA ---
        // Sprawdzamy, czy GameObject jest w ogóle włączony.
        if (gameObject.activeInHierarchy)
        {
            // Obiekt jest aktywny (tylko komponent został wyłączony), więc animujemy zmniejszanie
            _tween = _host.StartCoroutine(TweenScale(transform.localScale, _baseScale, clearBaseOnComplete: true));
        }
        else
        {
            // Obiekt został całkowicie wyłączony (SetActive false).
            // Nie możemy uruchomić Coroutine, więc przywracamy skalę natychmiastowo ("na sztywno").
            transform.localScale = _baseScale;
            _hasBaseScale = false;
        }
        // ----------------
    }

    private void StopTween()
    {
        if (_host != null && _tween != null)
        {
            _host.StopCoroutine(_tween);
            _tween = null;
        }
    }

    private IEnumerator TweenScale(Vector3 startScale, Vector3 endScale, bool clearBaseOnComplete)
    {
        float bottomTarget = keepBottomOnGround ? GetBottomY() : 0f;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(time / duration));

            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            if (anchorCollider != null) Physics.SyncTransforms();

            if (keepBottomOnGround)
            {
                float bottomNow = GetBottomY();
                transform.position += Vector3.up * (bottomTarget - bottomNow);
            }

            yield return null;
        }

        transform.localScale = endScale;
        if (anchorCollider != null) Physics.SyncTransforms();

        if (keepBottomOnGround)
        {
            float bottomNow = GetBottomY();
            transform.position += Vector3.up * (bottomTarget - bottomNow);
        }

        _tween = null;
        if (clearBaseOnComplete) _hasBaseScale = false;
    }

    private float GetBottomY()
    {
        if (anchorCollider != null) return anchorCollider.bounds.min.y;
        if (anchorRenderer != null) return anchorRenderer.bounds.min.y;
        return transform.position.y;
    }

    private static bool IsClose(Vector3 a, Vector3 b) => (a - b).sqrMagnitude < 1e-6f;
}
