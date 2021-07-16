using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChangeMinDepth : MonoBehaviour
{
    public TMP_InputField field;
    public OctreeBuilder build;

    private void Start()
    {
        field.text = build.displayRange.x.ToString();
    }

    public void Change(bool increase)
    {
        build.displayRange.x+= (increase? 1 : -1);
        field.text = build.displayRange.x.ToString();
        build.BuildVisuals();
    }

    public void Set(string str)
    {
        build.displayRange.x = int.Parse(str);
        build.BuildVisuals();
    }
}
