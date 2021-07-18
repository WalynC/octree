using UnityEngine;

public class Quit : MonoBehaviour
{
    public void Close()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}
