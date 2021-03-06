﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MapGenerator : MonoBehaviour {

    [Tooltip("The number of grid squares on the map")] [Range(2, 100)]
    public int size;

    public bool generateSideRoads;
    public bool centerOrigin;
    public bool clearCoordinates;

    public Dictionary<int, GameObject> coordinateDictionary = new Dictionary<int, GameObject>();
    
    private CoordinateGenerator coordGen;
    private RoadGenerator roadGen;
    private BuildingGenerator buildingGen;


    private void Start()
    {
        coordGen = gameObject.GetComponent<CoordinateGenerator>();
        roadGen = gameObject.GetComponent<RoadGenerator>();
        buildingGen = gameObject.GetComponent<BuildingGenerator>();

        GenerateRoads();
    }

    public void GenerateRoads()
    {
        coordGen.CreateCoordinates(size, centerOrigin);
        coordGen.CreateCoordinateGameObjects(ref coordinateDictionary);
        roadGen.CreateRoadGameObject(size, GameObject.FindGameObjectsWithTag("Coordinate"), ref coordinateDictionary, generateSideRoads);

        buildingGen.InitializeUniqueBuildings();
        buildingGen.GenerateBuildings();
        buildingGen.ClearDefaultBuildings();

        if (clearCoordinates) coordGen.ClearCoordinates();
    }
}
