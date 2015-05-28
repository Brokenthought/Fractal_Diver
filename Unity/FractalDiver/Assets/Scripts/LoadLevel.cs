using UnityEngine;
using System.Collections;

public class LoadLevel : MonoBehaviour {

    public void loadLevel(int level)
    {
        Application.LoadLevel(level);
    }

}
