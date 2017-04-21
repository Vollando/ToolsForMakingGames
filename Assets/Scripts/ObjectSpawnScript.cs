using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawnScript : MonoBehaviour {

    public bool spawnEnabled = false;

    public TerrainType[] regions;

    public void ObjectSpawn()
    {
        for (int i = 0; i < regions.Length; i++)
        {
            if (currentHeight <= regions[i].height)
            {

        }	
}
