using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public static FirstPersonCamera instance;
    public Vector3 eul;
    public Transform tgt;

    public float sens = 5f; //sensitivity
    public Vector3 offset;
    void Start()
    {
        instance = this;
        tgt = transform.parent;
        transform.parent = null;
    }

    public void Load(Vector3 e)
    {
        eul = e;
        transform.eulerAngles = eul;
    }
    public void Load(Transform e)
    {
        eul = e.eulerAngles;
        transform.eulerAngles = eul;
    }
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            float maxSDisp = Screen.width / 2f;
            Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
            if (Screen.width > Screen.height) { maxSDisp = Screen.height / 2; }
            Vector2 pos = new Vector2(screenCenter.y - Input.mousePosition.y, screenCenter.x - Input.mousePosition.x).normalized;
            float disp = Vector2.Distance(screenCenter, new Vector2(Input.mousePosition.x, Input.mousePosition.y));
            float scale = Mathf.Clamp(disp / maxSDisp, 0, 1);
            eul = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(transform.forward - transform.up * pos.x - transform.right * pos.y, transform.up), sens * scale * Time.deltaTime);
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
        }
    }
}
