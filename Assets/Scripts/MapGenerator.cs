using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class MapGenerator : MonoBehaviour
{
    public int width;
    public int height;
    public int iterations = 3;
    public float wallSize = 1;

    [Range(0, 100)]
    public int fillPercent;

    public bool resetMap = false;

    public bool clickToReset = false;

    bool[,] map;

    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
    }

    public void SetWidthAndHeight(int w, int h)
    {
        width = w;
        height = h;
    }

    // Update is called once per frame
    void Update()
    {
        if (clickToReset && Input.GetMouseButtonDown(0)) GenerateMap();
        if (resetMap)
        {
            resetMap = false;
            GenerateMap();
        }
    }

    public void GenerateMap()
    {
        map = new bool[width, height];
        RandomFillMap();
        for (int i = 0; i < iterations; i++) CellularAutomata();
        SmootherEdges();
        FillWalls();
        //var points = CreateColliderPoints();
        //GetComponent<PolygonCollider2D>().points = points;
        var mesh = CreateMesh();
        //var mesh = CreateMeshExample();
        GetComponent<MeshFilter>().mesh = mesh;
        UpdateCollider();
        gameObject.transform.position = new Vector3((-width / 2) * wallSize, (-height / 2) * wallSize);
    }
    void RandomFillMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float random = Random.Range(0f, 1f);
                var black = random < ((float)fillPercent / 100);
                map[x, y] = black;
            }
        }
    }

    int GetNeighbors(int x, int y)
    {
        int neighbors = 0;

        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (x != i || y != j)
                {
                    if (i < width && i >= 0 && j < height && j >= 0)
                    {
                        if (map[i, j]) neighbors++;
                    }
                    else
                    {
                        neighbors++;
                    }
                }
            }
        }

        return neighbors;
    }

    void CellularAutomata()
    {
        if (map != null)
        {
            bool[,] newMap = map.Clone() as bool[,];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var neighbors = GetNeighbors(x, y);
                    newMap[x, y] = neighbors > 4;
                }
            }
            map = newMap;
        }
    }

    void FillWalls()
    {
        if (map != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1) map[x, y] = true;
                }
            }
        }
    }

    Mesh CreateMeshExample()
    {
        var mesh = new Mesh();

        // size = how many points in the polygon
        Vector3[] vertices = new Vector3[4];

        // size = always same as vertices
        Vector2[] uv = new Vector2[4];

        // size = how many connections
        int[] triangles = new int[6];

        // generate all points of triangles, EXAMPLE:
        //
        //
        // * <- point 2 [x = 0, y = 100] => new Vector3(0,100)
        // |\
        // | \
        // |  \
        // |   \
        // |    \
        // |     \
        // |      \
        // *-------*
        // ^       ^
        // |       L point 3 [x = 100, y = 0] => new Vector3(100,0)
        // |
        // L point 1 [x = 0, y = 0] => new Vector3(0,0)

        vertices[0] = new Vector3(0, 0);
        vertices[1] = new Vector3(0, wallSize);
        vertices[2] = new Vector3(wallSize, wallSize);
        vertices[3] = new Vector3(wallSize, 0);

        // uv is responsible for textures
        // it's an array of normalized vertices

        uv[0] = (Vector2)vertices[0].normalized;
        uv[1] = (Vector2)vertices[1].normalized;
        uv[2] = (Vector2)vertices[2].normalized;
        uv[3] = (Vector2)vertices[3].normalized;

        // using example above, assign indexes in the array of vertices,
        // to points in a triangle, so that
        // triangles[n] -> triangles[n + 1] -> triangles[n + 2]
        // "->" - connected to
        // connect vertices CLOCKWISE

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        return mesh;
    }

    Vector2[] CreateColliderPoints()
    {
        PolygonCollider2D collider = gameObject.AddComponent<PolygonCollider2D>();
        List<Vector2> points = new List<Vector2>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (map[x, y])
                {
                    points.Add(new Vector2(x, y));
                }
            }
        }

        return points.ToArray();
    }

    int CountBlackSquares(bool[,] map)
    {
        int blackSquares = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y]) blackSquares++;
            }
        }
        return blackSquares;
    }

    Mesh CreateMesh()
    {
        if (map is null) return new Mesh();

        var mesh = new Mesh();
        int blackSquares = CountBlackSquares(map);

        var vertices = new Vector3[blackSquares * 4];
        Vector2[] uv = new Vector2[blackSquares * 4];
        int[] triangles = new int[blackSquares * 6];

        var coords = new Vector3[blackSquares];

        int lastIndex = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y])
                {
                    coords[lastIndex] = new Vector3(x, y);
                    lastIndex++;
                }
            }
        }

        for (int i = 0; i < blackSquares; i++)
        {
            // 0 4 8 12 16 20 ...
            vertices[(i * 4)] = new Vector3(coords[i].x - wallSize / 2, coords[i].y - wallSize / 2);
            // 1 5 9 13 17 21 ...
            vertices[(i * 4) + 1] = new Vector3(coords[i].x - wallSize / 2, coords[i].y + wallSize / 2);
            // 2 6 10 14 18 22
            vertices[(i * 4) + 2] = new Vector3(coords[i].x + wallSize / 2, coords[i].y + wallSize / 2);
            // 3 7 11 15 19 23
            vertices[(i * 4) + 3] = new Vector3(coords[i].x + wallSize / 2, coords[i].y - wallSize / 2);

            uv[(i * 4)] = (Vector2)vertices[(i * 4)].normalized;
            uv[(i * 4) + 1] = (Vector2)vertices[(i * 4) + 1].normalized;
            uv[(i * 4) + 2] = (Vector2)vertices[(i * 4) + 2].normalized;
            uv[(i * 4) + 3] = (Vector2)vertices[(i * 4) + 3].normalized;

            //voodo magic
            triangles[(i * 6)] = (i * 4);
            triangles[(i * 6) + 1] = (i * 4) + 1;
            triangles[(i * 6) + 2] = (i * 4) + 2;

            triangles[(i * 6) + 3] = (i * 4);
            triangles[(i * 6) + 4] = (i * 4) + 2;
            triangles[(i * 6) + 5] = (i * 4) + 3;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        return mesh;
    }

    void UpdateCollider()
    {
        if (map is null) return;
        var col = GetComponent<PolygonCollider2D>();

        bool[,] mapClone = map.Clone() as bool[,];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (GetNeighbors(x, y) == 8)
                {
                    mapClone[x, y] = false;
                }
            }
        }
        int pathCount = CountBlackSquares(mapClone);
        var vertRecktangles = VertRectangles(ref mapClone);
        pathCount -= vertRecktangles.Count;

        col.pathCount = pathCount;

        for (int i = 0; i < vertRecktangles.Count; i++)
        {

            col.SetPath(i, vertRecktangles[i]);
        }

        int blackSquares = CountBlackSquares(mapClone);
        var coords = new Vector3[blackSquares];

        int lastIndex = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapClone[x, y])
                {
                    coords[lastIndex] = new Vector3(x, y);
                    lastIndex++;
                }
            }
        }

        for (int i = 0; i < blackSquares; i++)
        {
            var v2s = new Vector2[4] {new Vector2(coords[i].x - wallSize / 2, coords[i].y - wallSize / 2),
                new Vector2(coords[i].x - wallSize / 2, coords[i].y + wallSize / 2),
                new Vector2(coords[i].x + wallSize / 2, coords[i].y + wallSize / 2),
                new Vector2(coords[i].x + wallSize / 2, coords[i].y - wallSize / 2)
            };

            col.SetPath(vertRecktangles.Count + i, v2s);
        }

    }
    List<Vector2[]> VertRectangles(ref bool[,] map)
    {
        List<Vector2[]> vertRectangles = new List<Vector2[]>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (y + 1 < height && map[x, y] && map[x, y + 1])
                {
                    vertRectangles.Add(new Vector2[]
                    {
                        new Vector2(x + wallSize / 2, y - wallSize / 2),
                        new Vector2(x + wallSize / 2, y + 1 + wallSize / 2),
                        new Vector2(x - wallSize / 2, y + 1 + wallSize / 2),
                        new Vector2(x - wallSize / 2, y - wallSize / 2),
                    });
                    map[x, y + 1] = false;
                    map[x, y] = false;
                }
            }
        }

        return vertRectangles;
    }
    bool SquareSticksOut(int x, int y)
    {
        // all possibilities
        // XXX XOO OOO OOX
        // OXO XXO OXO OXX
        // OOO XOO XXX OOX

        if (GetNeighbors(x, y) != 3) return false;

        if (x - 1 >= 0 && y - 1 >= 0 && map[x - 1, y - 1])
        {
            if (x + 1 < width && map[x, y - 1] && map[x + 1, y - 1]) return true;
            if (y + 1 < height && map[x - 1, y] && map[x - 1, y - 1]) return true;
        }
        if (x + 1 < width && y + 1 < height && map[x + 1, y + 1])
        {
            if (x - 1 >= 0 && map[x, y + 1] && map[x + 1, y + 1]) return true;
            if (y - 1 >= 0 && map[x + 1, y] && map[x + 1, y - 1]) return true;
        }
        return false;
    }
    void SmootherEdges()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] && SquareSticksOut(x, y))
                {
                    map[x, y] = false; continue;
                }
                if (map[x, y] && GetNeighbors(x, y) == 0)
                {
                    map[x, y] = false; continue;
                }
            }
        }
    }
    // TODO: region detection, combining all regions into one big cave
}
