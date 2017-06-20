using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TileDir
{
    E,      // 동     0        1,0
    SE,     // 남동   1        1,-1
    S,      // 남     2        0,-1
    SW,     // 남서   3       -1,-1 
    W,      // 서     4       -1,0
    NW,     // 북서   5       -1,1
    N,      // 북     6        0,1
    NE      // 북동   7        1,1
}
public enum TileSpriteType
{
    Normal,
    Obstacle
}


public class Passage
{
    public List<RandomMapGenerateData> m_passageTileList;

    public int m_startX;
    public int m_startY;

    public int m_endX;
    public int m_endY;
    
    public Passage(int _sx,int _sy, int _ex,int _ey)
    {
        m_passageTileList = new List<RandomMapGenerateData>();

        m_startX = _sx;
        m_startY = _sy;

        m_endX = _ex;
        m_endY = _ey;
    }
}
[System.Serializable]
public class Room 
{
    public int m_id;

    public List<Room> m_linkedRoomList;

    public List<RandomMapGenerateData> m_tileList;
    public List<RandomMapGenerateData> m_edgeTileList;
    
    public int m_roomSize;
    public bool m_isMainRoom;
    public bool m_isConnectedToMainRoom;
    

    public Room(int _id)
    {
        m_id = _id;
        m_tileList = new List<RandomMapGenerateData>();
        m_edgeTileList = new List<RandomMapGenerateData>();
        m_linkedRoomList = new List<Room>();
    }
    public void AddGenData(RandomMapGenerateData _data)
    {
        m_roomSize++;
        m_tileList.Add(_data);
    } 
    public void AddEdgeTile(RandomMapGenerateData _data)
    {
        // 이미 들어가 있으면 제외해서 집어넣기

        for(int i = 0; i < m_edgeTileList.Count;i++)
        {
            RandomMapGenerateData inData = m_edgeTileList[i];

            if (_data.m_xIndex == inData.m_xIndex && _data.m_yIndex == inData.m_yIndex)
                return;
        }

        m_edgeTileList.Add(_data);
    }
    public void LinkRoom(Room _room)
    {
        
        if (CheckAlreadyLinkedRoom(_room))
            return;

        if( _room.m_isConnectedToMainRoom )
            SetConnectedToMain();

        m_linkedRoomList.Add(_room);


        _room.LinkRoom(this);
    }
   
    public void SetConnectedToMain()
    {
        m_isConnectedToMainRoom = true;

        for(int i = 0; i < m_linkedRoomList.Count;i++)
        {
            if (!m_linkedRoomList[i].m_isConnectedToMainRoom)
                m_linkedRoomList[i].SetConnectedToMain();
        }
    }
    public bool CheckAlreadyLinkedRoom(Room _room)
    {
        for(int i = 0; i < m_linkedRoomList.Count;i++)
        {
            if (m_linkedRoomList[i].m_id == _room.m_id)
            {
                //Debug.Log(m_id.ToString() + " 의 방에는 이미 " + m_linkedRoomList[i].m_id.ToString() + "의 방이 링크되어있다");
                return true;
            }
        }

        return false;
    }
    public void Clear()
    {
        for (int i = 0; i < m_tileList.Count; i++)
        {
            RandomMapGenerateData data = m_tileList[i];
            data.m_isWall = true;
        }

        for(int i = 0; i < m_edgeTileList.Count;i++)
        {
            RandomMapGenerateData data = m_edgeTileList[i];
            data.m_isWall = true;
        }

        m_tileList.Clear();
        m_edgeTileList.Clear();
    }
}
[System.Serializable]
public class RandomMapGenerateData
{
    public bool m_isWall;
    public bool m_isDetected;
    
    public int m_xIndex;
    public int m_yIndex;
    

    public RandomMapGenerateData(int _xIndex, int _yIndex)
    {
        m_xIndex = _xIndex;
        m_yIndex = _yIndex;

        m_isWall = false;
        m_isDetected = false;
    }
    public void Clear()
    {
        m_isWall = false;
        m_isDetected = false;
    }
}
[System.Serializable]
public struct Coord2DSt
{
    public int m_x;
    public int m_y;

    public Coord2DSt(int _x, int _y)
    {
        m_x = _x;
        m_y = _y;
    }
}


