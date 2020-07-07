using PathFind;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Coords
{
    static float r = Mathf.Sqrt(3) / 3.0f;//指六边形center到各顶点的距离

    //采用pointy top
    static float w = Mathf.Sqrt(3) * r;
    static float h = 2 * r;//指6边形 2R 为h

    public static Vector2 GetSpacing(Vector2Int point)
    {
        return new Vector2(w * point.x, -h * 0.75f * point.y);
    }

    static Vector3Int even_r_to_cube(Vector2Int point)
    {
        int x, y, z;
        x = point.x - (point.y + (point.y & 1)) / 2;
        z = point.y;
        y = -x - z;
        return new Vector3Int(x, y, z);
    }

    static Vector2Int cube_to_even_r(Vector3Int cube)
    {
        int x, y;
        x = cube.x + (cube.z + (cube.z & 1)) / 2;
        y = cube.z;
        return new Vector2Int(x, y);
    }

    public static Vector3Int odd_r_to_cube(Vector2Int point)
    {
        int x, y, z;
        x = point.x - (point.y - (point.y & 1)) / 2;
        z = point.y;
        y = -x - z;
        return new Vector3Int(x, y, z);
    }

    public static Vector2Int cube_to_odd_r(Vector3Int cube)
    {
        int x, y;
        x = cube.x + (cube.z - (cube.z & 1)) / 2;
        y = cube.z;
        return new Vector2Int(x, y);
    }


    public static Vector3Int axial_to_cube(Vector2Int point)
    {
        //本质还是使用cube 的约束规范  cube.x + cube.y + cube.z = 0;
        //然后point.x 为axial的x  point.y 为 axial.z  为了保存数据的方便， axial.y 采用计算求得
        return new Vector3Int(point.x, -point.x - point.y, point.y);
    }

    public static Vector2Int cube_to_axial(Vector3Int cube)
    {
        return new Vector2Int(cube.x, cube.z);
    }
}

public static class Neighbors
{
    public enum HexCellDirection : int
    {
        right = 0,
        rightup = 1,
        leftup = 2,
        left = 3,
        leftdown = 4,
        rightdown = 5,
    }
    public static Vector3Int cube_neighbor(Vector3Int currentCube, HexCellDirection direction)
    {
        List<Vector3Int> directions = new List<Vector3Int>()
            {
                new Vector3Int(1,-1,0),
                new Vector3Int(1,0,-1),
                new Vector3Int(0,1,-1),
                new Vector3Int(-1,1,0),
                new Vector3Int(-1,0,1),
                new Vector3Int(0,-1,1),
            };
        return currentCube + directions[(int)direction];

    }

    public static Vector2Int axial_neighbor(Vector2Int currentAxial, HexCellDirection direction)
    {
        List<Vector2Int> directions = new List<Vector2Int>()
            {
                new Vector2Int(1, 0),
                new Vector2Int(1, -1),
                new Vector2Int(0, -1),
                new Vector2Int(-1, 0),
                new Vector2Int(-1, 1),
                new Vector2Int(0, 1)
            };
        return currentAxial + directions[(int)direction];
    }

    public static Vector2Int oddr_neighbor(Vector2Int point, HexCellDirection direction)
    {
        List<List<Vector2Int>> directions = new List<List<Vector2Int>>()
            {
                new List<Vector2Int>{
                    new Vector2Int(1,0),
                    new Vector2Int(0,-1),
                    new Vector2Int(-1,-1),
                    new Vector2Int(-1,0),
                    new Vector2Int(-1,1),
                    new Vector2Int(0,1)
                },
                new List<Vector2Int>{
                    new Vector2Int(+1,  0),
                    new Vector2Int(+1, -1),
                    new Vector2Int( 0, -1),
                    new Vector2Int(-1,  0),
                    new Vector2Int(0, +1),
                    new Vector2Int(+1, +1)
                },
            };
        int parity = point.y & 1;
        return directions[parity][(int)direction];
    }


    public static List<Vector3Int> GetCubeRange(Vector3Int cube, int R)
    {
        List<Vector3Int> result = new List<Vector3Int>();

        for (int x = -R; x <= R; x++)
        {
            for (int y = Mathf.Max(-R, -x - R); y <= Mathf.Min(R, -x + R); y++)
            {
                int z = -x - y;
                result.Add(cube + new Vector3Int(x, y, z));
            }
        }
        return result;
    }

}

public class CustomHex : MonoBehaviour
{
    public GameObject tilePrefab;

    public Vector2Int mapSize;

    private float r = Mathf.Sqrt(3) / 3.0f;



    float w;
    float h;

    private void Awake()
    {
       
    }



    Dictionary<Vector2Int, CellView> cellViewSet = new Dictionary<Vector2Int, CellView>();
    void Start()
    {
        for (int col = 0; col < mapSize.x; col += 1)
        {
            for (int row = 0; row < mapSize.y; row += 1)
            {
                GameObject go = GameObject.Instantiate<GameObject>(tilePrefab);
                Vector3 pos = go.transform.position;

                Vector2Int point = new Vector2Int(col, row);
                Vector2 spacing = Coords.GetSpacing(point);
                Vector2 offsetCoord = GetOffsetCoord_odd_r(point);
                go.name = point.ToString() + ":" + Coords.odd_r_to_cube(point).ToString() + ":" + Coords.cube_to_odd_r(Coords.odd_r_to_cube(point)).ToString();
                cellViewSet.Add(point, go.AddComponent<CellView>());
                pos.x = spacing.x + offsetCoord.x;
                pos.z = spacing.y + offsetCoord.y;
                pos.y = 0;
                go.transform.position = pos;
            }
        }
    }


    public int range = 1;
    public Vector2Int centerPoint;
    List<Vector3Int> cubeSet = new List<Vector3Int>();
    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
        {
            return;
        }
        foreach (Vector3Int cube in cubeSet)
        {
            Vector2Int point = Coords.cube_to_odd_r(cube);
            if (cellViewSet.ContainsKey(point))
            {
                cellViewSet[point].SetCellViewStatus(CellViewStatus.None);
            }
        }
        cubeSet.Clear();
        cubeSet = Neighbors.GetCubeRange(Coords.odd_r_to_cube(centerPoint), range);
        foreach (Vector3Int cube in cubeSet)
        {
            Vector2Int point = Coords.cube_to_odd_r(cube);
            if (cellViewSet.ContainsKey(point))
            {
                cellViewSet[point].SetCellViewStatus(CellViewStatus.EyeSight);
            }
        }
    }

    Vector2 GetOffsetCoord_even_r(Vector2Int point)
    {
        if (point.y % 2 == 0)
        {
            return new Vector2(0.5f * w, 0);
        }
        else
            return Vector2.zero;
    }

    Vector2 GetOffsetCoord_odd_r(Vector2Int point)
    {
        if (point.y % 2 == 0)
        {
            return Vector2.zero;
        }
        return new Vector2(0.5f * w, 0);
    }









}
