using UnityEngine;

public interface IBlock
{
    int ColorID { get; } // Bloðun rengini temsil eden ID
    int ShapeID { get; } // Bloðun þekil ID'si (örneðin, "circle", "square" vs.)
    Vector2Int GridPosition { get; set; } // Bloðun bulunduðu ýzgara konumu
    void Initialize(int colorID, Vector2Int gridPosition); // Bloðu baþlatýr
    void SetIconStrategy(IIconStrategy strategy); // Ýkon stratejisini belirler
    void UpdateVisual(); // Görsel güncellemeleri uygular
    void Release(); // Bloðu devre dýþý býrakýr

    void MoveToPosition(Vector2Int targetPosition);// Bloðu belirtilen hedef konuma taþýr. Grid üzerindeki yerini deðiþtirir.

    void SetShapeID(int groupSize); // Bloðun görsel temsilini deðiþtirmek için yeni bir sprite ayarlar.
    void ResetToOriginalShape(); // Bloðun sprite'ýný orijinal haline geri döndürür.
    void Activate(); // Bloðu aktif hale getirir.

}

