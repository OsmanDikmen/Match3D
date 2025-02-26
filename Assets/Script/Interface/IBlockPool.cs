using UnityEngine;

public interface IBlockPool
{
    IBlock GetBlock(int colorID, Vector2Int gridPos); // Havuzdan bir blok alýr
    void ReturnBlock(IBlock block); // Bloðu havuza geri koyar
}

