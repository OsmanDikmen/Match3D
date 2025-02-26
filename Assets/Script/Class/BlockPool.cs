using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class BlockPool : MonoBehaviour, IBlockPool
{
    [SerializeField] private GameObject _blockPrefab;
    [SerializeField] private int _initialPoolSize = 100;

    // Thread-safe ve performanslý havuz yönetimi
    private ConcurrentQueue<IBlock> _pool = new ConcurrentQueue<IBlock>();

    // Performans için cache
    private IIconStrategy _cachedIconStrategy;

    // Singleton veya dependency injection için hazýrlýk
    private static BlockPool _instance;
    public static BlockPool Instance => _instance;

    private void Awake()
    {
        // Singleton pattern
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Icon strategy'i daha verimli bulma
        _cachedIconStrategy = GetComponentInParent<SimpleIconStrategy>()
            ?? FindObjectOfType<SimpleIconStrategy>();
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        // Toplu blok oluþturma için optimize edilmiþ döngü
        var blocks = new IBlock[_initialPoolSize];
        for (int i = 0; i < _initialPoolSize; i++)
        {
            var obj = Instantiate(_blockPrefab, transform);
            var block = obj.GetComponent<IBlock>();
            block.Release();
            blocks[i] = block;
        }

        // Toplu ekleme
        foreach (var block in blocks)
        {
            _pool.Enqueue(block);
        }
    }

    public IBlock GetBlock(int colorID, Vector2Int gridPos)
    {
        IBlock block;
        if (!_pool.TryDequeue(out block))
        {
            // Havuz boþsa geniþletme mekanizmasý
            CreateNewBlock(out block);
        }

        block.Initialize(colorID, gridPos);
        block.SetIconStrategy(_cachedIconStrategy);
        block.Activate();
        block.UpdateVisual();
        return block;
    }

    public void ReturnBlock(IBlock block)
    {
        block.Release();
        _pool.Enqueue(block);
    }

    private void CreateNewBlock(out IBlock block)
    {
        var obj = Instantiate(_blockPrefab, transform);
        block = obj.GetComponent<IBlock>();
        block.Release();
    }
}
