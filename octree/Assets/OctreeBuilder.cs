using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

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
        OctreeBuilder.instance.nodeCount[depth]++;
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

    public void AddLinesToRange()
    {
        OctreeBuilder builder = OctreeBuilder.instance;
        float x = pos.x - size / 2f;
        float y = pos.y - size / 2f;
        float z = pos.z - size / 2f;
        Vector2 linePos = new Vector2(x, y);
        Vector2 range = new Vector2(z, z + size);
        OctreeBuilder.AddToRange(builder.xy, linePos, range);
        linePos.x += size;
        OctreeBuilder.AddToRange(builder.xy, linePos, range);
        linePos.y += size;
        OctreeBuilder.AddToRange(builder.xy, linePos, range);
        linePos.x -= size;
        OctreeBuilder.AddToRange(builder.xy, linePos, range);
        linePos = new Vector2(x, z);
        range = new Vector2(y, y + size);
        OctreeBuilder.AddToRange(builder.xz, linePos, range);
        linePos.x += size;
        OctreeBuilder.AddToRange(builder.xz, linePos, range);
        linePos.y += size;
        OctreeBuilder.AddToRange(builder.xz, linePos, range);
        linePos.x -= size;
        OctreeBuilder.AddToRange(builder.xz, linePos, range);
        linePos = new Vector2(y, z);
        range = new Vector2(x, x + size);
        OctreeBuilder.AddToRange(builder.yz, linePos, range);
        linePos.x += size;
        OctreeBuilder.AddToRange(builder.yz, linePos, range);
        linePos.y += size;
        OctreeBuilder.AddToRange(builder.yz, linePos, range);
        linePos.x -= size;
        OctreeBuilder.AddToRange(builder.yz, linePos, range);
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
    public Dictionary<Vector2, RangeSet> xy, xz, yz;
    public float lastOctreeTime, lastLineTime;
    public int[] nodeCount;

    public GameObject line;
    Queue<LineRenderer> pool = new Queue<LineRenderer>();
    public Queue<LineRenderer> used = new Queue<LineRenderer>();

    public static void AddToRange(Dictionary<Vector2, RangeSet> dict, Vector2 pos, Vector2 range)
    {
        RangeSet set;
        dict.TryGetValue(pos, out set);
        if (set == null)
        {
            set = new RangeSet();
            dict.Add(pos, set);
        }
        set.AddRange(range);
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

    public void BuildVisuals()
    {
        Stopwatch st = new Stopwatch();
        st.Start();
        while (used.Count > 0)
        {
            LineRenderer o = used.Dequeue();
            pool.Enqueue(o);
            o.gameObject.SetActive(false);
        }
        xy = new Dictionary<Vector2, RangeSet>();
        xz = new Dictionary<Vector2, RangeSet>();
        yz = new Dictionary<Vector2, RangeSet>();
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
        foreach (KeyValuePair<Vector2, RangeSet> kv in xy)
        {
            foreach (Vector2 v in kv.Value.ranges)
            {
                LineRenderer rend = GetLineRenderer();
                rend.SetPosition(0, new Vector3(kv.Key.x, kv.Key.y, v.x));
                rend.SetPosition(1, new Vector3(kv.Key.x, kv.Key.y, v.y));
            }
        }
        foreach (KeyValuePair<Vector2, RangeSet> kv in xz)
        {
            foreach (Vector2 v in kv.Value.ranges)
            {
                LineRenderer rend = GetLineRenderer();
                rend.SetPosition(0, new Vector3(kv.Key.x, v.x, kv.Key.y));
                rend.SetPosition(1, new Vector3(kv.Key.x, v.y, kv.Key.y));
            }
        }
        foreach (KeyValuePair<Vector2, RangeSet> kv in yz)
        {
            foreach (Vector2 v in kv.Value.ranges)
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