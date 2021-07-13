using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LineTimeDisplay : MonoBehaviour
{
    public OctreeBuilder build;
    public TextMeshProUGUI text;
    void Update()
    {
        text.text = "Line: " + build.lastLineTime + " ms";
    }
}
