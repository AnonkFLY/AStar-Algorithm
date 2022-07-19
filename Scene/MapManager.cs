using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    private SmalletList list = new SmalletList();
    public bool useManhattanDistance = true;
    public GameObject rectMesh;
    public float waitOnceTimer = .2f;
    public float overPathTimer = .1f;
    const int xMax = 8;
    const int yMax = 8;
    private Point start;
    private Point over;
    private Point[] direction = { new Point(0, -1), new Point(-1, 0), new Point(0, 1), new Point(1, 0) };
    /// <summary>
    /// 0 is null ,1 is start,2 is target,3 is obstacle
    /// </summary>
    /// <value></value>
    private int[,] map = new int[xMax, yMax]
    {
        {1,0,3,0,3,0,0,0},
        {0,0,0,0,3,2,0,0},
        {0,0,0,0,3,3,3,0},
        {3,0,3,0,3,0,0,0},
        {0,0,0,0,0,0,0,0},
        {0,0,3,0,0,3,3,3},
        {0,0,0,0,0,3,0,0},
        {0,0,0,0,0,0,0,0},

    };
    public RectMesh[,] mapRects = new RectMesh[xMax, yMax];
    private void Awake()
    {
        CreateMap();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartCoroutine(AStar());
        }
    }
    private IEnumerator AStar()
    {
        list.AddValue(new NodeData(start, 0, GetTargetCost(start), Direction.Up));
        NodeData current = list.GetSmalletValue();
        // print($"get start point is {current.pos}");
        while (current != null)
        {
            if (Find(Direction.Up, current) ||
            Find(Direction.Right, current) ||
            Find(Direction.Down, current) ||
            Find(Direction.Left, current)) break;
            current = list.GetSmalletValue();
            yield return new WaitForSeconds(waitOnceTimer);
        }
    }
    /// <summary>
    /// 尝试进行一个位置的搜索
    /// </summary>
    /// <param name="pos">方向</param>
    /// <param name="origin">源位置</param>
    /// <returns></returns>
    private bool Find(Direction dir, NodeData origin)
    {
        var pos = direction[(int)dir] + origin.pos;
        if (!CanMove(pos))
            return false;
        var targetCost = GetTargetCost(pos);
        var moveCost = GetMoveCost(origin);
        var isAdd = list.AddValue(new NodeData(pos, moveCost, targetCost, dir));
        if (isAdd)
        {
            mapRects[(int)pos.x, (int)pos.y].SetImageColor(Color.gray);
            mapRects[(int)pos.x, (int)pos.y].SetMoveCost((origin.moveCost + 1).ToString());
            mapRects[(int)pos.x, (int)pos.y].SetTargetCost(targetCost.ToString("f1"));
            mapRects[(int)pos.x, (int)pos.y].SetTotalCost((origin.moveCost + 1 + moveCost).ToString());
        }
        if (pos == over)
        {
            // mapRects[(int)origin.pos.x, (int)origin.pos.y].SetImageColor(Color.yellow);
            // origin.dir = pos.dir;
            list.Clear();
            StartCoroutine(GetPath());
            return true;
        }
        return false;
    }
    Stack<NodeData> queue = new Stack<NodeData>();
    private IEnumerator GetPath()
    {
        var node = list.GetNodeData(over);
        while (node != null)
        {
            queue.Push(node);
            // 非网格地图应该将节点进行链表储存
            node = list.GetNodeData(node.pos - direction[(int)node.dir]);
            if (node.pos == start)
                break;
            print($"Get pos is {node.pos}");
        }
        var pathNode = start;
        while (queue.Count > 0)
        {
            pathNode += direction[(int)queue.Pop().dir];
            print($"Get pos is {pathNode},count is {queue.Count}");
            SetImageColor(pathNode.x, pathNode.y, Color.yellow, true);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private bool CanMove(Point pos)
    {
        if (pos.x >= xMax || pos.x < 0 || pos.y >= yMax || pos.y < 0)
            return false;
        return !(map[pos.x, pos.y] == 3);
    }
    private float GetTargetCost(Point pos)
    {
        if (useManhattanDistance)
            return Mathf.Abs(over.x - pos.x) + Mathf.Abs(over.y - pos.y);
        var x = Mathf.Abs(over.x - pos.x);
        var y = Mathf.Abs(over.y - pos.y);
        return Mathf.Sqrt(x * x + y * y);
    }
    private int GetMoveCost(NodeData origin)
    {
        return origin.moveCost + 1;
    }
    private void CreateMap()
    {
        var rectSize = rectMesh.GetComponent<RectTransform>().sizeDelta;
        var xSize = rectSize.x;
        var ySize = rectSize.y;
        rectSize.x = xSize * xMax;
        rectSize.y = ySize * yMax;
        transform.GetComponent<RectTransform>().sizeDelta = rectSize;
        var offset = new Vector2(-rectSize.y / 2, rectSize.x / 2);
        Vector2 startPos = (Vector2)transform.position + offset;
        for (int i = 0; i < xMax; ++i)
        {
            for (int j = 0; j < yMax; ++j)
            {
                var pos = startPos + i * new Vector2(0, -ySize) + j * new Vector2(xSize, 0);
                var rect = Instantiate(rectMesh, pos, Quaternion.identity).GetComponent<RectMesh>();
                rect.transform.SetParent(transform);
                mapRects[i, j] = rect;
                if (map[i, j] == 1)
                {
                    SetImageColor(i, j, Color.green);
                    start = new Point(i, j);
                }
                if (map[i, j] == 2)
                {
                    SetImageColor(i, j, Color.blue, true);
                    over = new Point(i, j);
                }
                if (map[i, j] == 3)
                    SetImageColor(i, j, Color.black, true);
            }
        }
    }
    private void SetImageColor(int i, int j, Color color, bool isClearText = false)
    {
        mapRects[i, j].SetImageColor(color);
        if (!isClearText)
            return;
        mapRects[i, j].SetMoveCost("");
        mapRects[i, j].SetTargetCost("");
        mapRects[i, j].SetTotalCost("");
    }

}
/// <summary>
/// 这是一个队列，取出时将会进行从大到小排序后取出最后一个（TatolCost最小值）
/// </summary>
public class SmalletList
{
    private List<NodeData> datas = new List<NodeData>();
    private Dictionary<Point, NodeData> contains = new Dictionary<Point, NodeData>();
    /// <summary>
    /// 尝试添加一个值，不会进行重复添加
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool AddValue(NodeData data)
    {
        // Debug.Log($"try add {data.pos}");
        if (contains.ContainsKey(data.pos))
            return false;
        // Debug.Log($"try add done");
        contains.Add(data.pos, data);
        datas.Add(data);
        return true;
    }
    public NodeData GetNodeData(Point pos)
    {
        contains.TryGetValue(pos, out var result);
        return result;
    }
    public NodeData GetNodeData(int x, int y)
    {
        return GetNodeData(new Point(x, y));
    }
    public NodeData GetSmalletValue()
    {
        if (datas.Count <= 0)
            return null;
        datas.Sort();
        var result = datas[datas.Count - 1];
        //Debug.Log("Get smallet value is " + result.totalCost);
        datas.RemoveAt(datas.Count - 1);
        return result;
    }
    public void Clear()
    {
        datas.Clear();
    }
}
public class NodeData : IComparable<NodeData>
{
    public NodeData(Point pos, int moveCost, float targetCost, Direction dir)
    {
        this.pos = pos;
        this.moveCost = moveCost;
        this.targetCost = targetCost;
        this.dir = dir;
        totalCost = targetCost + moveCost;
    }
    public Direction dir { get; set; }
    public Point pos { get; private set; }
    public int moveCost { get; private set; }
    public float targetCost { get; private set; }
    public float totalCost { get; private set; }

    public int CompareTo(NodeData other)
    {
        // 以离目标的距离为权重值
        return other.targetCost.CompareTo(targetCost);
    }
}
public enum Direction
{
    Left, Up, Right, Down
}
public struct Point
{
    public int x;
    public int y;

    public Point(int x = 0, int y = 0)
    {
        this.x = x;
        this.y = y;
    }
    public static Point operator +(Point x, Point y)
    {
        x.x += y.x;
        x.y += y.y;
        return x;
    }
    public static Point operator -(Point x, Point y)
    {
        x.x -= y.x;
        x.y -= y.y;
        return x;
    }
    public static bool operator ==(Point x, Point y)
    {
        return x.Equals(y);
    }
    public static bool operator !=(Point x, Point y)
    {
        return !x.Equals(y);
    }
    public override string ToString()
    {
        return $"[{x},{y}]";
    }
}