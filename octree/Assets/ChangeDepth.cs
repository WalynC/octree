using UnityEngine;
using TMPro;

public class ChangeDepth : MonoBehaviour
{
    public TMP_InputField field;
    OctreeBuilder build;
    public bool max = false;

    private void Start()
    {
        build = OctreeBuilder.instance;
        field.text = (max ? build.displayRange.y : build.displayRange.x).ToString();
    }

    public void Change(bool increase)
    {
        if (!max) build.displayRange.x+= (increase? 1 : -1);
        else build.displayRange.y += (increase ? 1 : -1);
        field.text = (max ? build.displayRange.y : build.displayRange.x).ToString();
        build.BuildVisuals();
    }

    public void Set(string str)
    {
        if (!max) build.displayRange.x = int.Parse(str);
        else build.displayRange.y = int.Parse(str);
        build.BuildVisuals();
    }
}
