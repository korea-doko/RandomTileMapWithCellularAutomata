using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Tile : MonoBehaviour
{
    public SpriteRenderer m_ren;

    public int m_xIndex;
    public int m_yIndex;

    public void Init(int _xIndex, int _yIndex)
    {
        m_ren = this.GetComponent<SpriteRenderer>();

        m_xIndex = _xIndex;
        m_yIndex = _yIndex;
    }   
    public void ChangeSprite(Sprite _sp)
    {
        m_ren.sprite = _sp;
    }
}
