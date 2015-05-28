using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;


public class JuliaSettings : MonoBehaviour {

    Text text;
    Slider slider;

	void Start () {

        slider = (Slider)GameObject.Find("sliderJuliaCX").GetComponent<Slider>();
        slider.value = PlayerPrefs.GetInt("JuliaCX");
        text = (Text)GameObject.Find("txtJuliaCX").GetComponent<Text>();
        text.text = ((float)slider.value / 100).ToString();

        slider = (Slider)GameObject.Find("sliderJuliaCY").GetComponent<Slider>();
        slider.value = PlayerPrefs.GetInt("JuliaCY");
        text = (Text)GameObject.Find("txtJuliaCY").GetComponent<Text>();
        text.text = ((float)slider.value / 100).ToString();
	
	}
    
    public void valueChanged(string name)
    {
        text = (Text)GameObject.Find("txt" + name).GetComponent<Text>();
        slider = (Slider)GameObject.Find("slider" + name).GetComponent<Slider>();

        text.text = ((float)slider.value / 100).ToString();
        PlayerPrefs.SetInt(name, (int)slider.value);
        FractalGenerator.reload = true;
    }

}
