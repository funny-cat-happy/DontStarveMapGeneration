using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapManager:MonoBehaviour
{
    private Transform _sphereTrans;
    private MapEntire _mapEntire;
    public MapEntire GameMap { get { return _mapEntire; } }
    private Transform _mapZone;
    [Header("Map Info")]
    public int Mapheight;
    public int Mapwidth;
    public int MapAllZoneNum;
    public float CircleRadius;
    public int MapWalkZoneNum;
    public GenerateWalkZoneType ZoneType;
    [Header("Seed")]
    public bool IsUseSeed;
    public string SeedString;
    public int CurrentSeed;
    private List<PointPosition> pointPositions= new List<PointPosition>();
    protected void Awake()
    {
        _mapZone = GameObject.Find("MapZone").transform;
        _sphereTrans = GameObject.Find("Point").transform;
        ZoneType = GenerateWalkZoneType.Depth;
        GenerateMap();
    }
    public void GenerateMap()
    {
        if (IsUseSeed)
        {
            Random.InitState(SeedString.GetHashCode());
            CurrentSeed=SeedString.GetHashCode();
        }
        else
        {
            CurrentSeed = Random.state.GetHashCode();
        }
        _mapEntire = new MapEntire(CircleRadius, MapWalkZoneNum, MapAllZoneNum, Mapwidth, Mapheight, ZoneType);
    }
    public void ReGenerateMap()
    {
        pointPositions.Clear();
        for (int i = 0; i < _sphereTrans.childCount; i++)
        {
            Destroy(_sphereTrans.GetChild(i).gameObject);
        }
        for (int i = 0; i < _mapZone.childCount; i++)
        {
            Destroy(_mapZone.GetChild(i).gameObject);
        }
        GenerateMap();
    }
}

