using PathFind;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface GameEntityRemote
{
    void MoveAlongPath(IList<ICell> path);//基于指定的路径移动

}

public partial class GameEntity : MonoBehaviour, GameEntityRemote
{
    [SerializeField]
    private GameEntityConfig entityConfig;

    Vector2Int currentCell;

    [SerializeField]
    private MapController mapController;

    GameEntityVisual entityVisual;

    IEnumerator Start()
    {
        yield return null;
        GameEntityMgr.Init(mapController);
        entityVisual = new GameEntityVisual(this.gameObject);
        StartCoroutine(workAsUpdate());
        currentCell = mapController.GetCellView(new Vector2Int(0, 0)).GetPoint();
    }

    IEnumerator workAsUpdate()
    {
        yield return null;

        while (true)
        {
            if (currentPath == null || currentPath.Count == 0)
                yield return new WaitForSeconds(0.5f);

            if (currentPath != null && currentPath.Count > 0)
            {
                if (currentPath.Count == 1)
                {
                    yield return movefromApoint2Bpoint(currentPath[0], currentPath[0]);
                    currentPath.RemoveAt(0);
                }
                else
                {
                    ICell start = currentPath[0];
                    ICell next = currentPath[1];

                    yield return movefromApoint2Bpoint(start, next);
                    currentPath.RemoveAt(0);
                }
            }

        }
    }

    internal void GainFocus()
    {
        entityVisual.SetColor(Color.green);
        ShowEyeSight();
    }

    internal void LoseFocus()
    {
        entityVisual.SetColor(Color.white);
    }

    IEnumerator movefromApoint2Bpoint(ICell from, ICell to)
    {
        var mapSize = Map.Instance.GetMapSize();
        Vector3 fromVisualPos = HexCoords.GetHexVisualCoords(from.Point, mapSize);
        Vector3 toVisualPos = HexCoords.GetHexVisualCoords(to.Point, mapSize);
        float t = 0;
        while (t < 1)
        {
            t += 0.1f;
            this.transform.LookAt(toVisualPos);
            if (t < 0.5)
                currentCell = from.Point;
            else
                currentCell = to.Point;
            this.transform.position = Vector3.Lerp(fromVisualPos, toVisualPos, t);
            yield return null;
        }
    }


    private IList<ICell> currentPath = null;

    public void MoveAlongPath(IList<ICell> path)
    {
        if (path == null || path.Count == 0)
            return;

        currentPath = path;
    }


}

public partial class GameEntity : IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    GameEntitySelectStatus selectStatus = GameEntitySelectStatus.None;

    public GameEntitySelectStatus SelectStatus
    {
        get => selectStatus;
        set
        {
            if (selectStatus == GameEntitySelectStatus.Selected)
            {
                switch (value)
                {
                    case GameEntitySelectStatus.UnSelected:
                        selectStatus = GameEntitySelectStatus.UnSelected;
                        SelectStatus = GameEntitySelectStatus.None;//Auto 2 none
                        return;
                    default:
                        break;
                }
            }
            else
            {
                selectStatus = value;
            }
            
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (SelectStatus == GameEntitySelectStatus.Selected)
        {
            SelectStatus = GameEntitySelectStatus.UnSelected;
            GameEntityMgr.SetSelectedEntity(null);
        }
        else
        {
            SelectStatus = GameEntitySelectStatus.Selected;
            GameEntityMgr.SetSelectedEntity(this);
        }

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SelectStatus = GameEntitySelectStatus.PointerEnter;
        if(SelectStatus == GameEntitySelectStatus.PointerEnter)
            EnableOutLine(true);

    }

    private static List<Vector2Int> eyeSightArea = new List<Vector2Int>();//用作公用
    void ShowEyeSight()
    {
        CleanLastEyeSight();
        UpdateCurrentEyeSight();

    }

    private void UpdateCurrentEyeSight()
    {
        HexCoords.GetHexCellByRadius(currentCell, entityConfig.eyeSight, ref eyeSightArea);
        foreach (Vector2Int v in eyeSightArea)
        {
            CellView cellview = GameEntityMgr.instance.map.GetCellView(v);
            if (cellview != null)
                cellview.SetCellViewStatus(CellViewStatus.EyeSight);
        }
    }

    void CleanLastEyeSight()
    {
        foreach(Vector2Int v in eyeSightArea)
        {
            CellView cellview = GameEntityMgr.instance.map.GetCellView(v);
            if (cellview != null)
                cellview.SetCellViewStatus(CellViewStatus.None);
        }
        eyeSightArea.Clear();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SelectStatus = GameEntitySelectStatus.None;
        if (SelectStatus == GameEntitySelectStatus.None)
            EnableOutLine(false);
    }

    private void EnableOutLine(bool active)
    {
        //展示描边效果
        //目前用换颜色代替模拟
        if (active)
        {
            entityVisual.SetColor(Color.red);
        }
        else
        {
            entityVisual.SetColor(Color.white);
        }
    }
}

public class GameEntityVisual
{
    public Material material;
    public GameEntityVisual(GameObject modelObj)
    {
        material = modelObj.GetComponent<Renderer>().material;
    }

    public void SetColor(Color c)
    {
        material.SetColor("_Color", c);
    }
}


[System.Serializable]
public struct GameEntityConfig
{
    public int eyeSight;
}


public enum GameEntitySelectStatus
{
    None,
    PointerEnter,
    Selected,
    UnSelected,
}

public partial class GameEntity
{

}