using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Block : MonoBehaviour, IBlock
{
    // Blok hareket suresi
    [SerializeField] private float _moveDuration = 0.3f;

    // Blok renk ve sekil ozellikleri
    public int ColorID { get; private set; }
    public int ShapeID { get; private set; }
    public Vector2Int GridPosition { get; set; }

    // Gorsel bilesenler
    private int _originalShapeID;
    private SpriteRenderer _renderer;
    [SerializeField] private IIconStrategy _iconStrategy;

    // Performans icin transform onbellegi
    private Transform _cachedTransform;

    // Baslangic ayarlarini yapma
    private void Awake()
    {
        _cachedTransform = transform;
        _renderer = GetComponent<SpriteRenderer>();
    }

    // Blok ozelliklerini ilk defa ayarlama
    public void Initialize(int colorID, Vector2Int gridPosition)
    {
        ColorID = colorID;
        _originalShapeID = colorID;
        ShapeID = colorID;
        GridPosition = gridPosition;

        // cached transform
        _cachedTransform.position = new Vector3(gridPosition.x, -gridPosition.y, 0);
    }

    // Blogu yeni konuma tasima
    public void MoveToPosition(Vector2Int targetPosition)
    {
        GridPosition = targetPosition;
        StartCoroutine(SmoothMove(targetPosition));
    }

    // Yumusak hareket animasyonu
    private IEnumerator SmoothMove(Vector2Int target)
    {
        Vector3 startPos = _cachedTransform.position;
        Vector3 endPos = new Vector3(target.x, target.y, 0);
        float elapsed = 0;

        // Time.deltaTime'ý cache'leme
        float deltaTime = Time.deltaTime;

        while (elapsed < _moveDuration)
        {
            _cachedTransform.position = Vector3.Lerp(startPos, endPos, elapsed / _moveDuration);
            elapsed += deltaTime;
            deltaTime = Time.deltaTime;
            yield return null;
        }

        _cachedTransform.position = endPos;
    }

    public void SetIconStrategy(IIconStrategy strategy) => _iconStrategy = strategy;

    // Blok gorselini guncelleme
    public void UpdateVisual()
    {
        if (_renderer != null && _iconStrategy != null)
        {
            _renderer.sprite = _iconStrategy.GetIcon(ColorID);
        }
    }

    // Blogu orijinal sekline dondurme
    public void ResetToOriginalShape()
    {
        ShapeID = _originalShapeID;
        if (_renderer != null && _iconStrategy != null)
        {
            _renderer.sprite = _iconStrategy.GetIcon(ShapeID);
        }
    }

    // Yeni sekil atama
    public void SetShapeID(int groupSize)
    {
        if (_renderer != null && _iconStrategy != null)
        {
            _renderer.sprite = _iconStrategy.GetIcon(groupSize);
        }
    }

    // Nesne havuzuna iade etme
    public void Release()
    {
        gameObject.SetActive(false);
    }

    // Nesne havuzundan aktif etme
    public void Activate()
    {
        gameObject.SetActive(true);
    }

    
}