public class MapModel : MonoBehaviour {

    public List<Sprite> m_tileSpriteList;

    public TileData[][] m_tileDataAry;

    public List<Room> m_roomList;
    public List<Passage> m_passageList;

    // 방 담아야함
    
    public int m_mapWidth;          // 맵 전체 가로 크기
    public int m_mapHeight;         // 맵 전체 세로 크기

    /// <summary>
    /// 맵 생성 시 사용하는 변수들
    /// </summary>
    [Range(0,100)]
    public int m_fillRate;
    public int m_numOfSmooth;
    public RandomMapGenerateData[][] m_genData;
    public int m_deletRoomSize;
    public Coord2DSt[] m_tileDirOffSet;
    

    public void Init()
    {

        LoadSprite();
        InitVariables();


        RegenMap();
    }

    public int GetValidRandomX()
    {
        return UnityEngine.Random.Range(0, m_mapWidth );
    }
    public int GetValidRandomY()
    {
        return UnityEngine.Random.Range(0, m_mapHeight );
    }
    public Sprite GetTileSprite(TileSpriteType _type)
    {
        return m_tileSpriteList[(int)_type];
    }

    public void RegenMap()
    {
        Clear();

        InitTileData();                     // Cellular Automata 를 이용해서 만들어진 RandomMapGeneratorData의 맵
        SmoothingTileData();                // 만들어진 RandomMapGeneratorData를 사용가능하게 여러 번 부드럽게 만든다.
        DetectingRoomsInTileDatas();        // 만들어진 맵에서 방을 찾아낸다. 방이라고 하는 것은 다른 곳과 떨어진 독립된 덩어리
        DeleteSmallSizeRooms();             // 너무 작은 사이즈의 맵은 삭제한다.
        SetMainRoom();                      // 남은 방 중에서 메인 룸을 만든다. 이것은 현재 가장 큰 사이즈를 가진 방이 된다.
        FindEdgeTileInRooms();              // 모든 방의 엣지타일을 만든다. 엣지타일의 경우 방의 벽이 될 부분.
        FindClosestRooms();                 // 가장 가까운 방을 연결시킨다.
        CheckConnectivity();                // 모든 방이 연결이 되었는지 확인한다.
        GetPassageTiles();                  // 통로가 될 데이터 타일들을 가져온다.
        MakePassage();                      // 통로를 만든다.

        // 실제 맵 생성
        InitMap();
    }


