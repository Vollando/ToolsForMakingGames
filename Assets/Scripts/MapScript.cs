using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapScript : MonoBehaviour {

	public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public bool autoUpdate;

    public void GenerateMap()
    {
        float[,] noiseMap = NoiseScript.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);


        MapDisplayScript display = FindObjectOfType<MapDisplayScript>();
        display.DrawNoiseMap(noiseMap);
    }

}
