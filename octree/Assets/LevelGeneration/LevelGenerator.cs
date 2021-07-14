using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator instance;
    public GameObject[] objects;

    public Queue<GameObject>[] pool, used;

    public int count = 100;

    private void Start()
    {
        instance = this;
        pool = new Queue<GameObject>[objects.Length];
        used = new Queue<GameObject>[objects.Length];
        for (int i = 0; i < objects.Length; ++i)
        {
            pool[i] = new Queue<GameObject>();
            used[i] = new Queue<GameObject>();
        }
    }

    public GameObject GetGameObject(int i)
    {
        if (pool[i].Count == 0) pool[i].Enqueue(Instantiate(objects[i]));
        GameObject o = pool[i].Dequeue();
        used[i].Enqueue(o);
        return o;
    }

    public void UnloadObjects(int i)
    {
        while (used[i].Count > 0)
        {
            GameObject o = used[i].Dequeue();
            pool[i].Enqueue(o);
            o.SetActive(false);
        }
    }

    public void Generate()
    {
        for (int i = 0; i < objects.Length; ++i)
        {
            UnloadObjects(i);
        }
        for (int i = 0; i < count; ++i)
        {
            GameObject o = GetGameObject(Random.Range(0, objects.Length));
            o.transform.position = new Vector3(Random.Range(-1000, 1000), Random.Range(-1000, 1000), Random.Range(-1000, 1000));
            o.transform.rotation = Random.rotation;
            o.transform.localScale = new Vector3(Random.Range(1, 500), Random.Range(1, 500), Random.Range(1, 500));
            o.SetActive(true);
        }
    }
}
