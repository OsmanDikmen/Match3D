using UnityEngine;

public interface IBlock
{
    int ColorID { get; } // Blo�un rengini temsil eden ID
    int ShapeID { get; } // Blo�un �ekil ID'si (�rne�in, "circle", "square" vs.)
    Vector2Int GridPosition { get; set; } // Blo�un bulundu�u �zgara konumu
    void Initialize(int colorID, Vector2Int gridPosition); // Blo�u ba�lat�r
    void SetIconStrategy(IIconStrategy strategy); // �kon stratejisini belirler
    void UpdateVisual(); // G�rsel g�ncellemeleri uygular
    void Release(); // Blo�u devre d��� b�rak�r

    void MoveToPosition(Vector2Int targetPosition);// Blo�u belirtilen hedef konuma ta��r. Grid �zerindeki yerini de�i�tirir.

    void SetShapeID(int groupSize); // Blo�un g�rsel temsilini de�i�tirmek i�in yeni bir sprite ayarlar.
    void ResetToOriginalShape(); // Blo�un sprite'�n� orijinal haline geri d�nd�r�r.
    void Activate(); // Blo�u aktif hale getirir.

}

