using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public static FirstPersonCamera instance;
    public Vector3 eul;
    public Transform tgt;
    public static Vector3 lookAtPoint = Vector3.zero;
    public static GameObject lookAtObject = null;
    public LayerMask lookAtMask;

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
        float maxSDisp = Screen.width / 2f;
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        if (Screen.width > Screen.height) { maxSDisp = Screen.height / 2; }
        float angle = Mathf.Atan2(screenCenter.y - Input.mousePosition.y, screenCenter.x - Input.mousePosition.x);
        Vector2 pos = new Vector2(screenCenter.y - Input.mousePosition.y, screenCenter.x - Input.mousePosition.x).normalized;
        float disp = Vector2.Distance(screenCenter, new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        float scale = Mathf.Clamp(disp / maxSDisp, 0, 1);
        eul = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(transform.forward - transform.up * pos.x - transform.right * pos.y, transform.up), sens * scale * Time.deltaTime);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, lookAtMask))
        {
            lookAtPoint = hit.point;
            lookAtObject = hit.collider.gameObject;
        }
        else
        {
            lookAtPoint = transform.position + transform.forward * 9999f;
            lookAtObject = null;
        }
    }
}
