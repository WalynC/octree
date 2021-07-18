using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LineTimeDisplay : MonoBehaviour
{
    public TextMeshProUGUI text;
    void Update()
    {
        text.text = "Line: " + OctreeBuilder.instance.lastLineTime + " ms";
    }
}
