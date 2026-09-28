using UnityEngine;
using System.IO;

public class UwCausticFx : MonoBehaviour
{
    // Caustic effect
    // Uses pre-computed PNG images

    public float fps = 25.0f;
    public string CausticTexturesRelativePath = "/_ideoservo/Fx/Caustic/Textures/PNG/";
    public string CausticTexturesPNGFileName     = "CausticsRender_";
    public int  NbFrames = 125;
    public Texture2D[] frames;
    private int frameIndex;
    private Projector projector;

    void Start()
    {
        projector = GetComponent<Projector>();
        LoadFrames();
        InvokeRepeating("NextFrame", 0, 1 / fps);
    }

    //private bool flipflop = true;
    void NextFrame()
    {
        frameIndex = (frameIndex + 1) % frames.Length;
        projector.material.SetTexture("_ShadowTex", frames[frameIndex]);
    }

    public void LoadFrames()
    {
        if(frames.Length == 0)
        {
            frames = new Texture2D[NbFrames];
            for (int i = 0; i < NbFrames; i++)
            {
                frames[i] = LoadPNG(i + 1);
            }
        }
    }
    public Texture2D LoadPNG(int id)
    {

        Texture2D tex = null;
        byte[] fileData;

        string fullPath = Application.dataPath + CausticTexturesRelativePath + CausticTexturesPNGFileName + id.ToString("D3") + ".png";

        if (File.Exists(fullPath))
        {
            fileData = File.ReadAllBytes(fullPath);
            tex = new Texture2D(256, 256);
            bool bRet = tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            if(bRet == false)
            {
                Debug.Log("Failed to LoadImage = " + fullPath);
            }
        }
        else
        {
            Debug.Log("File " + fullPath + " does not exist");
        }
        return tex;
    }
}
