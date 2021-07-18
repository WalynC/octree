using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LineObjectCount : MonoBehaviour
{
    public OctreeBuilder build;
    public TextMeshProUGUI text;
    void Update()
    {
        text.text = "Line object count: " + build.used.Count;
    }
}
