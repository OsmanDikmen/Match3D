using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleIconStrategy : MonoBehaviour, IIconStrategy
{
    [SerializeField] private Sprite[] _sprites = new Sprite[24]; //24 farkl� sprite'� i�eren dizi



    public Sprite GetIcon(int colorID)
    {
        return _sprites[colorID];
    }
}
