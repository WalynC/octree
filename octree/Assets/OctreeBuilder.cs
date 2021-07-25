using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class LineSet
{
    public List<Vector2> set;

    public LineSet()
    {
        set = new List<Vector2>();
    }

    public void Add(Vector2 add)
    {
        List<Vector2> remove = new List<Vector2>();
        bool redundant = false;
        for (int i = 0; i < set.Count; ++i)
        {
            bool xOverlap = add.x >= set[i].x && add.x <= set[i].y; //added range's x value is within the current range
            bool yOverlap = add.y >= set[i].x && add.y <= set[i].y; //added range's y value is within the current range
            if (xOverlap && yOverlap) // the range being added is completely within the current range
            {
                redundant = true;
                continue; //if completely overlapped, the added range won't hit anything outside of it, so this addition is finished
            }
            bool addOverlap = add.x <= set[i].x && add.y >= set[i].y; //added range completely overlaps the current range
            if (xOverlap || yOverlap || addOverlap) //if there's overlap, update the added range's values accordingly, and set the old range to be removed
            {
                add.x = Mathf.Min(add.x, set[i].x);
                add.y = Mathf.Max(add.y, set[i].y);
                remove.Add(set[i]);
            }
        }
        if (!redundant)
        {
            foreach (Vector2 i in remove) set.Remove(i);
            set.Add(add);
        }
    }
}

public class Node
{
    static Vector3[] directions = { new Vector3(1, 1, 1), new Vector3(1, 1, -1), new Vector3(1, -1, 1), new Vector3(1, -1, -1),
                                    new Vector3(-1, 1, 1), new Vector3(-1, 1, -1), new Vector3(-1, -1, 1), new Vector3(-1, -1, -1)}; //direction child nodes should be in relative to position of parent node
    /*
     * x:left/right
     * y:up/down
     * z:forward/back
     */
    public Node[] children = new Node[0];
    public Node parent = null;
    public Vector3 pos; //world position of the cube
    public float size; //size of the cube from one face to the other
    public int depth = 0; //depth in the octree of this node
    public bool collision; //whether a collision has been found in this node or not
    public Node(float size, Vector3 pos, int maxDepth, Node parent = null, int depth = 0)
    {
        this.parent = parent;
        this.pos = pos;
        this.size = size;
        this.depth = depth;
        OctreeBuilder.instance.nodeCount[depth]++;
        collision = Physics.CheckBox(pos, new Vector3(size, size, size) / 2f); //set collision value
        if (depth < maxDepth - 1 && collision) //if not at max depth and we have a collision, child nodes are needed
        {
            children = new Node[8];
            for (int i = 0; i < 8; ++i)
            {
                children[i] = new Node(size / 2f, pos + directions[i] * size / 4f, maxDepth, this, depth + 1);
            }
        }
    }

    public void AddLinesToRange()
    {
        Vector2[] mods = { new Vector2(size, 0), new Vector2(0, size), new Vector2(-size, 0) }; //Used for changing the line positions from one end to another
        OctreeBuilder builder = OctreeBuilder.instance;
        float x = pos.x - size / 2f;
        float y = pos.y - size / 2f;
        float z = pos.z - size / 2f;
        Vector2 linePos = new Vector2(x, y); //the position on the 2d plane the line is perpendicular to 
        Vector2 range = new Vector2(z, z + size); //the length of the line
        OctreeBuilder.AddToLinePlane(builder.xy, linePos, range); //add the line to the line plane
        foreach (Vector2 m in mods) OctreeBuilder.AddToLinePlane(builder.xy, linePos += m, range); //modify the line's 2d plane position, and add it
        //repeat for xz and yz plane
        linePos = new Vector2(x, z);
        range = new Vector2(y, y + size);
        OctreeBuilder.AddToLinePlane(builder.xz, linePos, range);
        foreach (Vector2 m in mods) OctreeBuilder.AddToLinePlane(builder.xz, linePos += m, range);
        linePos = new Vector2(y, z);
        range = new Vector2(x, x + size);
        OctreeBuilder.AddToLinePlane(builder.yz, linePos, range);
        foreach (Vector2 m in mods) OctreeBuilder.AddToLinePlane(builder.yz, linePos += m, range);
    }
}