    void InitVariables()
    {
        m_mapWidth = 128;
        m_mapHeight = 72;
        // 맵 사이즈

        m_fillRate = 60;
        // 맵을 얼마나 채울 것인가?

        m_numOfSmooth = 5;
        // 맵을 몇 번이나 부드럽게 할 것인가?

        m_deletRoomSize = 50;
        // 너무 작은 방은 삭제하는데 그 사이즈는?

        m_roomList = new List<Room>();
        // 룸 리스트 초기화

        m_genData = new RandomMapGenerateData[m_mapWidth][];

        for (int i = 0; i < m_mapWidth; i++)
            m_genData[i] = new RandomMapGenerateData[m_mapHeight];

        for (int y = 0; y < m_mapHeight; y++)
            for (int x = 0; x < m_mapWidth; x++)
                m_genData[x][y] = new RandomMapGenerateData(x, y);
        // 맵 생성시 사용하는 RandomMapGenrateData 초기화

        m_tileDataAry = new TileData[m_mapWidth][];

        for (int i = 0; i < m_mapWidth; i++)
            m_tileDataAry[i] = new TileData[m_mapHeight];
        // 실제로 사용될 예정인 타일데이터 초기화

        int numOfDir = System.Enum.GetNames(typeof(TileDir)).Length;
        m_tileDirOffSet = new Coord2DSt[numOfDir];

        for (int i = 0; i < numOfDir; i++)
        {
            switch (((TileDir)i))
            {
                case TileDir.E:
                    m_tileDirOffSet[i] = new Coord2DSt(1, 0);
                    break;
                case TileDir.SE:
                    m_tileDirOffSet[i] = new Coord2DSt(1, -1);
                    break;
                case TileDir.S:
                    m_tileDirOffSet[i] = new Coord2DSt(0, -1);
                    break;
                case TileDir.SW:
                    m_tileDirOffSet[i] = new Coord2DSt(-1, -1);
                    break;
                case TileDir.W:
                    m_tileDirOffSet[i] = new Coord2DSt(-1, 0);
                    break;
                case TileDir.NW:
                    m_tileDirOffSet[i] = new Coord2DSt(-1, 1);
                    break;
                case TileDir.N:
                    m_tileDirOffSet[i] = new Coord2DSt(0, 1);
                    break;
                case TileDir.NE:
                    m_tileDirOffSet[i] = new Coord2DSt(1, 1);
                    break;
            }
        }
        // 방향에 대해서 사용할 것 만들어 놨음..

        m_passageList = new List<Passage>();
    }
    void InitTileData()
    {                    
        for (int y = 0; y < m_mapHeight; y++)
        {
            for (int x = 0; x < m_mapWidth; x++)
            {
                if (x == 0 || x == m_mapWidth - 1 || y == 0 || y == m_mapHeight- 1)
                    m_genData[x][y].m_isWall = true;
                else
                    m_genData[x][y].m_isWall = Random.Range(0, 100) < m_fillRate ? true : false;
            }
        }        
    }
    void SmoothingTileData()
    {
        bool[][] prevAry = new bool[m_mapWidth][];

        for (int i = 0; i < m_mapWidth; i++)
            prevAry[i] = new bool[m_mapHeight];

        for (int y = 0; y < m_mapHeight; y++)
            for (int x = 0; x < m_mapWidth; x++)
                prevAry[x][y] = m_genData[x][y].m_isWall;

        
        for(int i = 0; i < m_numOfSmooth ;i++)
        {
            for (int y = 0; y < m_mapHeight; y++)
            {
                for (int x = 0; x < m_mapWidth; x++)
                {
                    int neigborCount = GetNeigborCount(prevAry,x, y);

                    m_genData[x][y].m_isWall = neigborCount > 4 ? true : false;
                }
            }

            // Update Prev infos
            for (int y = 0; y < m_mapHeight; y++)
                for (int x = 0; x < m_mapWidth; x++)
                    prevAry[x][y] = m_genData[x][y].m_isWall;
            
        }        
    }
    int GetNeigborCount(bool[][] _prevary,int _xIndex , int _yIndex)
    {
        int neigborCount = 0;
        
        for(int i = 0; i < m_tileDirOffSet.Length;i++)
        {
            Coord2DSt coord = m_tileDirOffSet[i];
            int x = _xIndex + coord.m_x;
            int y = _yIndex + coord.m_y;

            if (IsValidIndex(x,y))
            {
             
                if (_prevary[x][y])
                    neigborCount++;
                
            }
            else
                neigborCount++;
        }      
        
        return neigborCount;
    }
    void DetectingRoomsInTileDatas()
    {

        RandomMapGenerateData startData = GetStartData();
        // 시작하는 놈 찾아옴. 그놈을 확장시킨다.

        Queue<RandomMapGenerateData> queue = new Queue<RandomMapGenerateData>();

        while (true)
        {
            Room room = new Room(m_roomList.Count);

            room.AddGenData(startData);
            queue.Enqueue(startData);
            
            while( queue.Count != 0)
            {
                RandomMapGenerateData data = queue.Dequeue();
                // 가져와서 검사한 다음 큐에 집어넣기

                for (int  i = 0; i < m_tileDirOffSet.Length; i++)
                {
                    Coord2DSt coord = m_tileDirOffSet[i];

                    TileDir dir = (TileDir)i;

                    if (dir == TileDir.NE || dir == TileDir.NW || dir == TileDir.SE || dir == TileDir.SW)
                        continue;


                    int x = data.m_xIndex + coord.m_x;
                    int y = data.m_yIndex + coord.m_y;

                    if (!IsValidIndex(x,y))
                        continue;

                    RandomMapGenerateData genData = m_genData[x][y];

                    if (genData.m_isDetected == true)
                        continue;

                    genData.m_isDetected = true;
                   
                    if (!genData.m_isWall)
                    {
                        room.AddGenData(genData);
                        queue.Enqueue(genData);
                    }
                }
            }

            startData = GetStartData();
            m_roomList.Add(room);
            queue.Clear();

            if (startData == null)
                break;
        }


    }
    RandomMapGenerateData GetStartData()
    {
        for (int y = 0; y < m_mapHeight; y++)
        {
            for (int x = 0; x < m_mapWidth; x++)
            {
                if (!m_genData[x][y].m_isDetected)
                {
                    m_genData[x][y].m_isDetected = true;

                    if (!m_genData[x][y].m_isWall)
                        return m_genData[x][y];
                }
            }
        }

        return null;
    }
    void DeleteSmallSizeRooms()
    {
        for(int i = m_roomList.Count-1; i >=0  ;i--)
        {
            Room room = m_roomList[i];

            if (room.m_roomSize > m_deletRoomSize)
                continue;
            
            room.Clear();
            m_roomList.RemoveAt(i);
        }

        if (m_roomList.Count == 0)
            RegenMap();
    }
    void SetMainRoom()
    {
        int mainRoomID = 0;

        for(int i = 0; i < m_roomList.Count;i++)
        {
            int curMainRoomSize = m_roomList[mainRoomID].m_roomSize;
            int nextRoomSize = m_roomList[i].m_roomSize;

            if( nextRoomSize > curMainRoomSize)
                mainRoomID = i;
            
        }
        Room mainRoom = m_roomList[mainRoomID];
        
        mainRoom.m_isMainRoom = true;
        mainRoom.m_isConnectedToMainRoom = true;

        Debug.Log("MainRoom ID = " + mainRoom.m_id.ToString());

    }
    void FindEdgeTileInRooms()
    {
        for(int i = 0; i < m_roomList.Count;i++)
        {
            Room room = m_roomList[i];
            
            for(int k = 0; k < room.m_tileList.Count;k++)
            {
                RandomMapGenerateData data = room.m_tileList[k];

                for(int q = 0; q < m_tileDirOffSet.Length;q++)
                {
                    TileDir dir = (TileDir)q;

                    if (dir == TileDir.NE || dir == TileDir.NW || dir == TileDir.SE || dir == TileDir.SW)
                        continue;

                    int x = data.m_xIndex + m_tileDirOffSet[q].m_x;
                    int y = data.m_yIndex + m_tileDirOffSet[q].m_y;

                    if (!IsValidIndex(x, y))
                        continue;

                    RandomMapGenerateData expandData = m_genData[x][y];

                    if (!expandData.m_isWall)
                        continue;


                    room.AddEdgeTile(expandData);
                }
            }
        }
    }
    void FindClosestRooms()
    {
     
        if (m_roomList.Count == 1)
            return;

        
        for (int i = 0; i < m_roomList.Count; i++)
        {
            Room baseRoom = m_roomList[i];
            
            if (baseRoom.m_isConnectedToMainRoom && !baseRoom.m_isMainRoom) 
                  continue;
            
            Room ShortestRoom = GetShortestRoom(baseRoom);

            if (ShortestRoom == null)
                Debug.Log(" 발생 불가능 ");

            // 전체 알고리즘
            //
            // 가장 최단 거리의 방을 찾았는데, 혹시 그 방이 baseRoom에 연결된 친구가 더 가깝다면,
            // 그 친구와 가장 짧은 거리의 방을 연결시켜주자.
            // 그런데 이미 새로운 친구가 그 짧은 거리의 방과 연결되어 있다면 
            // 더 이상 하지 않는다.

            Room shortestLinkedRoom = GetShortestLinkedRoom(baseRoom, ShortestRoom);

            if( shortestLinkedRoom != null)
            {
                // 아니라는 것은 더 짧은 길이의 방이 존재한다.
                // 그렇다면 이 방이 이미 연결이 되었는지 확인한다.

                // 만약에 연결이 되어있으면 해당 방은 더이상하지 않는다.
                if ( shortestLinkedRoom.CheckAlreadyLinkedRoom(ShortestRoom) )
                    continue;

                // 만약에 여기라면 합쳐햐나는 baseRoom을 더 가까이 있는 ShortestRoom으로 교체한다.
                baseRoom = shortestLinkedRoom;
            }



            RandomMapGenerateData baseTile = null;
            RandomMapGenerateData shortTile = null;

            GetShortestPathPointTilesBetweenRooms(baseRoom, ShortestRoom, out baseTile, out shortTile);

            m_passageList.Add(new Passage(baseTile.m_xIndex, baseTile.m_yIndex,
                shortTile.m_xIndex, shortTile.m_yIndex));            
            // 통로를 집어넣는다.

            baseRoom.LinkRoom(ShortestRoom);                            
        }        
    }

