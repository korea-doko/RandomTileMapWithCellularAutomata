using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapView : MonoBehaviour
{

    public Tile[][] m_tileAry;
    
    public void Init(MapModel _model)
    {
        InitTile(_model);

        testColoring();
    }

    public void UpdateView()
    {

    }

    
    public void RegenMap(MapModel _model)
    {
        for (int y = _model.m_mapHeight - 1; y > -1; y--)
        {
            for (int x = 0; x < _model.m_mapWidth; x++)
            {
                TileData data = _model.m_tileDataAry[x][y];

                if (data.m_isObs)
                    m_tileAry[x][y].ChangeSprite(_model.GetTileSprite(TileSpriteType.Obstacle));
                else
                    m_tileAry[x][y].ChangeSprite(_model.GetTileSprite(TileSpriteType.Normal));

            }
        }

        testColoring();
    }

    void InitTile(MapModel _model)
    {
        GameObject tilePrefab = Resources.Load("PlayScene/Prefabs/Tile") as GameObject;
        SpriteRenderer tilePrefabRen = tilePrefab.GetComponent<SpriteRenderer>();

        m_tileAry = new Tile[_model.m_mapWidth][];

        for (int i = 0; i < _model.m_mapWidth; i++)
            m_tileAry[i] = new Tile[_model.m_mapHeight];

        for (int y = _model.m_mapHeight - 1; y > -1; y--)
        {
            for (int x = 0; x < _model.m_mapWidth; x++)
            {
                TileData data = _model.m_tileDataAry[x][y];

                m_tileAry[x][y] = ((GameObject)Instantiate(tilePrefab)).GetComponent<Tile>();
                m_tileAry[x][y].Init(x, y);
                m_tileAry[x][y].transform.SetParent(this.transform);
                m_tileAry[x][y].transform.position = new Vector3(tilePrefabRen.size.x * x, tilePrefabRen.size.y * y, 0);

                if (data.m_isObs)
                    m_tileAry[x][y].ChangeSprite(_model.GetTileSprite(TileSpriteType.Obstacle));
            }
        }
    }

    void testColoring()
    {
        //클리어
        Color clearCol = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        Color edgeColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);


        for(int y = 0; y < MapManager.GetInst.m_model.m_mapHeight; y++  )
        {
            for (int x = 0; x < MapManager.GetInst.m_model.m_mapWidth; x++)
            {
                m_tileAry[x][y].m_ren.color = clearCol;
            }
        }

        for(int i = 0; i < MapManager.GetInst.m_model.m_roomList.Count;i++)
        {
            Room r = MapManager.GetInst.m_model.m_roomList[i];
            Color color = new Color
                (
                UnityEngine.Random.Range(0.0f, 1.0f),
                UnityEngine.Random.Range(0.0f, 1.0f),
                UnityEngine.Random.Range(0.0f, 1.0f),
                1.0f
                );

            for(int k = 0; k < r.m_tileList.Count;k++)
            {
                int x = r.m_tileList[k].m_xIndex;
                int y = r.m_tileList[k].m_yIndex;

                m_tileAry[x][y].m_ren.color = color;
            }           
        }


        for(int i = 0; i < MapManager.GetInst.m_model.m_passageList.Count;i++)
        {
            Passage passage = MapManager.GetInst.m_model.m_passageList[i];

            //m_tileAry[passage.m_startX][passage.m_startY].m_ren.color = edgeColor;
            //m_tileAry[passage.m_endX][passage.m_endY].m_ren.color = edgeColor;

            Debug.DrawLine(m_tileAry[passage.m_startX][passage.m_startY].transform.position,
                m_tileAry[passage.m_endX][passage.m_endY].transform.position, Color.red, 6.0f, false);
        }
    }
   
}
