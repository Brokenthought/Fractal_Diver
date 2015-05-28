using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class Settings : MonoBehaviour
{

    int height;
    int width;
    Text text;
    Slider slider;
    void Start()
    {
        slider = (Slider)GameObject.Find("sliderRed").GetComponent<Slider>();
        slider.value = PlayerPrefs.GetInt("Red");
        text = (Text)GameObject.Find("txtRed").GetComponent<Text>();
        text.text = Math.Round(slider.value, 0).ToString() + '%';

        slider = (Slider)GameObject.Find("sliderGreen").GetComponent<Slider>();
        slider.value = PlayerPrefs.GetInt("Green");
        text = (Text)GameObject.Find("txtGreen").GetComponent<Text>();
        text.text = Math.Round(slider.value, 0).ToString() + '%';

        slider = (Slider)GameObject.Find("sliderBlue").GetComponent<Slider>();
        slider.value = PlayerPrefs.GetInt("Blue");
        text = (Text)GameObject.Find("txtBlue").GetComponent<Text>();
        text.text = Math.Round(slider.value, 0).ToString() + '%';

        slider = (Slider)GameObject.Find("sliderResolutionX").GetComponent<Slider>();
        slider.value = PlayerPrefs.GetInt("ResolutionX");
        text = (Text)GameObject.Find("txtResolutionX").GetComponent<Text>();
        text.text = Math.Round(slider.value, 0).ToString();

        slider = (Slider)GameObject.Find("sliderResolutionY").GetComponent<Slider>();
        slider.value = PlayerPrefs.GetInt("ResolutionY");
        text = (Text)GameObject.Find("txtResolutionY").GetComponent<Text>();
        text.text = Math.Round(slider.value, 0).ToString();

        slider = (Slider)GameObject.Find("sliderSpeed").GetComponent<Slider>();
        slider.value = PlayerPrefs.GetInt("Speed");
        text = (Text)GameObject.Find("txtSpeed").GetComponent<Text>();
        text.text = Math.Round(slider.value, 0).ToString();

        slider = (Slider)GameObject.Find("sliderDepth").GetComponent<Slider>();
        slider.value = PlayerPrefs.GetInt("Depth");
        text = (Text)GameObject.Find("txtDepth").GetComponent<Text>();
        text.text = Math.Round(slider.value, 0).ToString();
    }


    public void valueChanged(string name)
    {
        text = (Text)GameObject.Find("txt" + name).GetComponent<Text>();
        slider = (Slider)GameObject.Find("slider" + name).GetComponent<Slider>();

        if (name.Equals("Red") || name.Equals("Green") || name.Equals("Blue"))
            text.text = Math.Round(slider.value, 0).ToString() + '%';
        else
            text.text = Math.Round(slider.value, 0).ToString();

        PlayerPrefs.SetInt(name, (int)slider.value);
        SettingsAnimation.reload = true;
    }


    public void loadLevel(int level)
    {
        Application.LoadLevel(level);
    }
}
