using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour,IManager {

    static MapManager m_inst;
    public static MapManager GetInst
    {
        get { return m_inst; }
    }
    public MapManager()
    {
        m_inst = this;
    }

    public MapModel m_model;
    public MapView m_view;


    public void AwakeMgr()
    {
        m_model = Utils.MakeObjectWithComponent<MapModel>("MapModel", this.gameObject);
        m_model.Init();

        m_view = Utils.MakeObjectWithComponent<MapView>("MapView", this.gameObject);
        m_view.Init(m_model);
    }
    public void StartMgr()
    {
    
    }
    public void UpdateMgr()
    {

    }

    public TileData GetTileData(int _x,int _y)
    {
        return m_model.m_tileDataAry[_x][_y];
    }
    public TileData GetTileData(Tile _tile)
    {
        return GetTileData(_tile.m_xIndex, _tile.m_yIndex);
    }
    public Tile GetTile(TileData _data)
    {
        return GetTile(_data.m_xIndex, _data.m_yIndex);
    }
    public Tile GetTile(int _x,int _y)
    {
        return m_view.m_tileAry[_x][_y];
    }


    public bool IsValidMoveIndex(int _x,int _y)
    {
        if (_x >= m_model.m_mapWidth || _x < 0)
            return false;

        if (_y >= m_model.m_mapHeight || _y < 0)
            return false;

        TileData data = GetTileData(_x, _y);

        if (data.m_isObs)
            return false;

        return true;
    }
    public TileData GetValidRandomTileData()
    {
        return GetTileData(m_model.GetValidRandomX(), m_model.GetValidRandomY());
    }
    public Vector3 GetTilePosWithIndice(int _x ,int _y)
    {
        return m_view.m_tileAry[_x][_y].transform.position;
    }

    public void SceneChanged()
    {

    }


    public void RegenMap()
    {
        m_model.RegenMap();
        m_view.RegenMap(m_model);
    }
}
