using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileData 
{
    public int m_xIndex;
    public int m_yIndex;

    public bool m_isObs;
    

    public TileData(int _xIndex, int _yIndex)
    {
        m_xIndex = _xIndex;
        m_yIndex = _yIndex;
        m_isObs = false;
    }
    public void SetAsObstacle()
    {
        m_isObs = true;
    }    
}
