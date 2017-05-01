using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FortGen {

    public static void PlaceForts(GameObject[] forts, MeshData meshData, TerrainType[] regions, int regionIndex)
    {

        TerrainType placementRegion = regions[regionIndex];
        List<Vector3> spawnLocations = new List<Vector3>();

        for (int i = 0; i < meshData.vertices.Length; i++)
        {
            if (meshData.vertices[i].y <= placementRegion.height &&
                meshData.vertices[i].y > regions[regionIndex - 1].height)
            {
                spawnLocations.Add(meshData.vertices[i]);
            }
        }
        GameObject lolmemes = new GameObject();
        for (int i = 0; i < spawnLocations.Count - 1; i++)
        {
            //if (i % 10 == 0)
            //{
                Vector3 temp = spawnLocations[i];
                temp.x *= 10;
                temp.y += 50;
                temp.z *= 10;
                GameObject fort = GameObject.Instantiate(forts[0], temp, Quaternion.identity);
            fort.transform.SetParent(lolmemes.transform);
            //}
        }

    }

}
