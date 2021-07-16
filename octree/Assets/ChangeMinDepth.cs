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
}
