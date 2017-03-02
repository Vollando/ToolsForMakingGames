using UnityEngine;

public static class NoiseScript {

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        // If the same map is wanted then the same seed can be used
        Vector2[] octaveOffsets = new Vector2[octaves];
        // Each octave to be sampled from the a different location
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
            // 100000 range because Mathf.PerlinNoise will return 
            // the same value if the coordinate is too high
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }
        // Handles Division by 0 Error

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;
        // Centerizes Noise Scale

        for(int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x-halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y-halfHeight) / scale * frequency + octaveOffsets[i].y;
                    // The higher the frequency, the further apart the sample points will be
                    // which means the height values will change more rapidly

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    // * 2 - 1 allows the perlinValue to be negative so that
                    // the noiseHeight can sometimes decrease
                    noiseHeight += perlinValue * amplitude;
                    // Increases the noiseheight by the perlinValue of each octave

                    amplitude *= persistance;
                    frequency += lacunarity;

                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
                // Normalize before returning so that all values
                // are back in the range of 0 to 1
                return noiseMap;
    }

}
