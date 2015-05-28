using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class MenuAnimation : MonoBehaviour {

    Texture2D texture;
    static int xGridSize;
    static int yGridSize;

    int[,] set;
    float width = 1, height = 1;
    float xDistFromCenter;
    float yDistFromCenter;

    public float magnification = 2f;
    public int maxIterations = 80;

    float cX, cY;
    int redInt, greenInt, blueInt;

    void Start()
    {
        //Sets default values for all player settings
        if(!PlayerPrefs.HasKey("Red"))
            PlayerPrefs.SetInt("Red",15);   
        redInt = PlayerPrefs.GetInt("Red");

        if (!PlayerPrefs.HasKey("Green") )
            PlayerPrefs.SetInt("Green", 39);        
        greenInt = PlayerPrefs.GetInt("Green");

        if (!PlayerPrefs.HasKey("Blue"))
            PlayerPrefs.SetInt("Blue", 8);       
        blueInt = PlayerPrefs.GetInt("Blue");

        if (!PlayerPrefs.HasKey("ResolutionX"))
            PlayerPrefs.SetInt("ResolutionX",1000);
        xGridSize = PlayerPrefs.GetInt("ResolutionX");

        if (!PlayerPrefs.HasKey("ResolutionY"))
            PlayerPrefs.SetInt("ResolutionY", 1000);        
        yGridSize = PlayerPrefs.GetInt("ResolutionY");


        if (!PlayerPrefs.HasKey("Speed"))
            PlayerPrefs.SetInt("Speed", 2);

        if (!PlayerPrefs.HasKey("Depth"))
            PlayerPrefs.SetInt("Depth",80);
        maxIterations = PlayerPrefs.GetInt("Depth");

        if (!PlayerPrefs.HasKey("JuliaCX"))
             PlayerPrefs.SetInt("JuliaCX", 28);
        cX = (float)PlayerPrefs.GetInt("JuliaCX") / 100;

        if (!PlayerPrefs.HasKey("JuliaCY"))
            PlayerPrefs.SetInt("JuliaCY",1);
        cY = (float)PlayerPrefs.GetInt("JuliaCY") / 100;

        set = new int[xGridSize, yGridSize];
        texture = new Texture2D(xGridSize, yGridSize);
        xDistFromCenter = xGridSize / 2;
        yDistFromCenter = yGridSize / 2;
        GetComponent<Renderer>().material.mainTexture = texture;

        fitToScreen();
        //generates background images
        generateJulia(cX, cY);
      
        GetComponent<Renderer>().material.mainTextureScale = new Vector2(1 * width, 1 / height);

    }
    void generateJulia(float cX, float cY)
    {
        float tempX;
        float compX, compY;
        int i;
        print(height);
        float xDivisor = (2 / xDistFromCenter) * magnification;
        float yDivisor = (2 / yDistFromCenter) * magnification;

        for (int x = 0; x < xGridSize; x++)
        {
            for (int y = 0; y < yGridSize; y++)
            {
                compX = (x - xDistFromCenter) * xDivisor + 1 - width;

                compY = (y - yDistFromCenter) * yDivisor + height-1 ;

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

                set[x, y] = i;

                texture.SetPixel(x, y, getColour(i));

            }
        }

        texture.Apply();
    }
    

    Color getColour(int count)
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