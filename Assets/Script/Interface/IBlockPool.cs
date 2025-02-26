using UnityEngine;

public interface IBlockPool
{
    IBlock GetBlock(int colorID, Vector2Int gridPos); // Havuzdan bir blok al�r
    void ReturnBlock(IBlock block); // Blo�u havuza geri koyar
}

