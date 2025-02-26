using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // Oyun tahtasinin boyutlari
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;

    // Oyun tahtasi ve gerekli bilesenler
    private IBlock[,] _grid;
    private IBlockPool _blockPool;
    private MatchProcessor _matchProcessor;
    private MatchFinder _matchFinder;

    // Performans icin onbellek ve diger yardimci degiskenler
    private Camera _mainCamera;
    private readonly Vector2Int _invalidPosition = new Vector2Int(-1, -1);

    // Nesne havuzu icin pozisyon listesi
    private Queue<Vector2Int> _positionPool;

    private void Awake()
    {
        InitializePools();
        _mainCamera = Camera.main;
    }

    // Pozisyon havuzunu baslatma
    private void InitializePools()
    {
        int maxSize = width * height;
        _positionPool = new Queue<Vector2Int>(maxSize);
        for (int i = 0; i < maxSize; i++)
        {
            _positionPool.Enqueue(new Vector2Int());
        }
    }

    private void Start()
    {
        InitializeComponents();
        InitializeBoard();
        _matchFinder.FindMatches();
    }

    // Gerekli bilesenleri ve referanslari baslatma
    private void InitializeComponents()
    {
        // BlockPool referansýný al
        _blockPool = GetComponent<BlockPool>();
        if (_blockPool == null)
        {
            _blockPool = FindObjectOfType<BlockPool>();
            if (_blockPool == null)
            {
                Debug.LogError("BlockPool component Yok!");
                enabled = false;
                return;
            }
        }

        // Grid'i oluþtur
        _grid = new IBlock[height, width];

        // Eslestirme islemcilerini baslatma
        _matchProcessor = new MatchProcessor(_grid, _blockPool, width, height);
        _matchFinder = new MatchFinder(_grid, _blockPool, width, height);
    }

    // Her kare fare tiklamasi kontrolu
    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2Int gridPos = GetClickedGridPosition();
        if (gridPos == _invalidPosition) return;

       

        
        ProcessGridClick(gridPos);
    }

    // Fare tiklanan konumun izgaradaki karsiligini bulma
    private Vector2Int GetClickedGridPosition()
    {
        Vector2 worldPoint = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = WorldToGridPosition(worldPoint);

        return IsValidGridPosition(gridPos) ? gridPos : _invalidPosition;
    }

    // Tiklanan konumu isleme
    private void ProcessGridClick(Vector2Int gridPos)
    {
        _matchProcessor.ProcessClick(gridPos);
        _matchFinder.FindMatches();
    }

    // Oyun tahtasini baslangic bloklariyla doldurma
    private void InitializeBoard()
    {

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int position;
                if (_positionPool.Count > 0)
                {
                    position = _positionPool.Dequeue();
                    position.x = x;
                    position.y = y;
                }
                else
                {
                    position = new Vector2Int(x, y);
                }

                _grid[y, x] = _blockPool.GetBlock(Random.Range(0, 6), position);

                _positionPool.Enqueue(position);
            }
        }
    }

    // Dunya koordinatlarini Grid koordinatlarýna donusturme
    private Vector2Int WorldToGridPosition(Vector2 worldPos)
    {
        Vector2Int position;
        if (_positionPool.Count > 0)
        {
            position = _positionPool.Dequeue();
            position.x = Mathf.RoundToInt(worldPos.x);
            position.y = Mathf.RoundToInt(-worldPos.y);
        }
        else
        {
            position = new Vector2Int(
                Mathf.RoundToInt(worldPos.x),
                Mathf.RoundToInt(-worldPos.y)
            );
        }

        _positionPool.Enqueue(position);
        return position;
    }

    // Grid pozisyonunun gecerli olup olmadigini kontrol etme
    private bool IsValidGridPosition(Vector2Int pos)
    {
        return pos.x >= 0 &&
               pos.x < width &&
               pos.y >= 0 &&
               pos.y < height &&
               _grid[pos.y, pos.x] != null;
    }

}