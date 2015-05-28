using UnityEngine;
using System.Collections;
using System;
#if NETFX_CORE
 
#else
using System.Threading;
#endif

using System.Diagnostics;
using UnityEngine.UI;

public class FractalGenerator : MonoBehaviour
{

    #region globals

    public bool isJulia;
    bool threadFinished;
    bool finishedRendering;
    bool rendering;
    bool scaling;
    bool start;
    bool fill;
    bool threadActive;
    bool fractalFinished;


   

    int maxIterations;

    //Stores movement speed in the x and y direction
    public int yMovementSpeed = 5;
    public int xMovementSpeed = 5;

    //Used to scale the texture
    float width = 1, height = 1;

    //Stores the current coordinates of the fractal
    double xCords = 0;
    double yCords = 0;

    //Used to store the offset of the texture
    float xOffset = 0;
    float yOffset = 0;

   //Used to store how many pixels a texture has moved
    int yPixels = 0;
    int xPixels = 0;


    int xGridSize;
    int yGridSize;

    int[,] fractalSet;

    public static bool reload;
    bool finished;

    public float magnification = 1.5f;
    float oldMagnification;
    public float zoomDistance;
    public float scaleSpeed;

    double cX, cY;
    float cXFloat, cYFloat;


    public float fScale = 1;

    int greenInt, redInt, blueInt;

    Button btnStartStop;
    Texture2D texture;
   // Thread fractalThread;
    #endregion

    

    void Start()
    {
        xGridSize = PlayerPrefs.GetInt("ResolutionX");
        yGridSize = PlayerPrefs.GetInt("ResolutionY");

        maxIterations = PlayerPrefs.GetInt("Depth");

        greenInt = PlayerPrefs.GetInt("Green");
        redInt = PlayerPrefs.GetInt("Red");
        blueInt = PlayerPrefs.GetInt("Blue");

        cX = (float)PlayerPrefs.GetInt("JuliaCX")/100;
        cY = (float)PlayerPrefs.GetInt("JuliaCY")/100;


        scaleSpeed = .99f +  (10 - (float)PlayerPrefs.GetInt("Speed") )/ 1000;

       
        cXFloat = (float)cX;
        cYFloat = (float)cY;

        fractalSet = new int[xGridSize, yGridSize];
        texture = new Texture2D(xGridSize, yGridSize);

        btnStartStop = (Button)GameObject.Find("btnStartStop").GetComponent<Button>();
        
        GetComponent<Renderer>().material.mainTexture = texture;
   
        GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0f, 0f);

        oldMagnification = magnification;

        fitToScreen();
        xCords = 1 - width;
        yCords = height -1;

        generateFractal(null);
        renderFractal();
        
