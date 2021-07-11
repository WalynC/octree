using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    public TextMeshProUGUI text;
    float timer;
    public float refreshRate;
    void Update()
    {
        if (Time.unscaledTime > timer)
        {
            int fps = (int)(1f / Time.unscaledDeltaTime);
            text.text = "FPS: " + fps.ToString();
            timer = Time.unscaledTime + refreshRate;
        }
    }
}