    // 최단거리의 방을 가져오기
    Room GetShortestRoom(Room _baseRoom)
    {
        Room shortestRoom = null;
        float shortestRoomDis = float.MaxValue;
        
        
        for (int k = 0; k < m_roomList.Count; k++)
        {
            Room checkRoom = m_roomList[k];

            if (_baseRoom.m_id == checkRoom.m_id)
                continue;

            if (_baseRoom.CheckAlreadyLinkedRoom(checkRoom))
                continue;

            float shortestDisBetweenRoom = GetShortestDistanceBetweenRooms(_baseRoom, checkRoom);

            if (shortestDisBetweenRoom <= shortestRoomDis)
            {
                shortestRoom = checkRoom;
                shortestRoomDis = shortestDisBetweenRoom;
            }
        }

        return shortestRoom;
    }
   
    // 연결된 방에서 타겟까지 더 가까운 방이 있냐?
    Room GetShortestLinkedRoom(Room _base ,Room _target)
    {

        Room shortestRoom = null;


        if (_base.m_linkedRoomList.Count == 0)
            return null;

        float shortestDis = GetShortestDistanceBetweenRooms(_base, _target);

        for(int i = 0; i < _base.m_linkedRoomList.Count;i++)
        {
            Room linkedRoom = _base.m_linkedRoomList[i];

            float dis = GetShortestDistanceBetweenRooms(linkedRoom, _target);

            if( dis < shortestDis)
            {
                shortestRoom = linkedRoom;
                shortestDis = dis;
            }
        }

        return shortestRoom;
    }

