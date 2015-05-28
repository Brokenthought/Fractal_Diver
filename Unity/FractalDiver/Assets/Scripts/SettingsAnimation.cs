using UnityEngine;
using System.Collections;
using System;

public class SettingsAnimation : MonoBehaviour
{

    public static bool reload;
    bool finished;
    Texture2D texture;
    static int xGridSize;
    static int yGridSize;
    int[,] fractalSet;
    float xDistFromCenter;
    float yDistFromCenter;

    float cX, cY;
    public float magnification;
    public int maxIterations;

    int greenInt, redInt, blueInt;
    float width = 1, height = 1;
    void Start()
    {
        xGridSize = PlayerPrefs.GetInt("ResolutionX");
        yGridSize = PlayerPrefs.GetInt("ResolutionY");

        redInt = PlayerPrefs.GetInt("Red");
        greenInt = PlayerPrefs.GetInt("Green");
        blueInt = PlayerPrefs.GetInt("Blue");

        maxIterations = PlayerPrefs.GetInt("Depth");

        cX = (float)PlayerPrefs.GetInt("JuliaCX") / 100;
        cY = (float)PlayerPrefs.GetInt("JuliaCY") / 100;
        texture = new Texture2D(xGridSize, yGridSize);
        GetComponent<Renderer>().material.mainTexture = texture;
        fitToScreen();
        generateJulia(cX, cY);
        GetComponent<Renderer>().material.mainTextureScale = new Vector2(1 * width, 1 / height);
  
        finished = true;
    }

    public Color getColour(int count)
    {
        Color color;

        if (count >= maxIterations)
        {
            return Color.black;
        }

        byte red = Convert.ToByte((count % redInt) * (256 / redInt));

        byte green = Convert.ToByte((count % greenInt) * (256 / greenInt));

        byte blue = Convert.ToByte((count % blueInt) * (256 / blueInt));

        color = new Color32(red, green, blue, 0);

        return color;
    }
    
    void generateJulia(float cX, float cY)
    {
        float tempX;
        float compX, compY;
        int i;

        fractalSet = new int[xGridSize, yGridSize];
        xDistFromCenter = xGridSize / 2;
        yDistFromCenter = yGridSize / 2;
        float xDivisor = (2 / xDistFromCenter) *magnification;
        float yDivisor = (2 / yDistFromCenter) *magnification;

        for (int x = 0; x < xGridSize; x++)
        {
            for (int y = 0; y < yGridSize; y++)
            {
                compX = (x - xDistFromCenter) * xDivisor + 1 - width;

                compY = (y - yDistFromCenter) * yDivisor + height -1 ;

                for (i = 1; i < maxIterations; i++)
                {

                    if (compX * compX + compY * compY > 4)
                    {
                        break;
                    }
                    tempX = compX;

                    compX = cX + compX * compX - compY * compY;

                    compY = cY + 2 * tempX * compY;
                }

                if(i==maxIterations)
                {
                    texture.SetPixel(x, y, Color.black);
                }
                fractalSet[x, y] = i;
                texture.SetPixel(x, y, getColour(fractalSet[x, y]));

            }
        }
        
        texture.Apply();
    }

    public IEnumerator Julia()
    {

        for (int x = 0; x < xGridSize; x++)
        {
            for (int y = 0; y < yGridSize; y++)
            {
                if (fractalSet[x, y] != maxIterations)
                    texture.SetPixel(x, y, getColour(fractalSet[x, y]));
            }
            if (x % 50 == 0) yield return null;
        }
         yield return null;
       texture.Apply();
       finished = true;
        
    }

    void Update()
    {
       
        if (reload && finished)
        {
            reload = false;
            finished = false;

            redInt = PlayerPrefs.GetInt("Red");
            greenInt =  PlayerPrefs.GetInt("Green");
            blueInt = PlayerPrefs.GetInt("Blue");
            maxIterations = PlayerPrefs.GetInt("Depth");

            if (xGridSize != PlayerPrefs.GetInt("ResolutionX") ||
                yGridSize != PlayerPrefs.GetInt("ResolutionY"))
            {
                xGridSize = PlayerPrefs.GetInt("ResolutionX");
                yGridSize = PlayerPrefs.GetInt("ResolutionY");

                texture = new Texture2D(xGridSize, yGridSize);
                GetComponent<Renderer>().material.mainTexture = texture;
               
                generateJulia(cX, cY);
            }

            
            StartCoroutine(Julia());
            
        }
        
          
    }

    void fitToScreen()
    {

        Camera cam = Camera.main;
        float pos = (cam.nearClipPlane + 10.0f);
        transform.position = cam.transform.position + cam.transform.forward * pos;
        transform.LookAt(cam.transform);
        transform.Rotate(90.0f, 0.0f, 0.0f);
        float h = (Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f) * pos * 2f) / 10.0f;
        transform.localScale = new Vector3(h * cam.aspect, 1.0f, h);
        if (h * cam.aspect > 1)
            height = h * cam.aspect;
        else
            width = h * cam.aspect; 
    }
}