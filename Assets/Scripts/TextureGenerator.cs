﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator {

    public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        // fixes blur issue of Colour Map
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Color[] colourMap = new Color[width * height];
        // Generating an array of all the colours for all the pixels
        // by looping through all the values of the noiseMap
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
                // The colourMap is a 1D Array whilst the noiseMap is a 2D Array
            }
        }

        return TextureFromColourMap(colourMap, width, height);
    }

}
