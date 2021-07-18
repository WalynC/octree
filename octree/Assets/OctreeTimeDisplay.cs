using UnityEngine;
using TMPro;

public class OctreeTimeDisplay : MonoBehaviour
{
    public TextMeshProUGUI text;
    void Update()
    {
        text.text = "Octree: " + OctreeBuilder.instance.lastOctreeTime + " ms";
    }
}
