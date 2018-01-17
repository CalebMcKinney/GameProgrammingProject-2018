﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour {

    [Header("Road Generation Variables")]

    [Tooltip("The number of grid squares on the map")]
    [Range(2, 100)]
    public int size;
    [Tooltip("The length of each road before an intersection")]
    public float roadDistance;

    public float intersectionSize;
    [Tooltip("How much to offset country roads")]
    public float countryOffset;
    public bool heightVariation;


    private Vector2[,] roadIntersections;
    private Dictionary<int, GameObject> coordinateDictionary = new Dictionary<int, GameObject>();

    [Header("Noise Output")]
    public Vector2 perlinNoiseSeed;
    public Vector2 heightMapSeed;

    [Header("Prefabs")]
    [Tooltip("Prefab to be used at intersections")]
    public GameObject coordinatePrefab;
    [Tooltip("Prefab to be used for each road")]
    public GameObject roadPrefab;
    public GameObject splitPrefab;

    public GameObject roadContainer;
    public GameObject splitRoadContainer;
    public GameObject horizontalContainer;
    public GameObject verticalContainer;

    List<GameObject[]> allGridSquares = new List<GameObject[]>();

    public float roadWidth = 4f;


    public void CreateCoordinates()
    {
        roadIntersections = new Vector2[Convert.ToInt32(size), Convert.ToInt32(size)];
        perlinNoiseSeed = new Vector2(UnityEngine.Random.Range(-10000f, 10000f), UnityEngine.Random.Range(-10000f, 10000f));


        for (int x = 0; x < roadIntersections.GetLength(0); x++)
        {
            for (int y = 0; y < roadIntersections.GetLength(1); y++)
            {
                roadIntersections[x, y] = new Vector2(x - size / 2, y - size / 2);
                float currentLocationValue = Mathf.PerlinNoise(x + perlinNoiseSeed.x, y + perlinNoiseSeed.y);


                if (currentLocationValue < 0.5)
                {
                    roadIntersections[x, y] = roadIntersections[x, y] + Vector2.one * UnityEngine.Random.Range(-1 * (Mathf.Lerp(1, 0, currentLocationValue * 2) * countryOffset), Mathf.Lerp(1, 0, currentLocationValue * 2) * countryOffset);
                }
            }
        }
    }

    public void CreateCoordinateGameObjects()
    {
        int i = 0;

        for (int x = 0; x < roadIntersections.GetLength(0); x++)
        {
            for (int y = 0; y < roadIntersections.GetLength(1); y++)
            {
                var createdObject = Instantiate(coordinatePrefab, new Vector3(roadIntersections[x, y].x, 0, roadIntersections[x, y].y) * roadDistance, Quaternion.identity);
                createdObject.GetComponent<Identifier>().locationX = x;
                createdObject.GetComponent<Identifier>().locationY = y;
                createdObject.GetComponent<Identifier>().location = i;

                coordinateDictionary.Add(i, createdObject);
                i++;
            }
        }
    }

    public void CreateRoadGameObject()
    {

        GameObject[] allCoordinates = GameObject.FindGameObjectsWithTag("Coordinate");

        foreach(GameObject currentObject in allCoordinates)
        {
            if (currentObject.GetComponent<Identifier>().location <= size * size)
            {
                GameObject roadHorizontal;
                GameObject roadVertical;

                if (currentObject.GetComponent<Identifier>().locationY < size - 1)
                {
                    roadHorizontal = RoadGenerator.ConnectWithMainRoad(currentObject.transform.position, coordinateDictionary[currentObject.GetComponent<Identifier>().location + 1].transform.position, roadPrefab);
                    roadHorizontal.transform.parent = horizontalContainer.transform;
                    roadHorizontal.name = "Horizontal Road";
                }

                if(currentObject.GetComponent<Identifier>().locationX < size - 1)
                {
                    roadVertical = RoadGenerator.ConnectWithMainRoad(currentObject.transform.position, coordinateDictionary[currentObject.GetComponent<Identifier>().location + (Convert.ToInt16(size))].transform.position, roadPrefab);
                    roadVertical.transform.parent = verticalContainer.transform;
                    roadVertical.name = "Vertical Road";
                }
            }

            if(currentObject.GetComponent<Identifier>().locationY < size - 1 && currentObject.GetComponent<Identifier>().locationX < size - 1)
            {
                allGridSquares.Add(new GameObject[4] {
                    coordinateDictionary[currentObject.GetComponent<Identifier>().location],                              //One at original
                    coordinateDictionary[currentObject.GetComponent<Identifier>().location + 1],                          //One to right
                    coordinateDictionary[currentObject.GetComponent<Identifier>().location + (Convert.ToInt16(size))],    //One down
                    coordinateDictionary[currentObject.GetComponent<Identifier>().location + (Convert.ToInt16(size) + 1)] //One to lower right
                });
            }
        }

        foreach(GameObject[] currentGridCoordinates in allGridSquares)
        {
            RoadGenerator.ConnectWithSideRoad(Vector3.Lerp(currentGridCoordinates[0].transform.position, currentGridCoordinates[1].transform.position, 0.5f), Vector3.Lerp(currentGridCoordinates[2].transform.position, currentGridCoordinates[3].transform.position, 0.5f), splitPrefab, splitRoadContainer, roadWidth*0.75f);
            RoadGenerator.ConnectWithSideRoad(Vector3.Lerp(currentGridCoordinates[1].transform.position, currentGridCoordinates[3].transform.position, 0.5f), Vector3.Lerp(currentGridCoordinates[0].transform.position, currentGridCoordinates[2].transform.position, 0.5f), splitPrefab, splitRoadContainer, roadWidth*0.75f);
        }
    }

    public void ClearCoordinates() { foreach (GameObject i in GameObject.FindGameObjectsWithTag("Coordinate")){ Destroy(i); } }

    public void GenerateRoads()
    {
        CreateCoordinates();
        CreateCoordinateGameObjects();
        CreateRoadGameObject();
        ClearCoordinates();
    }


    private void Start()
    {
        GenerateRoads();
    }
}
