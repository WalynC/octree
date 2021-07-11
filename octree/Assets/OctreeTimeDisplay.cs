using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OctreeTimeDisplay : MonoBehaviour
{
    public OctreeBuilder build;
    public TextMeshProUGUI text;
    void Update()
    {
        text.text = "Octree: " + build.lastOctreeTime + " ms";
    }
}
