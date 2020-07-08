using PathFind;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class Coords
{
    public enum OffSetCoordsType
    {
        even_r,
        odd_r,
        even_q,
        odd_q,
    }

    public static OffSetCoordsType offsettype = OffSetCoordsType.odd_r;

    static float r = Mathf.Sqrt(3) / 3.0f;//指六边形center到各顶点的距离

    //采用pointy top
    static float w = Mathf.Sqrt(3) * r;
    static float h = 2 * r;//指6边形 2R 为h


    //static Vector2 manualOffsetForBetterLook = new Vector2(1.2f, 1.5f);
    static Vector2 manualOffsetForBetterLook = Vector2.one;
    public static Vector2 GetSpacing(Vector2Int point)
    {
        return new Vector2(manualOffsetForBetterLook.x * w * point.x, -h * 0.75f * point.y * manualOffsetForBetterLook.y);
    }

    public static Vector2 GetOffsetCoord_even_r(Vector2Int point)
    {
        if (point.y % 2 == 0)
        {
            return new Vector2(0.5f * w, 0);
        }
        else
            return Vector2.zero;
    }


    public static Vector3 PointToVisualPosition(Vector2Int point)
    {
        float x = 0, y = 0, z = 0;
        Vector2 spacing = GetSpacing(point);
        if (offsettype == OffSetCoordsType.odd_r)
        {
            Vector2 coord = GetOffsetCoord_odd_r(point);
            x = spacing.x + coord.x;
            z = spacing.y + coord.y;
        }
        else if (offsettype == OffSetCoordsType.even_r)
        {
            Vector2 coord = GetOffsetCoord_even_r(point);
            x = spacing.x + coord.x;
            z = spacing.y + coord.y;
        }

        return new Vector3(x, y, z);
    }

    public static Vector2Int Visualposition2Point(Vector3 visualposition)
    {
        float x = 0, y = 0;

        if (offsettype == OffSetCoordsType.odd_r)
        {
            y = visualposition.z * (-4 / (3 * h));
            x = (visualposition.x - (Mathf.RoundToInt(y) & 1) * 0.5f * w) / w;
        }
        else if(offsettype == OffSetCoordsType.even_r)
        {
            y = visualposition.z * (-4 / (3 * h));
            x = (visualposition.x - (1 - (Mathf.RoundToInt(y) & 1)) * 0.5f * w) / w;
        }

        return Vector2Int.RoundToInt(new Vector2(x, y));
    }

    public static Vector2 GetOffsetCoord_odd_r(Vector2Int point)
    {
        if (point.y % 2 == 0)
        {
            return Vector2.zero;
        }
        return new Vector2(0.5f * w, 0);
    }

    static Vector3Int even_r_to_cube(Vector2Int point)
    {
        int x, y, z;
        x = point.x - (point.y + (point.y & 1)) / 2;
        z = point.y;
        y = -x - z;
        return new Vector3Int(x, y, z);
    }
    public static Vector3Int Point_to_Cube(Vector2Int point)
    {
        if(offsettype == OffSetCoordsType.even_r)
        {
            return even_r_to_cube(point);
        }
        else if(offsettype == OffSetCoordsType.odd_r)
        {
            return odd_r_to_cube(point);
        }
        return new Vector3Int(point.x, 0, point.y);
    }

    public static Vector2Int Cube_to_Point(Vector3Int cube)
    {
        if (offsettype == OffSetCoordsType.even_r)
        {
            return cube_to_even_r(cube);
        }
        else
            return cube_to_odd_r(cube);
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

public static class Distance
{
    public static int oddr_distance(Vector2Int from, Vector2Int to)
    {
        return Cube_distance(Coords.odd_r_to_cube(from), Coords.odd_r_to_cube(to));
    }

    public static int Cube_distance(Vector3Int from, Vector3Int to)
    {
        return Mathf.Max(Mathf.Abs(from.x - to.x), Mathf.Abs(from.y - to.y), Mathf.Abs(from.z - to.z));
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
    static List<Vector3Int> cube_dir = new List<Vector3Int>()
            {
                new Vector3Int(1,-1,0),
                new Vector3Int(1,0,-1),
                new Vector3Int(0,1,-1),
                new Vector3Int(-1,1,0),
                new Vector3Int(-1,0,1),
                new Vector3Int(0,-1,1),
            };
    public static Vector3Int cube_neighbor(Vector3Int currentCube, HexCellDirection direction)
    {

        return currentCube + cube_dir[(int)direction];

    }
    static List<Vector2Int> axial_dir = new List<Vector2Int>()
            {
                new Vector2Int(1, 0),
                new Vector2Int(1, -1),
                new Vector2Int(0, -1),
                new Vector2Int(-1, 0),
                new Vector2Int(-1, 1),
                new Vector2Int(0, 1)
            };
    public static Vector2Int axial_neighbor(Vector2Int currentAxial, HexCellDirection direction)
    {

        return currentAxial + axial_dir[(int)direction];
    }
    static List<List<Vector2Int>> oddr_dir = new List<List<Vector2Int>>()
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

   
    public static Vector2Int oddr_neighbor(Vector2Int point, HexCellDirection direction)
    {
        int parity = point.y & 1;
        return oddr_dir[parity][(int)direction] + point;
    }

    public static bool IsPointANeighborB(Vector2Int point,Vector2Int nei)
    {
        return oddr_neighbor(point, HexCellDirection.right) == nei
            || oddr_neighbor(point, HexCellDirection.right + 1) == nei
            || oddr_neighbor(point, HexCellDirection.right + 2) == nei
            || oddr_neighbor(point, HexCellDirection.right + 3) == nei
            || oddr_neighbor(point, HexCellDirection.right + 4) == nei
            || oddr_neighbor(point, HexCellDirection.right + 5) == nei;
    }


    public static List<Vector3Int> GetCubeRange(Vector3Int cube, int R)
    {
        List<Vector3Int> result = new List<Vector3Int>();
#if Range1
        for (int x = -R; x <= R; x++)
        {
            for (int y = Mathf.Max(-R, -x - R); y <= Mathf.Min(R, -x + R); y++)
            {
                int z = -x - y;
                result.Add(cube + new Vector3Int(x, y, z));
            }
        }
#else
        int xmin = cube.x - R;
        int xmax = cube.x + R;
        int ymin = cube.y - R;
        int ymax = cube.y + R;
        int zmin = cube.z - R;
        int zmax = cube.z + R;
        for (int x = xmin; x <= xmax; x++)
        {
            for (int y = Mathf.Max(ymin, -x - zmax); y <= Mathf.Min(ymax, -x - zmin); y++)
            {
                int z = -x - y;
                result.Add(new Vector3Int(x, y, z));
            }
        }
#endif
        return result;
    }

}


public static class DrawCoordsLine
{
   


    private static Vector3Int cubeRound(Vector3 cube)
    {
        int rx = Mathf.RoundToInt(cube.x);
        int ry = Mathf.RoundToInt(cube.y);
        int rz = Mathf.RoundToInt(cube.z);

        float x_diff = Mathf.Abs(rx - cube.x);
        float y_diff = Mathf.Abs(ry - cube.y);
        float z_diff = Mathf.Abs(rz - cube.z);

        if (x_diff > y_diff && x_diff > z_diff)
            rx = -ry - rz;
        else if (y_diff > z_diff)
            ry = -rx - rz;
        else
            rz = -rx - ry;
        return new Vector3Int(rx, ry, rz);
    }
    private static Vector3Int cubeLerp(Vector3Int cubefrom, Vector3Int cubeto, float t)
    {
        float x = 0, y = 0, z = 0;
        x = cubefrom.x + (cubeto.x - cubefrom.x) * t;
        y = cubefrom.y + (cubeto.y - cubefrom.y) * t;
        z = cubefrom.z + (cubeto.z - cubefrom.z) * t;

        return cubeRound(new Vector3(x, y, z));

    }

    public static List<Vector3Int> GetCubeLine(Vector3Int cubefrom, Vector3Int cubeto)
    {
        List<Vector3Int> line = new List<Vector3Int>();
        int distance = Distance.Cube_distance(cubefrom, cubeto);
        for (int i = 0; i <= distance; i++)
        {
            line.Add(cubeLerp(cubefrom, cubeto, i / (distance * 1.0f)));
        }
        return line;
    }
}

public static class Intersect
{

    public static List<Vector3Int> GetCubeHexIntersect(Vector3Int cube1, int r1, Vector3Int cube2, int r2)
    {
        List<Vector3Int> result = new List<Vector3Int>();
        int xmin1 = cube1.x - r1;
        int xmax1 = cube1.x + r1;
        int ymin1 = cube1.y - r1;
        int ymax1 = cube1.y + r1;
        int zmin1 = cube1.z - r1;
        int zmax1 = cube1.z + r1;

        int xmin2 = cube2.x - r2;
        int xmax2 = cube2.x + r2;
        int ymin2 = cube2.y - r2;
        int ymax2 = cube2.y + r2;
        int zmin2 = cube2.z - r2;
        int zmax2 = cube2.z + r2;

        int xmin = Mathf.Max(xmin1, xmin2);
        int xmax = Mathf.Min(xmax1, xmax2);
        int ymin = Mathf.Max(ymin1, ymin2);
        int ymax = Mathf.Min(ymax1, ymax2);
        int zmin = Mathf.Max(zmin1, zmin2);
        int zmax = Mathf.Min(zmax1, zmax2);
        for (int x = xmin; x <= xmax; x++)
        {
            for (int y = Mathf.Max(ymin, -x - zmax); y <= Mathf.Min(ymax, -x - zmin); y++)
            {
                int z = -x - y;
                result.Add(new Vector3Int(x, y, z));
            }
        }
        return result;
    }
}
public static class HexRotate
{
    public static Vector3Int Rotate60degrees_ClockSide(Vector3Int cube)
    {
        int x, y, z;
        x = -cube.z;
        y = -cube.x;
        z = -cube.y;
        return new Vector3Int(x, y, z);
    }

}



public class CustomHex : MonoBehaviour
{
    public GameObject tilePrefab;

    public Vector2Int mapSize;

    private float r = Mathf.Sqrt(3) / 3.0f;



    float w;
    float h;


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
                Vector2 offsetCoord = Coords.GetOffsetCoord_odd_r(point);
                go.name = point.ToString() + ":" + Coords.odd_r_to_cube(point).ToString() + ":" + Coords.cube_to_axial(Coords.odd_r_to_cube(point)).ToString();
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
    public Vector2Int centerPoint2;
    public int range2 = 1;
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
        cubeSet = Intersect.GetCubeHexIntersect(Coords.odd_r_to_cube(centerPoint), range, Coords.odd_r_to_cube(centerPoint2), range2);
        //Neighbors.GetCubeRange(Coords.odd_r_to_cube(centerPoint), range);
        foreach (Vector3Int cube in cubeSet)
        {
            Vector2Int point = Coords.cube_to_odd_r(cube);
            if (cellViewSet.ContainsKey(point))
            {
                cellViewSet[point].SetCellViewStatus(CellViewStatus.EyeSight);
            }
        }


    }
}