        scale();
        finished = true;
    }

    
    void generateFractal(object state)
    {

        if (fill)
        {
             fillIn(magnification / oldMagnification);
        }

        if (isJulia)
        {
            for (int x = 0; x < xGridSize; x++)
            {
                for (int y = 0; y < yGridSize; y++)
                {
                    if (!fill || fractalSet[x, y] == 0)
                    {
                        fractalSet[x, y] = getIterationsJulia(x, y);
                    }
                    
                }
            }
        }
        else
        {
            for (int x = 0; x < xGridSize; x++)
            {
                for (int y = 0; y < yGridSize; y++)
                {
                    if (!fill || fractalSet[x, y] == 0)
                    {
                        fractalSet[x, y] = getIterationsMandelbrot(x, y);
                    }
                   // else fractalSet[x, y] = maxIterations; //Uncomment for a visual display of what fillIn() does (looks kind of cool).
                }
            }
        }

        threadFinished = true;

    }


    //Lower precision version, used when precision isn't needed. Much faster, but does not let you zoom in nearly as much.
    void generateJuliaFloat()
    {
        float xDistFromCenter, yDistFromCenter;
        float tempX;
        float compX, compY;
        int i;

        xDistFromCenter = xGridSize / 2;
        yDistFromCenter = yGridSize / 2;
        float xDivisor = (2 / xDistFromCenter) * magnification;
        float yDivisor = (2 / yDistFromCenter) * magnification;

        for (int x = 0; x < xGridSize; x++)
        {
            for (int y = 0; y < yGridSize; y++)
            {
                compX = (x - xDistFromCenter) * xDivisor + 1 - width; ;

                compY = (y - yDistFromCenter) * yDivisor + height - 1;

                for (i = 1; i < maxIterations; i++)
                {

                    if (compX * compX + compY * compY > 4)
                    {
                        break;
                    }
                    tempX = compX;

                    compX = cXFloat + compX * compX - compY * compY;

                    compY = cYFloat + 2 * tempX * compY;
                }             
                texture.SetPixel(x, y, getColour(i));
            }
        }

        texture.Apply();
    }


    /*fillIn cuts down on redundancy somewhat. It finds areas in the fractal set that are the same i.e. a black spot in the fractal,
     * and presumes that when this area is zoomed in on it will remain the same but bigger. So it then magnifies that area and then marks it
     * so the fractal generator will just skip over that part of the set. It does however leave gaps so that new shapes can form.
     * 
     * It is most effective in homogenous areas, that never the less require large amounts of processing, like the the Mandelbrot set itself. 
     * Its least effective in the most colourfull and heterogeneous areas, but still results in a quicker total processing time.
     * 
     * */
    void fillIn(double newMagnification)
    {
        int startX;
        int lengthX;

        int startY;
        int lengthY;

        int newX;
        int newY;

        int[,] newSet = new int[xGridSize, yGridSize];

        int bottom = Convert.ToInt32((yGridSize - yGridSize * newMagnification) / 2);

        int left = Convert.ToInt32((xGridSize - xGridSize * newMagnification) / 2);


        if (xPixels > xGridSize - left || yPixels > yGridSize - bottom
            || xPixels < (xGridSize - left) * -1 || yPixels < (yGridSize - bottom) * -1)
        {
            xPixels = 0;
            yPixels = 0;
            fractalSet = newSet;
            return;
        }

        //set x offset
        if (xGridSize - left + xPixels > xGridSize)
        {
            lengthX = xGridSize;
        }
        else lengthX = xGridSize - left + xPixels;


        if (xPixels + left <= 0)
        {
            startX = 0;
        }
        else startX = xPixels + left;


        //set y offset
        if (yGridSize - bottom + yPixels > yGridSize)
        {
            lengthY = yGridSize;
        }
        else lengthY = yGridSize - bottom + yPixels;

        if (yPixels + bottom <= 0)
        {
            startY = 0;
        }
        else startY = yPixels + bottom;

        for (int x = startX + 2; x < lengthX - 2; x++)
        {
            for (int y = startY + 2; y < lengthY - 2; y++)
            {
                newX = Convert.ToInt32((x - left - xPixels) / newMagnification);

                newY = Convert.ToInt32((y - bottom - yPixels) / newMagnification);

                if (
                    fractalSet[x, y] == fractalSet[x + 1, y] &&
                    fractalSet[x, y] == fractalSet[x - 1, y] &&
                    fractalSet[x, y] == fractalSet[x, y + 1] &&
                    fractalSet[x, y] == fractalSet[x, y - 1])
                {
                    newSet[newX, newY] = fractalSet[x, y];

                    if (fractalSet[x, y] == fractalSet[x + 2, y] &&
                    fractalSet[x, y] == fractalSet[x - 2, y] &&

                    fractalSet[x, y] == fractalSet[x, y + 2] &&
                    fractalSet[x, y] == fractalSet[x, y - 2] &&

                    fractalSet[x, y] == fractalSet[x - 2, y + 2] &&
                    fractalSet[x, y] == fractalSet[x + 2, y - 2] &&

                    fractalSet[x, y] == fractalSet[x + 2, y + 2] &&
                    fractalSet[x, y] == fractalSet[x - 2, y - 2])
                    {
                        newSet[newX + 1, newY] = fractalSet[x, y];
                        newSet[newX, newY + 1] = fractalSet[x, y];
                        newSet[newX, newY - 1] = fractalSet[x, y];
                        newSet[newX - 1, newY] = fractalSet[x, y];
                    }
                }

            }

        }


        fractalSet = newSet;
        xPixels = 0;
        yPixels = 0;
    }

    //coroutine that paints the next texture
    IEnumerator coRender()
    {

        for (int x = 0; x < xGridSize; x++)
        {
            for (int y = 0; y < yGridSize; y++)
            {
                texture.SetPixel(x, y, getColour(fractalSet[x, y]));
            }
            if (x % 50 == 0) yield return null;
        }

        finishedRendering = true;
    }
    //Stops the program at whatever magnification its currently at and renders a new fractal at that location
    public void stop()
    {
        start = false;
        scaling = false;
        threadFinished = true;
        fill = false;
        magnification = oldMagnification * fScale;
        yOffset = 0;
        xOffset = 0;
        yPixels = 0;
        xPixels = 0;
        generateFractal(null);
        renderFractal();
        fScale = 1;
        btnStartStop.GetComponentInChildren<Text>().text = "Start";
        oldMagnification = magnification;
        scale();
    }

    public void startStop()
    {
        if(start)
        {
            stop();
           
        }
        else
        {
            btnStartStop.GetComponentInChildren<Text>().text = "Stop";
           start = true;
        }
     
    }


    public void reset()
    {
        start = false;
        scaling = false;
        threadFinished = true;
        fill = false;
        magnification = 1.5f;

        generateFractal(null);
        renderFractal();
        fScale = 1;
        yOffset = 0;
        xOffset = 0;
        yPixels = 0;
        xPixels = 0;
        oldMagnification = magnification;
        scale();
    }

    void Update()
    {
        //Waits on the julia animation script to change the values of Cx and Cy,
        //then generates a new fractal based on those new variables
        if (isJulia && reload && finished)
        {
            reload = false;
            finished = false;

            cX = (double)PlayerPrefs.GetInt("JuliaCX") / 100;
            cY = (double)PlayerPrefs.GetInt("JuliaCY") / 100;

            cXFloat = (float)cX;
            cYFloat = (float)cY;

            generateJuliaFloat();
            finished = true;
        }

        if (Input.GetKey("return"))
        {
            start = true;
            btnStartStop.GetComponentInChildren<Text>().text = "Stop";
        }
        if (Input.GetKey("p"))
        {
            stop();
        }
        //Starts zooming into the mandelbrot
        if (start)
        {    
            //If the program is not scaling(zooming in) start a thread to generate the next fractal image
            if (!scaling)
            {
                fill = true;
                magnification = magnification * zoomDistance;

                //Had a bit of trouble finding the dll for the windows 8 thread pool
                #if !NETFX_CORE
                  System.Threading.ThreadPool.QueueUserWorkItem(
                  new System.Threading.WaitCallback(generateFractal));
                #else
                  generateFractal(null);
                #endif
            }
            //once the thread has finished start creating the fractal texture
            //unity's api does not support threads so this is done seperatly in a coroutine
            if (threadFinished)
            {
                threadFinished = false;
                StartCoroutine(coRender());
            }

            //zoom in untill the correct magnifcation has been reached
            if (fScale > zoomDistance)
            {
                scaling = true;
                fScale = fScale * scaleSpeed;
                scale();
            }
            //when that magnification is reached and the texture has been finished apply the texture
            //then restart process
            else if (fScale < zoomDistance && finishedRendering)
            {
                texture.Apply();
                fScale = 1;
                yPixels = 0;
                xPixels = 0;
                yOffset = 0;
                xOffset = 0;
                oldMagnification = magnification;

                scale();

                finishedRendering = false;
                scaling = false;
            }

        }

        if (Input.GetKey("left"))
        {
            if (start) stop();
            moveLeft();
        }

        if (Input.GetKey("right"))
        {
            if (start) stop();
            moveRight();
        }

        if (Input.GetKey("up"))
        {
            if (start) stop();
            moveUp();
        }

        if (Input.GetKey("down"))
        {
            if (start) stop();
            moveDown();
        }
    }
    /*Moving in the x and yIs very efficient as only the parts of the fractal that are coming into view need to be generated,
     * the rest of the fractal is simply moved the right amount in the x or y direction,
     * or rather the texture offset is adjusted, 
     * then the newly created part of the fractal is patched on to the old fractal.
     * */
    # region movement

    void moveDown()
    {
        yCords -= (4 * (double)yMovementSpeed / yGridSize) * magnification;
        yPixels -= yMovementSpeed;
        yOffset = ((float)yPixels / yGridSize);
        scale();


        if (isJulia)
        {
            for (int x = 0; x < xGridSize; x++)
            {
                for (int y = 0; y < yMovementSpeed; y++)
                {
                    texture.SetPixel(x + xPixels, y + yPixels, getColour(getIterationsJulia(x, y)));
                }
            }

        }
        else
        {
            for (int x = 0; x < xGridSize; x++)
            {
                for (int y = 0; y < yMovementSpeed; y++)
                {
                    texture.SetPixel(x + xPixels, y + yPixels, getColour(getIterationsMandelbrot(x, y)));
                }
            }
        }

        texture.Apply();

    }


    void moveUp()
    {
        yCords += (4 * (double)yMovementSpeed / yGridSize) * magnification;
        yPixels += yMovementSpeed;
        yOffset = ((float)yPixels / yGridSize);
        scale();

        if (isJulia)
        {
            for (int x = 0; x < xGridSize; x++)
            {
                for (int y = yGridSize - 1; y >= yGridSize - yMovementSpeed; y--)
                {
                    texture.SetPixel(x + xPixels, y + yPixels, getColour(getIterationsJulia(x, y)));
                }
            }
        }
        else
        {
            for (int x = 0; x < xGridSize; x++)
            {
                for (int y = yGridSize - 1; y >= yGridSize - yMovementSpeed; y--)
                {
                    texture.SetPixel(x + xPixels, y + yPixels, getColour(getIterationsMandelbrot(x, y)));
                }
            }
        }
        texture.Apply();
    }


    void moveRight()
    {
        xCords += (4 * (double)xMovementSpeed / xGridSize) * magnification;
        xPixels += xMovementSpeed;
        xOffset = ((float)xPixels / xGridSize);
        scale();

        if (isJulia)
        {
            for (int x = xGridSize - 1; x >= xGridSize - xMovementSpeed; x--)
            {
                for (int y = 0; y < yGridSize; y++)
                {
                    texture.SetPixel(x + xPixels, y + yPixels, getColour(getIterationsJulia(x, y)));
                }
            }
        }
        else
        {
            for (int x = xGridSize - 1; x >= xGridSize - xMovementSpeed; x--)
            {
                for (int y = 0; y < yGridSize; y++)
                {
                    texture.SetPixel(x + xPixels, y + yPixels, getColour(getIterationsMandelbrot(x, y)));
                }
            }
        }
        texture.Apply();


    }

    void moveLeft()
    {
        xCords -= (4 * (double)xMovementSpeed / xGridSize) * magnification;
        xPixels -= xMovementSpeed;
        xOffset = ((float)xPixels / xGridSize);
        scale();
        if (isJulia)
        {
            for (int x = 0; x < xMovementSpeed; x++)
            {
                for (int y = 0; y < yGridSize; y++)
                {
                    texture.SetPixel(x + xPixels, y + yPixels, getColour(getIterationsJulia(x, y)));
                }
            }
        }
        else
        {
            for (int x = 0; x < xMovementSpeed; x++)
            {
                for (int y = 0; y < yGridSize; y++)
                {
                    texture.SetPixel(x + xPixels, y + yPixels, getColour(getIterationsMandelbrot(x, y)));
                }
            }
        }
        texture.Apply();

    }
    #endregion

    //Returns a colour 
    Color getColour(int count)
    {

        Color color;

        if (count == maxIterations)
        {
            return Color.black;
        }

        byte red = Convert.ToByte((count % redInt) * (256 / redInt));

        byte green = Convert.ToByte((count % greenInt) * (256 / greenInt));

        byte blue = Convert.ToByte((count % blueInt) * (256 / blueInt));

        color = new Color32(red, green, blue, 0);

        return color;
    }

    void renderFractal()
    {
        GetComponent<Renderer>().material.mainTextureScale = new Vector2(1, 1);
        GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0, 0);
        for (int x = 0; x < xGridSize; x++)
        {
            for (int y = 0; y < yGridSize; y++)
            {
                texture.SetPixel(x, y, getColour(fractalSet[x, y]));
            }
        }
        texture.Apply();
    }


    //Scales and adjusts the texture based on the current values of fScale, xOffset and yOffset
    void scale()
    {
        float dist = (1 - fScale) / 2;
        GetComponent<Renderer>().material.mainTextureScale = new Vector2(fScale* width, fScale/height);
        GetComponent<Renderer>().material.mainTextureOffset = new Vector2(dist + xOffset, dist + yOffset);
    }

    //Actual code that generates the fractal using the formula z = z^2 + c  
    int getIterationsMandelbrot(double x, double y)
    {
        double yDivisor, xDivisor;
        double yDistFromCenter, xDistFromCenter;
        double compX, compY, temp;

        xDistFromCenter = (xGridSize / 2) ;
        yDistFromCenter = (yGridSize / 2 );

        xDivisor = ((2  / xDistFromCenter) * magnification);
        yDivisor = (2 / yDistFromCenter) * magnification;

        x = (x - xDistFromCenter) * xDivisor + xCords;
        y = (y - yDistFromCenter) * yDivisor + yCords;

        compX = x;
        compY = y;

        for (int i = 1; i < maxIterations; i++)
        {
            if (compX * compX + compY * compY > 4)
            {
                return i; ;
            }
            temp = compX;

            compX = x + compX * compX - compY * compY;

            compY = y + 2 * temp * compY;
        }

        return maxIterations;
    }
    //Julias formula set is a variation on the mandelbrot set's where C remains contant 
    int getIterationsJulia(double x, double y)
    {
        double yDivisor, xDivisor;
        double yDistFromCenter, xDistFromCenter;
        double compX, compY, temp;

        yDistFromCenter = (yGridSize / 2);
        xDistFromCenter = (xGridSize / 2);

        yDivisor = (2 / yDistFromCenter) * magnification;
        xDivisor = (2 / xDistFromCenter) * magnification;

        compX = (x - xDistFromCenter) * xDivisor + xCords;
        compY = (y - yDistFromCenter) * yDivisor + yCords;

        for (int i = 1; i < maxIterations; i++)
        {
            if (compX * compX + compY * compY > 4)
            {
                return i; ;
            }
            temp = compX;

            compX = cX + compX * compX - compY * compY;

            compY = cY + 2 * temp * compY;
        }

        return maxIterations;
    }

    public string GetTime()
    {
        string TimeInString = "";
        int hour = DateTime.Now.Hour;
        int min = DateTime.Now.Minute;
        int sec = DateTime.Now.Second;

        TimeInString = (hour < 10) ? "0" + hour.ToString() : hour.ToString();
        TimeInString += ":" + ((min < 10) ? "0" + min.ToString() : min.ToString());
        TimeInString += ":" + ((sec < 10) ? "0" + sec.ToString() : sec.ToString());
        return TimeInString;
    }
 
    //Adjusts the camera based on the screen size
    void fitToScreen()
    {
        Camera cam = Camera.main;
        float pos = (cam.nearClipPlane + 10.0f);
        transform.position = cam.transform.position + cam.transform.forward * pos;
        transform.LookAt(cam.transform);
        transform.Rotate(90.0f, 0.0f, 0.0f);
        float h = (Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f) * pos * 2f) / 10.0f;
        transform.localScale = new Vector3(h * cam.aspect, 1.0f, h);
     
        if(h * cam.aspect>1)
            height =  h * cam.aspect;    
        else       
            width = h * cam.aspect; 
     
    }
}

