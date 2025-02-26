using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class MatchFinder
{
    private readonly IBlock[,] _grid; // Oyun tahtas�n� temsil eden 2D dizi
    private readonly IBlockPool _blockPool; // Blok havuzu
    private readonly int _width, _height; // Tahtan�n geni�li�i ve y�ksekli�i
    private readonly bool[,] _visited; // Hangi h�crelerin ziyaret edildi�ini takip eden dizi

    // Nesne havuzlar�
    private readonly Stack<Vector2Int> _positionPool; // Ge�ici pozisyonlar� tutan y���n (stack)
    private readonly List<Vector2Int> _groupPool; // Ayn� renkteki ba�l� bloklar� saklayan liste

    public MatchFinder(IBlock[,] grid, IBlockPool blockPool, int width, int height)
    {
        _grid = grid;
        _blockPool = blockPool;
        _width = width;
        _height = height;
        _visited = new bool[_height, _width];

        // Nesne havuzlar�n� ba�lat
        int maxSize = _width * _height;
        _positionPool = new Stack<Vector2Int>(maxSize);
        _groupPool = new List<Vector2Int>(maxSize);
    }

    // T�m oyun tahtas�nda e�le�me olup olmad���n� kontrol eder
    public void FindMatches()
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                FindItems(new Vector2Int(x, y)); // Her h�cre i�in e�le�me kontrol� yap
            }
        }
    }

    // Belirtilen h�cre i�in e�le�meleri bulur ve uygun i�lemi yapar
    public void FindItems(Vector2Int gridPos)
    {
        if (_grid[gridPos.y, gridPos.x] == null) return; // H�cre bo�sa i�lem yapma

        ResetVisited(); // Ziyaret edilenleri s�f�rla
        var group = FindConnectedGroup(gridPos); // Ba�l� bloklar� bul

        IBlock block = _grid[gridPos.y, gridPos.x];
        if (block == null) return;

        // E�le�en bloklar�n b�y�kl���ne g�re i�lem yap
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
            block.ResetToOriginalShape(); // E�er k���kse varsay�lan �ekline d�n
        }
    }

    // Ziyaret edilen h�creleri s�f�rlar
    private void ResetVisited()
    {
        Array.Clear(_visited, 0, _height * _width);
    }

    // Belirtilen pozisyondan ba�layarak ba�l� ayn� renkli bloklar� bulur
    private List<Vector2Int> FindConnectedGroup(Vector2Int startPos)
    {
        _positionPool.Clear(); // Pozisyon havuzunu temizle
        _groupPool.Clear(); // Grup listesini temizle

        if (_grid[startPos.y, startPos.x] == null) return _groupPool; // H�cre bo�sa i�lem yapma

        int targetColor = _grid[startPos.y, startPos.x].ColorID; // Ba�lang�� rengini al

        _positionPool.Push(startPos); // Ba�lang�� pozisyonunu stack'e ekle
        while (_positionPool.Count > 0)
        {
            var pos = _positionPool.Pop(); // Stack'ten pozisyon ��kar
            if (IsValidPosition(pos, targetColor)) // Ge�erli mi kontrol et
            {
                _groupPool.Add(pos); // Ge�erliyse gruba ekle
                _visited[pos.y, pos.x] = true; // Ziyaret edildi olarak i�aretle

                // Kom�u h�creleri s�raya ekle
                if (pos.x + 1 < _width) _positionPool.Push(new Vector2Int(pos.x + 1, pos.y));
                if (pos.x > 0) _positionPool.Push(new Vector2Int(pos.x - 1, pos.y));
                if (pos.y + 1 < _height) _positionPool.Push(new Vector2Int(pos.x, pos.y + 1));
                if (pos.y > 0) _positionPool.Push(new Vector2Int(pos.x, pos.y - 1));
            }
        }
        return _groupPool;
    }

    // Pozisyonun ge�erli olup olmad���n� kontrol eder
    private bool IsValidPosition(Vector2Int pos, int targetColor)
    {
        return pos.x >= 0 && pos.x < _width &&
               pos.y >= 0 && pos.y < _height &&
               !_visited[pos.y, pos.x] &&
               _grid[pos.y, pos.x] != null &&
               _grid[pos.y, pos.x].ColorID == targetColor;
    }

    // K���k e�le�me grubu i�in �ekil de�i�tirir
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

    // Orta b�y�kl�kte e�le�me grubu i�in �ekil de�i�tirir
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

    // B�y�k e�le�me grubu i�in �ekil de�i�tirir
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
