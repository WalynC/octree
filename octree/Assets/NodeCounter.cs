using UnityEngine;
using TMPro;

public class NodeCounter : MonoBehaviour
{
    public TextMeshProUGUI text;
    void Update()
    {
        string str = "Node count: ";
        foreach (int i in OctreeBuilder.instance.nodeCount) str += i + " : ";
        text.text = str;
    }
}