    // 방 사이에 최단거리 타일들 구하기
    void GetShortestPathPointTilesBetweenRooms(Room _baseRoom, Room _targetRoom,
        out RandomMapGenerateData _baseTile, out RandomMapGenerateData _shortTile)
    {
        List<RandomMapGenerateData> baseEdgeList = _baseRoom.m_edgeTileList;
        List<RandomMapGenerateData> checkEdgeList = _targetRoom.m_edgeTileList;

        _baseTile = null;
        _shortTile = null;

        float shortestTileDis = float.MaxValue;

        for (int bel = 0; bel < baseEdgeList.Count; bel++)
        {
            RandomMapGenerateData beld = baseEdgeList[bel];

            for (int cel = 0; cel < checkEdgeList.Count; cel++)
            {
                RandomMapGenerateData celd = checkEdgeList[cel];

                float disX = beld.m_xIndex - celd.m_xIndex;
                float disY = beld.m_yIndex - celd.m_yIndex;

                float tileDis = disX * disX + disY * disY;

                if (tileDis <= shortestTileDis)
                {
                    shortestTileDis = tileDis;
                    _baseTile = beld;
                    _shortTile = celd;
                }
            }
        }        
    }

    // 방 사이 최단거리 구하기
    float GetShortestDistanceBetweenRooms(Room _baseRoom, Room _targetRoom)
    {
        List<RandomMapGenerateData> baseEdgeList = _baseRoom.m_edgeTileList;
        List<RandomMapGenerateData> checkEdgeList = _targetRoom.m_edgeTileList;
        
        float shortestTileDis = float.MaxValue;

        for (int bel = 0; bel < baseEdgeList.Count; bel++)
        {
            RandomMapGenerateData beld = baseEdgeList[bel];

            for (int cel = 0; cel < checkEdgeList.Count; cel++)
            {
                RandomMapGenerateData celd = checkEdgeList[cel];

                float disX = beld.m_xIndex - celd.m_xIndex;
                float disY = beld.m_yIndex - celd.m_yIndex;

                float tileDis = disX * disX + disY * disY;

                if (tileDis <= shortestTileDis)
                    shortestTileDis = tileDis;

            }
        }

        return shortestTileDis;
    }

