using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplayScript : MonoBehaviour {

    public Renderer textureRender;
    // Reference to the renderer of the Plane so the texture can be set

    public void DrawNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colourMap = new Color[width * height];
        // Generating an array of all the colours for all the pixels
        // by looping through all the values of the noiseMap
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
                // The colourMap is a 1D Array whilst the noiseMap is a 2D Array
            }
        }
        texture.SetPixels(colourMap);
        texture.Apply();

        // Not using texturerenderer.material since that 
        // is only instantiated at runtime
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(width, 1, height);
        // Sets the size of the plane to the same size as the Map
    }
}
