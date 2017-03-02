using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplayScript : MonoBehaviour {

    public Renderer textureRender;
    // Reference to the renderer of the Plane so the texture can be set

    public void DrawTextureMap(Texture2D texture)
    {
        // Not using texturerenderer.material since that 
        // is only instantiated at runtime
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
        // Sets the size of the plane to the same size as the Map
    }
}
