using UnityEngine;
using TMPro;

public class LineObjectCount : MonoBehaviour
{
    public TextMeshProUGUI text;
    void Update()
    {
        text.text = "Line object count: " + OctreeBuilder.instance.used.Count;
    }
}