    void CheckConnectivity()
    {
        bool isConnectedAll = true;

        for (int i = 0; i < m_roomList.Count; i++)
            if (!m_roomList[i].m_isConnectedToMainRoom)
                isConnectedAll = false;

        if( !isConnectedAll)
        {
            // 다 연결이 안됐으면, 연결을 시켜주자.
            Debug.Log("다 연결이 안되서 추가적으로 연결 확인시켜야한다. 그러나 다시 맵을 만드는 것으로" +
                "바꾸자");

            RegenMap();
        }
    }

    void GetPassageTiles()
    {
        for(int i = 0; i < m_passageList.Count;i++)
        {
            Passage p = m_passageList[i];

            int sX = p.m_startX;
            int sY = p.m_startY;
            int eX = p.m_endX;
            int eY = p.m_endY;

            if( sX > eX)
            {
                sX = p.m_endX;
                sY = p.m_endY;
                eX = p.m_startX;
                eY = p.m_startY;
            }

            
            if( eX == sX && eY == sY)
            {
                // 통로가 1개의 타일로 연결된 경우.

                RandomMapGenerateData data = m_genData[sX][sY];
                p.m_passageTileList.Add(data);

                continue;
            }

            if (eX == sX && eY != sY)
            {
                // 통로가 수직

                int startY = eY < sY ? eY : sY;
                int endY = eY < sY ? sY : eY;

                for(int y = startY; y <= endY; y++)
                {
                    RandomMapGenerateData data = m_genData[eX][y];
                    p.m_passageTileList.Add(data);
                }

                continue;
            }

            int pivotY = sY;

            float coe = (float)(eY - sY) / (float)(eX - sX);
            
            for ( int x = sX ; x <= eX ; x++)
            {
                float fv = coe * (float)(x - sX) + (float)(sY);
                int quotient = (int)fv;
                
                int xIndex = x;
                int yIndex = pivotY;


                RandomMapGenerateData data =  m_genData[xIndex][yIndex];
                p.m_passageTileList.Add(data);

                if( pivotY != quotient)
                {
                    int difQuotient = quotient - pivotY;

                    if( difQuotient < 0)
                    {
                        for (int offset = difQuotient; offset < 0; offset++)
                        {
                            data = m_genData[xIndex][yIndex + offset];
                            p.m_passageTileList.Add(data);
                        }
                    }
                    else
                    {
                        for (int offset = difQuotient; offset > 0; offset--)
                        {
                            data = m_genData[xIndex][yIndex + offset];
                            p.m_passageTileList.Add(data);
                        }                      
                    }
                    pivotY += difQuotient;
                }
            }
        }


    }

    void MakePassage()
    {
        for(int i = 0; i < m_passageList.Count; i++)
        {

            Passage p = m_passageList[i];

            for(int k = 0; k < p .m_passageTileList.Count ; k++)
            {

                RandomMapGenerateData data = p.m_passageTileList[k];
                data.m_isWall = false;

            }
        }
    }

    void Clear()
    {
        for (int y = 0; y < m_mapHeight; y++)
            for (int x = 0; x < m_mapWidth; x++)
                m_genData[x][y].Clear();

        for (int i = 0; i < m_roomList.Count; i++)
            m_roomList[i].Clear();

        m_roomList.Clear();
        m_passageList.Clear();
    }
    void InitMap()
    {
       
        for (int y = 0; y < m_mapHeight; y++)
        {
            for (int x = 0; x < m_mapWidth; x++)
            {
                m_tileDataAry[x][y] = new TileData(x, y);
                
                if (m_genData[x][y].m_isWall)
                    m_tileDataAry[x][y].SetAsObstacle();
            }
        }
    }

    void LoadSprite()
    {
        m_tileSpriteList = new List<Sprite>();

        int numOfSprite = System.Enum.GetNames(typeof(TileSpriteType)).Length;
        
        for(int i = 0; i < numOfSprite;i++)
        {
            Sprite sp = Resources.Load<Sprite>("PlayScene/Images/Tiles/" + ((TileSpriteType)i).ToString());
            m_tileSpriteList.Add(sp);
        }        
    }
    bool IsValidIndex(int _xIndex, int _yIndex)
    {
        if (_xIndex < 0 || _xIndex >= m_mapWidth || _yIndex < 0 || _yIndex >= m_mapHeight)
            return false;

        return true;
    }
}