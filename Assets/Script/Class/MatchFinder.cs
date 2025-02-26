using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class MatchFinder
{
    private readonly IBlock[,] _grid; // Oyun tahtasýný temsil eden 2D dizi
    private readonly IBlockPool _blockPool; // Blok havuzu
    private readonly int _width, _height; // Tahtanýn geniþliði ve yüksekliði
    private readonly bool[,] _visited; // Hangi hücrelerin ziyaret edildiðini takip eden dizi

    // Nesne havuzlarý
    private readonly Stack<Vector2Int> _positionPool; // Geçici pozisyonlarý tutan yýðýn (stack)
    private readonly List<Vector2Int> _groupPool; // Ayný renkteki baðlý bloklarý saklayan liste

    public MatchFinder(IBlock[,] grid, IBlockPool blockPool, int width, int height)
    {
        _grid = grid;
        _blockPool = blockPool;
        _width = width;
        _height = height;
        _visited = new bool[_height, _width];

        // Nesne havuzlarýný baþlat
        int maxSize = _width * _height;
        _positionPool = new Stack<Vector2Int>(maxSize);
        _groupPool = new List<Vector2Int>(maxSize);
    }

    // Tüm oyun tahtasýnda eþleþme olup olmadýðýný kontrol eder
    public void FindMatches()
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                FindItems(new Vector2Int(x, y)); // Her hücre için eþleþme kontrolü yap
            }
        }
    }

    // Belirtilen hücre için eþleþmeleri bulur ve uygun iþlemi yapar
    public void FindItems(Vector2Int gridPos)
    {
        if (_grid[gridPos.y, gridPos.x] == null) return; // Hücre boþsa iþlem yapma

        ResetVisited(); // Ziyaret edilenleri sýfýrla
        var group = FindConnectedGroup(gridPos); // Baðlý bloklarý bul

        IBlock block = _grid[gridPos.y, gridPos.x];
        if (block == null) return;

        // Eþleþen bloklarýn büyüklüðüne göre iþlem yap
        if (group.Count >= 10)
        {
            LargeOne(gridPos);
        }
        else if (group.Count >= 7)
        {
            MediumOne(gridPos);
        }
        else if (group.Count >= 4)
        {
            SmallOne(gridPos);
        }
        else
        {
            block.ResetToOriginalShape(); // Eðer küçükse varsayýlan þekline dön
        }
    }

    // Ziyaret edilen hücreleri sýfýrlar
    private void ResetVisited()
    {
        Array.Clear(_visited, 0, _height * _width);
    }

    // Belirtilen pozisyondan baþlayarak baðlý ayný renkli bloklarý bulur
    private List<Vector2Int> FindConnectedGroup(Vector2Int startPos)
    {
        _positionPool.Clear(); // Pozisyon havuzunu temizle
        _groupPool.Clear(); // Grup listesini temizle

        if (_grid[startPos.y, startPos.x] == null) return _groupPool; // Hücre boþsa iþlem yapma

        int targetColor = _grid[startPos.y, startPos.x].ColorID; // Baþlangýç rengini al

        _positionPool.Push(startPos); // Baþlangýç pozisyonunu stack'e ekle
        while (_positionPool.Count > 0)
        {
            var pos = _positionPool.Pop(); // Stack'ten pozisyon çýkar
            if (IsValidPosition(pos, targetColor)) // Geçerli mi kontrol et
            {
                _groupPool.Add(pos); // Geçerliyse gruba ekle
                _visited[pos.y, pos.x] = true; // Ziyaret edildi olarak iþaretle

                // Komþu hücreleri sýraya ekle
                if (pos.x + 1 < _width) _positionPool.Push(new Vector2Int(pos.x + 1, pos.y));
                if (pos.x > 0) _positionPool.Push(new Vector2Int(pos.x - 1, pos.y));
                if (pos.y + 1 < _height) _positionPool.Push(new Vector2Int(pos.x, pos.y + 1));
                if (pos.y > 0) _positionPool.Push(new Vector2Int(pos.x, pos.y - 1));
            }
        }
        return _groupPool;
    }

    // Pozisyonun geçerli olup olmadýðýný kontrol eder
    private bool IsValidPosition(Vector2Int pos, int targetColor)
    {
        return pos.x >= 0 && pos.x < _width &&
               pos.y >= 0 && pos.y < _height &&
               !_visited[pos.y, pos.x] &&
               _grid[pos.y, pos.x] != null &&
               _grid[pos.y, pos.x].ColorID == targetColor;
    }

    // Küçük eþleþme grubu için þekil deðiþtirir
    private void SmallOne(Vector2Int gridPos)
    {
        IBlock block = _grid[gridPos.y, gridPos.x];
        if (block == null) return;

        int[] smallShapeMap = { 6, 9, 12, 15, 18, 21 }; // Lookup table
        int colorId = block.ColorID;

        if (colorId >= 0 && colorId < smallShapeMap.Length)
        {
            block.SetShapeID(smallShapeMap[colorId]);
        }
        else
        {
            block.SetShapeID(colorId);
        }
    }

    // Orta büyüklükte eþleþme grubu için þekil deðiþtirir
    private void MediumOne(Vector2Int gridPos)
    {
        IBlock block = _grid[gridPos.y, gridPos.x];
        if (block == null) return;

        int[] mediumShapeMap = { 7, 10, 13, 16, 19, 22 }; // Lookup table
        int colorId = block.ColorID;

        if (colorId >= 0 && colorId < mediumShapeMap.Length)
        {
            block.SetShapeID(mediumShapeMap[colorId]);
        }
        else
        {
            block.SetShapeID(colorId);
        }
    }

    // Büyük eþleþme grubu için þekil deðiþtirir
    private void LargeOne(Vector2Int gridPos)
    {
        IBlock block = _grid[gridPos.y, gridPos.x];
        if (block == null) return;

        int[] largeShapeMap = { 8, 11, 14, 17, 20, 23 }; // Lookup table
        int colorId = block.ColorID;

        if (colorId >= 0 && colorId < largeShapeMap.Length)
        {
            block.SetShapeID(largeShapeMap[colorId]);
        }
        else
        {
            block.SetShapeID(colorId);
        }
    }
}
