using SharpVoronoiLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public struct PointPosition
{
    public float x;
    public float z;
    public int MapSegmentIndex;
    public PointPosition(float x, float z, int index)
    {
        this.x = x;
        this.z = z;
        this.MapSegmentIndex = index;
    }
}
public struct MapTile:ICloneable
{
    public float x;
    public float z;
    public int MapSegmentIndex;
    public bool IsEmpty;
    public MapTile(float x, float z,int mapSegmentIndex)
    {
        this.x = x;
        this.z = z;
        this.MapSegmentIndex = mapSegmentIndex;
        IsEmpty = true;
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
public class MapEntire
{
    public MapTile[,] MapTiles;
    private int _mapWalkZoneNum;
    public List<MapZone> MapZones=new List<MapZone>();
    public List<int> CanWalkMapZoneIndex= new List<int>();
    private List<PointPosition> _zoneGeneratePoints=new List<PointPosition>();
    private int _mapWidth;
    private int _mapHeight;
    private GenerateWalkZoneType _mapType;
    private int _pointNumber;
    private float _circleRadius;
    public MapEntire(float circleRadius,int mapWalkZoneNum, int zoneNum, int mapWidth, int mapHeight, GenerateWalkZoneType type)
    {
        _circleRadius = circleRadius;
        _mapWalkZoneNum = mapWalkZoneNum;
        _mapHeight = mapHeight;
        _mapWidth = mapWidth;
        _mapType = type;
        _pointNumber = zoneNum;
        generatePoint();
        generatePlane();
        convertToTile();
        generateZone();
    }
    private void generatePoint()
    {
        var cells = FastPoissonDiscSample();
        int tryCount = 0;
        while (cells.Count < _pointNumber)
        {
            if (tryCount >= 10)
            {
                Debug.LogError("swamp failed");
                cells = RandomSample();
                break;
            }
            tryCount++;
            cells = FastPoissonDiscSample();
        }
        //var _sphereTrans = GameObject.Find("Point").transform;
        //var _sphere = Resources.Load<GameObject>("Sphere");
        //Debug.Log("trycount is " + tryCount);
        for (int i = 0; i < _pointNumber; i++)
        {
            int index = Random.Range(0, cells.Count);
            _zoneGeneratePoints.Add(new PointPosition(cells[index].x, cells[index].z, i));
            //var sphereGameobject = UnityEngine.Object.Instantiate(_sphere, cells[index], Quaternion.identity, _sphereTrans);
            //sphereGameobject.GetComponent<MeshRenderer>().material.color = Color.red;
            //sphereGameobject.name = "zonenumber" + i;
            cells.RemoveAt(index);
        }
    }
    private List<Vector3> FastPoissonDiscSample()
    {
        float cellSize = _circleRadius / 1.414f;
        int cols = (int)Math.Ceiling(_mapWidth / cellSize);
        int rows = (int)Math.Ceiling(_mapHeight / cellSize);
        var cells = new List<Vector3>();
        var grids = new int[rows, cols];
        for (var i = 0; i < rows; ++i)
        {
            for (var j = 0; j < cols; ++j)
            {
                grids[i, j] = -1;
            }
        }
        var x0 = new Vector3(Random.Range(0, _mapWidth), 0, Random.Range(0, _mapHeight));
        var col = (int)Math.Floor(x0.x / cellSize);
        var row = (int)Math.Floor(x0.z / cellSize);
        int x0_idx = cells.Count;
        cells.Add(x0);
        grids[row, col] = x0_idx;
        var active_list = new List<int>();
        active_list.Add(x0_idx);
        while (active_list.Count > 0)
        {
            var xi_idx = active_list[Random.Range(0, active_list.Count)];
            var xi = cells[xi_idx];
            var isFind = false;
            for (int i = 0; i < 30; i++)
            {
                var dir = Random.insideUnitCircle;
                var xk = new Vector3(xi.x + (dir.normalized * _circleRadius + dir * _circleRadius).x, 0, xi.z + (dir.normalized * _circleRadius + dir * _circleRadius).y);
                if (xk.x < 0 || xk.x >= _mapWidth || xk.z < 0 || xk.z >= _mapHeight)
                {
                    continue;
                }
                col = (int)Math.Floor(xk.x / cellSize);
                row = (int)Math.Floor(xk.z / cellSize);
                if (grids[row, col] != -1)
                {
                    continue;
                }
                bool ok = true;
                var min_r = (int)Math.Floor((xk.z - _circleRadius) / cellSize);
                var max_r = (int)Math.Floor((xk.z + _circleRadius) / cellSize);
                var min_c = (int)Math.Floor((xk.x - _circleRadius) / cellSize);
                var max_c = (int)Math.Floor((xk.x + _circleRadius) / cellSize);
                for (var or = min_r; or <= max_r; ++or)
                {
                    if (ok == false)
                    {
                        break;
                    }
                    if (or < 0 || or >= rows)
                    {
                        continue;
                    }

                    for (var oc = min_c; oc <= max_c; ++oc)
                    {
                        if (oc < 0 || oc >= cols)
                        {
                            continue;
                        }

                        var xj_idx = grids[or, oc];
                        if (xj_idx != -1)
                        {
                            var xj = cells[xj_idx];
                            var dist = (xj - xk).magnitude;
                            if (dist < _circleRadius)
                            {
                                ok = false;
                                break;
                            }
                        }
                    }
                }
                if (ok)
                {
                    var xk_idx = cells.Count;
                    cells.Add(xk);

                    grids[row, col] = xk_idx;
                    active_list.Add(xk_idx);

                    isFind = true;
                    break;
                }
            }
            if (!isFind)
            {
                active_list.Remove(xi_idx);
            }
        }
        return cells;
    }
    private List<Vector3> RandomSample()
    {
        List<Vector3> cells = new List<Vector3>();
        for (int i = 0; i < _pointNumber; i++)
        {
            var xPos = Random.Range(0f, _mapWidth);
            var zPos = Random.Range(0f, _mapHeight);
            cells.Add(new Vector3(xPos, 0, zPos));
        }
        return cells;
    }
    private void generatePlane()
    {
        var mapVoronoiSite = new List<VoronoiSite>();
        for (int i = 0; i < _zoneGeneratePoints.Count; i++)
        {
            mapVoronoiSite.Add(new VoronoiSite(_zoneGeneratePoints[i].x, _zoneGeneratePoints[i].z, _zoneGeneratePoints[i].MapSegmentIndex));
        }
        VoronoiPlane.TessellateOnce(mapVoronoiSite, 0, 0, _mapWidth, _mapHeight);
        for (int i = 0; i < _zoneGeneratePoints.Count; i++)
        {
            MapZones.Add(new MapZone(this,new List<int>(), _zoneGeneratePoints[i].MapSegmentIndex, mapVoronoiSite[i], new List<MapTile>(),_mapWidth,_mapHeight));
        }
    }
    private void convertToTile()
    {
        MapTiles = new MapTile[_mapWidth, _mapHeight];
        for (int i = 0; i < _mapHeight; i++)
        {
            for (int j = 0; j < _mapWidth; j++)
            {
                var minLength = float.PositiveInfinity;
                var minPoint = -1;
                for (int s = 0; s < _zoneGeneratePoints.Count; s++)
                {
                    var currentLength = Mathf.Pow(j - _zoneGeneratePoints[s].x, 2) + Mathf.Pow(i - _zoneGeneratePoints[s].z, 2);
                    if (currentLength < minLength)
                    {
                        minLength = currentLength;
                        minPoint = s;
                    }
                }
                MapTiles[j, i] = new MapTile(j, i, _zoneGeneratePoints[minPoint].MapSegmentIndex);
                MapZones[_zoneGeneratePoints[minPoint].MapSegmentIndex].HasTiles.Add(MapTiles[j, i]);
            }
        }
    }

    private void generateZone()
    {
        List<int> traversalOrder = new List<int>();
        if (_mapType==GenerateWalkZoneType.Depth)
        {
            traversalOrder = new List<int>();
            List<int> hasVisited = new List<int>();
            Stack<int> mapStack = new Stack<int>();
            int startNode = Random.Range(0, MapZones.Count);
            mapStack.Push(startNode);
            hasVisited.Add(startNode);
            while (mapStack.Count != 0)
            {
                int mapIndex = mapStack.Pop();
                traversalOrder.Add(mapIndex);
                for (int i = 0; i < MapZones[mapIndex].aroundZone.Count; i++)
                {
                    if (!hasVisited.Exists(t => t == MapZones[mapIndex].aroundZone[i]))
                    {
                        hasVisited.Add(MapZones[mapIndex].aroundZone[i]);
                        mapStack.Push(MapZones[mapIndex].aroundZone[i]);
                    }
                }
            }
        }
        else if (_mapType == GenerateWalkZoneType.Breadth)
        {
            traversalOrder = new List<int>();
            List<int> hasVisited = new List<int>();
            Queue<int> mapQueue = new Queue<int>();
            int startNode = Random.Range(0, MapZones.Count);
            hasVisited.Add(startNode);
            mapQueue.Enqueue(startNode);
            while (mapQueue.Count!=0)
            {
                int mapIndex = mapQueue.Dequeue();
                traversalOrder.Add(mapIndex);
                for (int i = 0; i < MapZones[mapIndex].aroundZone.Count; i++)
                {
                    if (!hasVisited.Exists(t => t == MapZones[mapIndex].aroundZone[i]))
                    {
                        hasVisited.Add(MapZones[mapIndex].aroundZone[i]);
                        mapQueue.Enqueue(MapZones[mapIndex].aroundZone[i]);
                    }
                }
            }
        }
        else if (_mapType == GenerateWalkZoneType.Faraway)
        {
            traversalOrder = new List<int>();
            List<int> hasVisited = new List<int>();
            Stack<int> mapStack = new Stack<int>();
            int startNode = Random.Range(0, MapZones.Count);
            mapStack.Push(startNode);
            hasVisited.Add(startNode);
            while (mapStack.Count != 0)
            {
                int mapIndex = mapStack.Pop();
                traversalOrder.Add(mapIndex);
                var sortList = new List<MapZone>();
                for (int i = 0; i < MapZones[mapIndex].aroundZone.Count; i++)
                {
                    if (!hasVisited.Exists(t => t == MapZones[mapIndex].aroundZone[i]))
                    {
                        hasVisited.Add(MapZones[mapIndex].aroundZone[i]);
                        sortList.Add(MapZones[MapZones[mapIndex].aroundZone[i]]);
                    }
                }
                sortList.Sort((p1, p2) =>
                {
                    if (Vector3.Distance(p1.CenterPoint, MapZones[startNode].CenterPoint)  > Vector3.Distance(p2.CenterPoint, MapZones[startNode].CenterPoint))
                    {
                        return 1;
                    }
                    else
                    {
                        return -1;
                    }
                });
                for (int i = 0; i < sortList.Count; i++)
                {
                    mapStack.Push(sortList[i].ZoneIndex);
                }
            }
        }
        for (int i = 0; i < MapZones.Count; i++)
        {
            MapZones[i].GeneratePlaneMesh();
        }
    }
}
public class MapZone
{
    private VoronoiSite _zoneSite;
    private int _mapWidth;
    private int _mapHeight;
    public List<int> aroundZone;
    public int ZoneIndex;
    public Vector3 CenterPoint;
    public List<MapTile> HasTiles;
    public List<MapTile> EmptyTiles;
    private MapEntire _gameMap;
    public MapZone(MapEntire gamemap,List<int> aroundZone, int zoneIndex, VoronoiSite zoneSite, List<MapTile> hasTiles, int mapWidth, int mapHeight)
    {
        _gameMap = gamemap;
        this.aroundZone = aroundZone;
        this.ZoneIndex = zoneIndex;
        _zoneSite = zoneSite;
        this.HasTiles = hasTiles;
        CenterPoint = new Vector3((float)zoneSite.Centroid.X, 0, (float)zoneSite.Centroid.Y);
        _mapWidth = mapWidth;
        _mapHeight = mapHeight;
        foreach (var item in _zoneSite.Neighbours)
        {
            aroundZone.Add(item.MapSegmentIndex);
        }
    }
    public MapTile GetEmptyTile(bool isDelete=false)
    {
        var tileIndex = Random.Range(0,EmptyTiles.Count);
        var emptyTile = EmptyTiles[tileIndex];
        if (isDelete)
        {
            _gameMap.MapTiles[(int)EmptyTiles[tileIndex].x, (int)EmptyTiles[tileIndex].z].IsEmpty = false;
            EmptyTiles.RemoveAt(tileIndex);
        }
        return emptyTile;
    }
    public void GeneratePlaneMesh()
    {
        var mapZone = GameObject.Find("MapZone").transform;
        var ver = new List<Vector3>();
        var uv = new List<Vector2>();
        foreach (var item in _zoneSite.ClockwisePoints)
        {
            float x = (float)item.X;
            float y = (float)item.Y;
            ver.Add(new Vector3(x, 0, y));
            uv.Add(new Vector2(x / _mapWidth, y / _mapHeight));
        }
        List<int> tri = new List<int>();
        for (int j = 1; j < ver.Count - 1; j++)
        {
            tri.Add(0);
            tri.Add(j);
            tri.Add(j + 1);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = ver.ToArray();
        mesh.triangles = tri.ToArray();
        mesh.uv = uv.ToArray();
        var PlaneObject = new GameObject("ground", new Type[2] { typeof(MeshFilter), typeof(MeshRenderer) });
        MeshFilter meshfilter = PlaneObject.GetComponent<MeshFilter>();
        meshfilter.mesh = mesh;
        PlaneObject.transform.SetParent(mapZone);
        PlaneObject.GetComponent<MeshRenderer>().material.color =
            new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
    }
}
public enum GenerateWalkZoneType
{
    Breadth,
    Depth,
    Faraway
}