public class OctreeBuilder : MonoBehaviour
{
    public static OctreeBuilder instance;
    public float size = 1024;
    public Vector2Int displayRange;
    public int maxDepth;
    public bool showCols, showNonCols;
    Node root;
    public Dictionary<Vector2, LineSet> xy, xz, yz;
    public float lastOctreeTime, lastLineTime;
    public int[] nodeCount;

    public GameObject line;
    Queue<LineRenderer> pool = new Queue<LineRenderer>();
    public Queue<LineRenderer> used = new Queue<LineRenderer>();

    public static void AddToLinePlane(Dictionary<Vector2, LineSet> dict, Vector2 pos, Vector2 range)
    {
        LineSet set;
        dict.TryGetValue(pos, out set);
        if (set == null)
        {
            set = new LineSet();
            dict.Add(pos, set);
        }
        set.Add(range);
    }

    LineRenderer GetLineRenderer()
    {
        if (pool.Count == 0) pool.Enqueue(Instantiate(line).GetComponent<LineRenderer>());
        LineRenderer obj = pool.Dequeue();
        obj.gameObject.SetActive(true);
        used.Enqueue(obj);
        return obj;
    }

    private void Start()
    {
        instance = this;
        Generate();
    }

    public void Generate()
    {
        nodeCount = new int[maxDepth];
        LevelGenerator.instance.Generate();
        GenerateOctree();
        BuildVisuals();
    }

    void GenerateOctree()
    {
        Stopwatch st = new Stopwatch();
        st.Start();
        root = new Node(size, Vector3.zero, maxDepth);
        st.Stop();
        lastOctreeTime = st.ElapsedMilliseconds;
    }

    public void Clear()
    {
        root = null;
        nodeCount = new int[maxDepth];
        ClearVisuals();
    }

    void ClearVisuals()
    {
        while (used.Count > 0)
        {
            LineRenderer o = used.Dequeue();
            pool.Enqueue(o);
            o.gameObject.SetActive(false);
        }
        xy = new Dictionary<Vector2, LineSet>();
        xz = new Dictionary<Vector2, LineSet>();
        yz = new Dictionary<Vector2, LineSet>();
    }

    public void BuildVisuals()
    {
        Stopwatch st = new Stopwatch();
        st.Start();
        ClearVisuals();
        List<Node> toSearch = new List<Node>();
        toSearch.Add(root);
        while (toSearch.Count > 0)
        {
            List<Node> newSearch = new List<Node>();
            if (toSearch[0].depth > displayRange.y) break; //Leave loop once we're no longer going to get any more visible ndoes
            foreach (Node n in toSearch)
            {
                if (n.depth >= displayRange.x && ((showCols && n.collision) || (showNonCols && !n.collision)))
                {
                    n.AddLinesToRange();
                }
                foreach (Node c in n.children) newSearch.Add(c);
            }
            toSearch = newSearch;
        }
        foreach (KeyValuePair<Vector2, LineSet> kv in xy)
        {
            foreach (Vector2 v in kv.Value.set)
            {
                LineRenderer rend = GetLineRenderer();
                rend.SetPosition(0, new Vector3(kv.Key.x, kv.Key.y, v.x));
                rend.SetPosition(1, new Vector3(kv.Key.x, kv.Key.y, v.y));
            }
        }
        foreach (KeyValuePair<Vector2, LineSet> kv in xz)
        {
            foreach (Vector2 v in kv.Value.set)
            {
                LineRenderer rend = GetLineRenderer();
                rend.SetPosition(0, new Vector3(kv.Key.x, v.x, kv.Key.y));
                rend.SetPosition(1, new Vector3(kv.Key.x, v.y, kv.Key.y));
            }
        }
        foreach (KeyValuePair<Vector2, LineSet> kv in yz)
        {
            foreach (Vector2 v in kv.Value.set)
            {
                LineRenderer rend = GetLineRenderer();
                rend.SetPosition(0, new Vector3(v.x, kv.Key.x, kv.Key.y));
                rend.SetPosition(1, new Vector3(v.y, kv.Key.x, kv.Key.y));
            }
        }
        st.Stop();
        lastLineTime = st.ElapsedMilliseconds;
    }
}