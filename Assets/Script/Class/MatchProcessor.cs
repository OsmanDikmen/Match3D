using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MatchProcessor
{
    // Oyun tahtasını temsil eden izgara, blok havuzu
    private IBlock[,] _grid;
    private IBlockPool _blockPool;
    private readonly int _width, _height;
    private bool[,] _visited;

    // Bellek yonetimi icin nesne havuzlari
    private Stack<Vector2Int> _positionPool;
    private List<Vector2Int> _groupPool;
    private List<IBlock> _pendingVisualUpdates;
    private List<Vector2Int[]> _swapPairsPool;
    private List<IBlock> _blocksPool;

    // Eslestirme islemcisini baslatma
    public MatchProcessor(IBlock[,] grid, IBlockPool blockPool, int width, int height)
    {
        _grid = grid;
        _blockPool = blockPool;
        _width = width;
        _height = height;
        _visited = new bool[_height, _width];

        InitializePools();
    }

    // Havuzlari maksimum boyutta baslatma
    private void InitializePools()
    {
        int maxSize = _width * _height;
        _positionPool = new Stack<Vector2Int>(maxSize);
        _groupPool = new List<Vector2Int>(maxSize);
        _pendingVisualUpdates = new List<IBlock>(maxSize);
        _swapPairsPool = new List<Vector2Int[]>(maxSize);
        _blocksPool = new List<IBlock>(maxSize);
    }

    // Kullanici tiklamalarini isleme
    public void ProcessClick(Vector2Int gridPos)
    {
        ResetVisited();
        var group = FindConnectedGroup(gridPos);
        if (group.Count >= 2)
        {
            ProcessGroup(group);
            CollapseColumns();

            // Tahtanin kilitleni kontrolu
            if (CheckDeadlock())
            {
                ShuffleBoard();
            }
        }
    }

    // Ziyaret edilmis blok kontrollerini sifirlama
    private void ResetVisited()
    {
        Array.Clear(_visited, 0, _height * _width);
    }

    // Ayni renkli bagli blok grubunu bulma
    private List<Vector2Int> FindConnectedGroup(Vector2Int startPos)
    {
        _positionPool.Clear();
        _groupPool.Clear();

        if (!IsValidPosition(startPos, _grid[startPos.y, startPos.x].ColorID))
            return _groupPool;

        int targetColor = _grid[startPos.y, startPos.x].ColorID;

        _positionPool.Push(startPos);
        while (_positionPool.Count > 0)
        {
            var pos = _positionPool.Pop();
            if (!_visited[pos.y, pos.x] && IsValidPosition(pos, targetColor))
            {
                _groupPool.Add(pos);
                _visited[pos.y, pos.x] = true;

                // Komsu pozisyonlarin sinir kontrolu
                if (pos.x + 1 < _width) _positionPool.Push(new Vector2Int(pos.x + 1, pos.y));
                if (pos.x > 0) _positionPool.Push(new Vector2Int(pos.x - 1, pos.y));
                if (pos.y + 1 < _height) _positionPool.Push(new Vector2Int(pos.x, pos.y + 1));
                if (pos.y > 0) _positionPool.Push(new Vector2Int(pos.x, pos.y - 1));
            }
        }
        return _groupPool;
    }

    // Pozisyon ve renk kontrolu
    private bool IsValidPosition(Vector2Int pos, int targetColor)
    {
        return pos.x >= 0 && pos.x < _width &&
               pos.y >= 0 && pos.y < _height &&
               _grid[pos.y, pos.x] != null &&
               _grid[pos.y, pos.x].ColorID == targetColor;
    }

    // Blok grubunu isleme
    private void ProcessGroup(List<Vector2Int> group)
    {
        _pendingVisualUpdates.Clear();
        foreach (var pos in group)
        {
            var block = _grid[pos.y, pos.x];
            _pendingVisualUpdates.Add(block);
            _blockPool.ReturnBlock(block);
            _grid[pos.y, pos.x] = null;
        }

        UpdateVisuals();
    }

    // Gorsellestirme guncelleme
    private void UpdateVisuals()
    {
        foreach (var block in _pendingVisualUpdates)
        {
            block.UpdateVisual();
        }
        _pendingVisualUpdates.Clear();
    }

    // Sutunlari dusurme ve yeni blok olusturma
    private void CollapseColumns()
    {
        for (int x = 0; x < _width; x++)
        {
            int writeIndex = _height - 1;

            // Mevcut blokları asagi tasima
            for (int y = _height - 1; y >= 0; y--)
            {
                if (_grid[y, x] != null)
                {
                    if (y != writeIndex)
                    {
                        _grid[writeIndex, x] = _grid[y, x];
                        _grid[writeIndex, x].MoveToPosition(new Vector2Int(x, -writeIndex));
                        _grid[y, x] = null;
                    }
                    writeIndex--;
                }
            }

            // Bos alanlara yeni bloklar olusturma
            for (int y = writeIndex; y >= 0; y--)
            {
                int colorID = UnityEngine.Random.Range(0, 6);
                var spawnPosition = new Vector2Int(x, _height + (writeIndex - y));
                var newBlock = _blockPool.GetBlock(colorID, spawnPosition);
                newBlock.Initialize(colorID, spawnPosition);
                newBlock.MoveToPosition(new Vector2Int(x, -y));
                _grid[y, x] = newBlock;
            }
        }

        // Toplu gorsel guncelleme
        UpdateVisuals();
    }

    // Tahtanin kilitli olup olmadigini kontrol etme
    private bool CheckDeadlock()
    {
        ResetVisited();
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                if (!_visited[y, x] && _grid[y, x] != null)
                {
                    var group = FindConnectedGroup(new Vector2Int(x, y));
                    if (group.Count >= 2)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    // Tahtayi karistirma
    private void ShuffleBoard()
    {
        _swapPairsPool.Clear();
        _blocksPool.Clear();

        // Minimal karistirma icin takas ciftlerini bulma
        FindSwapPairs();

        // Minimal karistirmayi deneme, basarisiz olursa tam karistirma
        if (!TryMinimalShuffle())
        {
            PerformFullShuffle();
        }
    }

    // Benzer renkli blok takas ciftlerini bulma
    private void FindSwapPairs()
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                // Yatay komsu kontrolleri
                if (x < _width - 1)
                {
                    IBlock current = _grid[y, x];
                    IBlock right = _grid[y, x + 1];
                    if (AreColorsSimilar(current, right))
                    {
                        _swapPairsPool.Add(new Vector2Int[] {
                            new Vector2Int(x, y),
                            new Vector2Int(x + 1, y)
                        });
                    }
                }
                // Dikey komsu kontrolleri
                if (y < _height - 1)
                {
                    IBlock current = _grid[y, x];
                    IBlock down = _grid[y + 1, x];
                    if (AreColorsSimilar(current, down))
                    {
                        _swapPairsPool.Add(new Vector2Int[] {
                            new Vector2Int(x, y),
                            new Vector2Int(x, y + 1)
                        });
                    }
                }
            }
        }
    }

    // Minimal karistirma islemi
    private bool TryMinimalShuffle()
    {
        // Fisher-Yates karistirma algoritmasi
        for (int i = _swapPairsPool.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var temp = _swapPairsPool[i];
            _swapPairsPool[i] = _swapPairsPool[j];
            _swapPairsPool[j] = temp;
        }

        // Gecerli tahta durumu bulunana kadar karsilastirma
        foreach (var pair in _swapPairsPool)
        {
            SwapBlocks(pair[0], pair[1]);

            if (!CheckDeadlock())
            {
                return true;
            }
        }

        return false;
    }

    // Tam karistirma islemi
    private void PerformFullShuffle()
    {
        // Tum blokları toplama
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                if (_grid[y, x] != null)
                {
                    _blocksPool.Add(_grid[y, x]);
                }
            }
        }

        // Fisher-Yates karistirma algoritmasi
        for (int i = _blocksPool.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var temp = _blocksPool[i];
            _blocksPool[i] = _blocksPool[j];
            _blocksPool[j] = temp;
        }

        // Karistirilmis blokları yeniden dagitma
        int index = 0;
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                if (_grid[y, x] != null)
                {
                    _grid[y, x] = _blocksPool[index];
                    _grid[y, x].MoveToPosition(new Vector2Int(x, -y));
                    index++;
                }
            }
        }

        UpdateVisuals();
    }

    // Blok takas islemi
    private void SwapBlocks(Vector2Int pos1, Vector2Int pos2)
    {
        IBlock temp = _grid[pos1.y, pos1.x];
        _grid[pos1.y, pos1.x] = _grid[pos2.y, pos2.x];
        _grid[pos2.y, pos2.x] = temp;

        // Blok konumlarını guncelleme
        if (_grid[pos1.y, pos1.x] != null)
            _grid[pos1.y, pos1.x].MoveToPosition(new Vector2Int(pos1.x, -pos1.y));
        if (_grid[pos2.y, pos2.x] != null)
            _grid[pos2.y, pos2.x].MoveToPosition(new Vector2Int(pos2.x, -pos2.y));

        // Gorsel guncelleme listesine ekleme
        _pendingVisualUpdates.Add(_grid[pos1.y, pos1.x]);
        _pendingVisualUpdates.Add(_grid[pos2.y, pos2.x]);
    }

    // Blok renklerinin benzerlik kontrolu
    private bool AreColorsSimilar(IBlock a, IBlock b)
    {
        if (a == null || b == null) return false;
        int diff = Mathf.Abs(a.ColorID - b.ColorID);
        return diff == 1 || diff == 5;
    }
}