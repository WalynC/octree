using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleVisibility : MonoBehaviour
{
    OctreeBuilder build;
    public Toggle toggle;
    public bool cols;

    private void Start()
    {
        build = OctreeBuilder.instance;
        toggle.SetIsOnWithoutNotify(cols? build.showCols : build.showNonCols);
    }

    public void Toggle(bool toggle)
    {
        if (!cols) build.showNonCols = toggle;
        else build.showCols = toggle;
        build.BuildVisuals();
    }
}
