using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RangeSet
{
    public List<Vector2> ranges;

    public RangeSet()
    {
        ranges = new List<Vector2>();
    }

    public void AddRange(Vector2 add)
    {
        List<Vector2> remove = new List<Vector2>();
        bool redundant = false;
        for (int i = 0; i < ranges.Count; ++i)
        {
            bool xOverlap = add.x >= ranges[i].x && add.x <= ranges[i].y;
            bool yOverlap = add.y >= ranges[i].x && add.y <= ranges[i].y;
            bool addOverlap = add.x <= ranges[i].x && add.y >= ranges[i].y;
            if (xOverlap && yOverlap)
            {
                redundant = true;
                continue;
            }
            if (xOverlap || yOverlap || addOverlap)
            {
                add.x = Mathf.Min(add.x, ranges[i].x);
                add.y = Mathf.Max(add.y, ranges[i].y);
                remove.Add(ranges[i]);
            }
        }
        if (!redundant)
        {
            foreach (Vector2 i in remove) ranges.Remove(i);
            ranges.Add(add);
        }
    }
}

public class Node
{
    static Vector3[] directions = { new Vector3(1, 1, 1), new Vector3(1, 1, -1), new Vector3(1, -1, 1), new Vector3(1, -1, -1),
                                    new Vector3(-1, 1, 1), new Vector3(-1, 1, -1), new Vector3(-1, -1, 1), new Vector3(-1, -1, -1)}; //direction child nodes should be in relative to position of parent node
    /*
     * up/down 
     * north/south
     * east/west
     */
    public Node[] children = new Node[0];
    public Node parent = null;
    public Vector3 pos; //world position of the cube
    public float size; //size of the cube from one face to the other
    public int depth = 0; //depth in the octree of this node
    public bool collision;
    public Node(float size, Vector3 pos, int maxDepth, Node parent = null)
    {
        this.parent = parent;
        this.pos = pos;
        this.size = size;
        depth = parent != null ? parent.depth + 1 : 0;
        collision = Physics.CheckBox(pos, new Vector3(size, size, size) / 2f);
        if (depth < maxDepth-1 && collision) //if we're not at max depth and we have a collision, child nodes are needed
        {
            children = new Node[8];
            for (int i = 0; i < 8; ++i)
            {
                children[i] = new Node(size / 2f, pos + directions[i] * size / 4f, maxDepth, this);
            }
        }
    }

    public void DrawWireCube()
    {
        Gizmos.DrawWireCube(pos, new Vector3(size, size, size));
        foreach (Node n in children) n.DrawWireCube();
    }
}

public class OctreeBuilder : MonoBehaviour
{
    //these two lists are used for keeping track of objects in certain depths and whether they have collisions or not
    public static List<GameObject>[] noCollisions;
    public static List<GameObject>[] collisions;
    public float size = 1024;
    public Vector2Int displayRange;
    public GameObject prefab;
    public int maxDepth;
    public bool showCollisionsOnly;
    Node root;

    Queue<GameObject> pool = new Queue<GameObject>();
    Queue<GameObject> used = new Queue<GameObject>();

    private void Start()
    {
        GenerateBoard();
    }

    void GenerateBoard()
    {
        root = new Node(size, Vector3.zero, maxDepth);
        SetupObjectLists();
        RecycleVisuals();
        BuildOctreeVisuals(root);
        UpdateVisualActiveState();
    }

    void SetupObjectLists()
    {
        noCollisions = new List<GameObject>[maxDepth];
        for (int i = 0; i < maxDepth; ++i)
        {
            noCollisions[i] = new List<GameObject>();
        }
        collisions = new List<GameObject>[maxDepth];
        for (int i = 0; i < maxDepth; ++i)
        {
            collisions[i] = new List<GameObject>();
        }
    }

    void UpdateVisualActiveState()
    {
        for (int i = 0; i < maxDepth; ++i)
        {
            if (i < displayRange.x-1 || i > displayRange.y-1)
            {
                foreach (GameObject o in collisions[i]) o.SetActive(false);
                if (!showCollisionsOnly) foreach (GameObject o in noCollisions[i]) o.SetActive(false);
            }
            else
            {
                foreach (GameObject o in collisions[i]) o.SetActive(true);
                if (!showCollisionsOnly) foreach (GameObject o in noCollisions[i]) o.SetActive(true);
            }
        }
    }

    void RecycleVisuals()
    {
        while (used.Count > 0)
        {
            GameObject obj = used.Dequeue();
            pool.Enqueue(obj);
            obj.SetActive(false);
        }
    }

    void BuildOctreeVisuals(Node n)
    {
        GameObject obj = GetGameObject();
        obj.transform.localScale = new Vector3(n.size, n.size, n.size);
        obj.transform.position = n.pos;
        if (n.collision) collisions[n.depth].Add(obj);
        else noCollisions[n.depth].Add(obj);
        foreach (Node c in n.children) BuildOctreeVisuals(c);
    }

    GameObject GetGameObject()
    {
        if (pool.Count == 0) pool.Enqueue(Instantiate(prefab));
        GameObject obj = pool.Dequeue();
        used.Enqueue(obj);
        return obj;     
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            root.DrawWireCube();
        }
    }
}
