using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class generate_map : MonoBehaviour {
    
    public GameObject[] buildings;
    public GameObject[] trees;
    public GameObject[] fences;
    public bool placeModels;
    public bool showBorder;
    public enum DrawMode { NoiseMap, ColorMap, Mesh};
    public DrawMode draw_mode;
    
    [Range(0,2)]
    public int levelOfDetails;
    public int octaves, seed;
    public int villageSizeCutoff = 5;
    [Range(0, 1)]
    public float persistence, cityLow, cityHigh;

    public float scale, lacunarity;
    public bool auto_update;
    public TerrainTypes[] regions;
    public Vector2 offset;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    private int mapChunkSize;

    void Start()
    {
        generateMap(false);
    }

    public static List<int[]> squares(int[,] livable, int cutoff)
    {
        int R = livable.GetLength(0);
        int C = livable.GetLength(1);
        int[,] S = new int[R, C];
        for (int i = 0; i < R; ++i)
        {
            S[i, 0] = livable[i, 0];
        }
        for (int i = 0; i < C; ++i)
        {
            S[0, i] = livable[0, i];
        }
        for (int i = 1; i < C; ++i)
        {
            for (int j = 1; j < R; ++j)
            {
                if (livable[i, j] == 1)
                    S[i, j] = Mathf.Min(S[i, j - 1], S[i - 1, j], S[i - 1, j - 1]) + 1;
                else
                    S[i, i] = 0;
            }
        }

        // data structure for storing square coordinates
        List<int[]> boxes = new List<int[]>();

        int[,] occupied = new int[100, 100];
        bool allSatisfied = true;
        // traverse S
        for (int i = 0; i < R; ++i)
        {
            for (int j = 0; j < C; ++j)
            {
                if (S[i, j] > 5)
                {
                    allSatisfied = true;
                    int[] coords = new int[4] { i, j - S[i, j] + 1, i - S[i, j] + 1, j };
                    //Debug.Log("w started at " + i.ToString() + " and ended at " + (i - S[i, j]).ToString());
                    //Debug.Log("h started at " + (j - S[i, j] + 1).ToString() + " and ended at " + (j - 1).ToString());
                    for (int w = i - S[i, j] + 1; w < i && allSatisfied; ++w)
                    {
                        for (int h = j - S[i, j] + 1; h < j && allSatisfied; ++h)
                        {
                           // Debug.Log("got into loop");
                            if (occupied[w, h] == 1)
                            {
                               // Debug.Log("Set to False");
                                allSatisfied = false;
                            }
                            else
                                occupied[w, h] = 1;
                        }
                    }
                    if (allSatisfied)
                        boxes.Add(coords);
                }
            }
        }

        return boxes;
    }

    public void generateMap(bool fill = true)
    {
        // have to force it to low value
        mapChunkSize = (fill ? 101 : 241);
        float heightCheck = regions[0].height;
        List<int> fencePositions = new List<int>();

        float[,] map = perlinMap.generateMap(mapChunkSize, mapChunkSize, seed, scale, octaves, persistence,
            lacunarity, offset);

        // store whether that area is livable
        int[,] livable = new int[mapChunkSize, mapChunkSize];

        Color[] colormap = new Color[mapChunkSize * mapChunkSize];
        for (int h = 0; h < mapChunkSize; ++h)
        {
            for (int w = 0; w < mapChunkSize; ++w)
            {
                // begin searching for livable areas
                if (map[w, h] <= cityHigh && map[w, h] >= cityLow)
                    livable[w, h] = 1;
                    
                else
                    livable[w, h] = 0;

                float currentHeight = map[w, h];

                for (int i = 0; i < regions.Length; ++i)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        Color colour = regions[i].color;

                        if (i == 1 && showBorder)
                        {
                            // check left
                            if (w > 0 && map[w - 1, h] <= heightCheck)
                            {
                                //colormap[h * mapChunkSize + w - 1] = Color.black;
                                fencePositions.Add(h * mapChunkSize + (w - 1));
                            }
                            // check top
                            if (h > 0 && map[w, h - 1] <= heightCheck)
                            {
                                //colormap[(h - 1) * mapChunkSize + w] = Color.black;
                                fencePositions.Add((h-1) * mapChunkSize + w);
                            }
                        }
                        else if (i == 0 && showBorder)
                        {
                            // check left
                            if (w > 0 && map[w - 1, h] > heightCheck)
                            {
                                fencePositions.Add(h * mapChunkSize + w);
                                //colour = Color.black;
                            }
                            // check top
                            if (h > 0 && map[w, h - 1] > heightCheck)
                            {
                                fencePositions.Add(h * mapChunkSize + w);
                                //colour = Color.black;
                            }
                        }
                        //else if (i == 3 && placeModels)
                        //{
                        //    treePositions.Add(h * mapChunkSize + w);
                        //}

                        colormap[h * mapChunkSize + w] = colour;
                        break;
                    }
                }
            }
        }

        display_map display = FindObjectOfType<display_map>();

        if (draw_mode == DrawMode.NoiseMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(map));
        else if (draw_mode == DrawMode.ColorMap)
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colormap, mapChunkSize, mapChunkSize));
        else if (draw_mode == DrawMode.Mesh)
        {
            MeshData md = MeshGenerator.generate_terrain_mesh(map, meshHeightMultiplier, levelOfDetails, meshHeightCurve);

            display.DrawMesh(md, TextureGenerator.TextureFromColorMap(colormap, mapChunkSize, mapChunkSize));

            for (int i = 0; i < fencePositions.Count; ++i)
            {
                Vector3 pos = md.vertices[fencePositions[i]];
                //pos.x *= 10;
                //pos.z *= 10;

                GameObject fence = Instantiate(fences[0], pos, Quaternion.identity);
            }

               // if (fill)
                {
                    // plant some vegetations!
                    // find the max value in map
                    int max_w = -1, max_h = -1;
                    float max = -1f;

                    for (int w = 0; w < mapChunkSize; ++w)
                    {
                        for (int h = 0; h < mapChunkSize; ++h)
                        {
                            if (map[w, h] > max)
                            {
                                max = map[w, h];
                                max_w = w;
                                max_h = h;
                            }
                            // generate some stuff on the ground
                            if (map[w, h] >= 0.3f && map[w, h] <= 0.6f)
                            {
                                float dice = Random.Range(0.0f, 1.0f);

                                if (dice < 0.08f)
                                {
                                    Vector3 treePos = new Vector3(w - 50.0f,
                                        meshHeightCurve.Evaluate(map[w, h]) * meshHeightMultiplier,
                                        (mapChunkSize - h) - 50.0f);
                                    Instantiate(trees[0], treePos, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                                }
                                else if (dice > 0.08f && dice < 0.1f)
                                {
                                Vector3 treePos = new Vector3(w - 50.0f,
                                    meshHeightCurve.Evaluate(map[w, h]) * meshHeightMultiplier,
                                    (mapChunkSize - h) - 50.0f);
                                Instantiate(trees[1], treePos, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                                }
                                else if (dice > 0.1f && dice < 0.15f)
                                {
                                Vector3 treePos = new Vector3(w - 50.0f,
                                    meshHeightCurve.Evaluate(map[w, h]) * meshHeightMultiplier,
                                    (mapChunkSize - h) - 50.0f);
                                Instantiate(trees[2], treePos, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                                }
                                else if (dice > 0.15f && dice < 0.25f)
                                {
                                Vector3 treePos = new Vector3(w - 50.0f,
                                    meshHeightCurve.Evaluate(map[w, h]) * meshHeightMultiplier,
                                    (mapChunkSize - h) - 50.0f);
                                Instantiate(trees[3], treePos, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                                }
                                else if (dice > 0.25f && dice < 0.35f)
                                {
                                Vector3 treePos = new Vector3(w - 50.0f,
                                    meshHeightCurve.Evaluate(map[w, h]) * meshHeightMultiplier,
                                    (mapChunkSize - h) - 50.0f);
                                Instantiate(trees[4], treePos, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                                }
                                else if (dice > 0.35f && dice < 0.50f)
                                {
                                Vector3 treePos = new Vector3(w - 50.0f,
                                    meshHeightCurve.Evaluate(map[w, h]) * meshHeightMultiplier,
                                    (mapChunkSize - h) - 50.0f);
                                Instantiate(trees[5], treePos, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                                }
                                else if (dice > 0.50f && dice < 0.70f)
                                {
                                Vector3 treePos = new Vector3(w - 50.0f,
                                    meshHeightCurve.Evaluate(map[w, h]) * meshHeightMultiplier,
                                    (mapChunkSize - h) - 50.0f);
                                Instantiate(trees[6], treePos, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                                }
                                else if (dice > 0.70f && dice < 0.80f)
                                {
                                Vector3 treePos = new Vector3(w - 50.0f,
                                    meshHeightCurve.Evaluate(map[w, h]) * meshHeightMultiplier,
                                    (mapChunkSize - h) - 50.0f);
                                Instantiate(trees[8], treePos, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                                }
                                else if (dice > 0.80f)
                                {
                                Vector3 treePos = new Vector3(w - 50.0f,
                                    meshHeightCurve.Evaluate(map[w, h]) * meshHeightMultiplier,
                                    (mapChunkSize - h) - 50.0f);
                                Instantiate(trees[9], treePos, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                                }


                        }
                            else if (map[w, h] >= 0.83f && map[w, h] <= 0.87f)
                            {
                                    Vector3 treePos = new Vector3(w - 50.0f,
                                        meshHeightCurve.Evaluate(map[w, h]) * meshHeightMultiplier,
                                        (mapChunkSize - h) - 50.0f);
                                    Instantiate(fences[0], treePos, Quaternion.identity);
                            }
                    }
                    }
                    // procedurally generate city
                    List<int[]> coords = squares(livable, villageSizeCutoff);

                    foreach (int[] j in coords)
                    {
                        int cityWidth = Mathf.Abs(j[3] - j[1]);
                        int cityHeight = Mathf.Abs(j[0] - j[2]);
                        int buildingFootprint = 2;

                        float[,] citygrid = new float[cityWidth, cityHeight];
                        for (int h = 0; h < cityHeight; ++h)
                        {
                            for (int w = 0; w < cityWidth; ++w)
                            {
                            //citygrid[w, h] = (int)(Mathf.PerlinNoise(w / 10.0f, h / 10.0f) * 10.0f);
                            citygrid[w, h] = Random.Range(0.0f, 1.0f);
                            }
                        }

                        // build city

                        float mapY = meshHeightCurve.Evaluate(map[j[0], j[1]]) * meshHeightMultiplier;
                        for (int h = 0; h < cityHeight; h++)
                        {
                            for (int w = 0; w < cityWidth; w++)
                            {
                                float result = citygrid[w, h];
                                Debug.Log(result);
                                Vector3 position = new Vector3((w * buildingFootprint + j[0]) - 50.0f, mapY, (mapChunkSize - (h * buildingFootprint + j[1])) - 50.0f);
                                if (w * buildingFootprint < cityWidth && h * buildingFootprint < cityHeight)
                                {
                                if (result < 0.2f)
                                    Instantiate(buildings[0], position, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                                else if (result > 0.2f && result < 0.3f)
                                        Instantiate(buildings[1], position, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                                else if (result > 0.3f && result < 0.4f)
                                    Instantiate(buildings[2], position, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                                else if (result > 0.4f && result < 0.5f)
                                    Instantiate(buildings[3], position, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                                else if (result > 0.5f && result < 0.6f)
                                    Instantiate(buildings[4], position, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                                else if (result > 0.6f && result < 0.7f)
                                    Instantiate(buildings[5], position, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                                else if (result > 0.7f && result < 0.8f)
                                    Instantiate(buildings[6], position, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                                else if (result > 0.8f && result < 0.9f)
                                    Instantiate(buildings[7], position, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                                else if (result > 0.9f)
                                    Instantiate(buildings[8], position, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                            }
                            }
                        }
                    }
            }
        }
    }

    void OnValidate()
    {
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }
}

[System.Serializable]
public struct TerrainTypes {
    public string name;
    public float height;
    public Color color;
}
