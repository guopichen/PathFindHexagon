using PathFind;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public interface GameEntityRemote : GameEntityTransformRemote
{
    void MoveAlongPath(IList<ICell> path);//基于指定的路径移动

    void UpdateEntityRuntime(float dt);

}

public interface GameEntityTransformRemote
{
    Vector2Int CurrentPoint { get; }
}







public partial class GameEntity : MonoBehaviour, GameEntityRemote
{
    public int entityID;
    private GameObject m_GameObject;
    private Transform m_Transform;

    public GameEntityConfig entityConfig;

    [SerializeField]
    private GameEntityRuntimeData runtimeData;



    private Vector2Int currentCell;

    public Vector2Int CurrentPoint { get { return currentCell; } }

    private MapController mapController;

    GameEntityVisual entityVisual;


    EntityControllType controllType = EntityControllType.None;
    GameEntityControllRemote controllRemote = GameEntityControllBase.emptyEntityControll;

    EntityActionEnum actionEnum = EntityActionEnum.None;
    GameEntityAction actionRemote;


    public void SetControllType(EntityControllType entityControllStatus)
    {
        this.controllType = entityControllStatus;
    }

    public EntityControllType GetControllType()
    {
        return this.controllType;
    }

    IEnumerator Start()
    {
        m_Transform = this.transform;
        m_GameObject = this.gameObject;
        yield return null;
        if (controllType == EntityControllType.Player)
        {
            controllRemote = new PlayerEntitiyControll();
        }
        else if (controllType == EntityControllType.AI)
        {
            controllRemote = new AIEntitiyControll();
        }
        actionEnum = EntityActionEnum.Warrior;
        if (actionEnum == EntityActionEnum.Warrior)
            actionRemote = new WarriorEntityAction(this);
        else
            actionRemote = new GameEntityAction(this);

        entityVisual = new GameEntityVisual(this.transform.Find("GameEntity").gameObject);
        StartCoroutine(workAsUpdate());
        mapController = GameCore.GetRegistServices<MapController>();
        currentCell = mapController.GetCellView(new Vector2Int(0, 0)).GetPoint();
        var mapSize = mapController.GetMapSize();
        this.transform.position = HexCoords.GetHexVisualCoords(currentCell, mapSize);
        GameTimer.AwaitLoopSeconds(1, CalledEverySeconds).ForgetAwait();



        if (controllType == EntityControllType.AI)
        {
            onReachDst += async () =>
            {
                await new WaitForSeconds(1);
                randomMove();
            };
            randomMove();
        }
    }


    private void randomMove()
    {
        if (targetEntity != null)
            return;
        ICell fromcell = mapController.GetMap().GetCell(CurrentPoint);
        ICell tocell = mapController.GetRandomCell();
        IList<ICell> path = mapController.GetPathFinder().FindPathOnMap(fromcell, tocell, mapController.GetMap());
        MoveAlongPath(path);
    }

    Action onReachDst = delegate { };
    IEnumerator workAsUpdate()
    {
        yield return null;

        while (true)
        {
            if (currentPath == null || currentPath.Count == 0)
            {
                if (currentPath?.Count == 0)
                {
                    currentPath = null;
                    onReachDst();
                    if (controllType == EntityControllType.Player && GameEntityMgr.GetSelectedEntity() == this)
                        ShowEyeSight();
                }
                yield return new WaitForSeconds(0.5f);
            }

            if (currentPath != null && currentPath.Count > 0)
            {
                if (runtimeData.tili - 1 >= 0)
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
                else
                {
                    currentPath.Clear();
                }

            }

        }
    }
    void fireEntityEvent(entityEvent e)
    {
        switch (e)
        {
            case entityEvent.enterNewCell:
                runtimeData.tili--;
                break;
        }
    }

    internal void GainFocus()
    {
        SelectStatus = GameEntitySelectStatus.Selected;
        entityVisual.SetColor(Color.green);
        ShowEyeSight();
    }

    internal void LoseFocus()
    {
        SelectStatus = GameEntitySelectStatus.UnSelected;
        entityVisual.SetColor(Color.white);
    }

    public IEnumerator movefromApoint2Bpoint(ICell from, ICell to)
    {
        var mapSize = mapController.GetMapSize();
        Vector3 fromVisualPos = HexCoords.GetHexVisualCoords(from.Point, mapSize);
        Vector3 toVisualPos = HexCoords.GetHexVisualCoords(to.Point, mapSize);
        float t = 0;
        float total = 0.1f * 60;
        enterCellPoint(from.Point);
        while (t < total)
        {
            t += (0.1f * entityConfig.speedFactor);
            if (t / total < 0.5)
            {
                //enterCellPoint(from.Point);
            }
            else
            {
                enterCellPoint(to.Point);
            }

            //this.transform.LookAt(HexCoords.GetHexVisualCoords(to.Point));

            this.transform.position = Vector3.Lerp(fromVisualPos, toVisualPos, t / total);


            yield return null;
        }
    }

    //public async Task moveFromAtoB(ICell from, ICell to)
    //{
    //    var mapSize = mapController.GetMapSize();
    //    Vector3 fromVisualPos = HexCoords.GetHexVisualCoords(from.Point, mapSize);
    //    Vector3 toVisualPos = HexCoords.GetHexVisualCoords(to.Point, mapSize);
    //    float t = 0;
    //    float total = 0.1f * 60;
    //    while (t < total)
    //    {
    //        //t += (0.1f * 10) ;
    //        t += (0.1f * entityConfig.speedFactor);
    //        if (t / total < 0.5)
    //        {
    //            enterCellPoint(from.Point);
    //        }
    //        else
    //        {
    //            enterCellPoint(to.Point);
    //        }

    //        this.transform.position = Vector3.Lerp(fromVisualPos, toVisualPos, t / total);

    //        await new WaitForEndOfFrame();
    //    }
    //}


    private void enterCellPoint(Vector2Int point)
    {
        if (currentCell.x == point.x && currentCell.y == point.y)
            return;
        currentCell = point;

        this.transform.LookAt(HexCoords.GetHexVisualCoords(point));
        fireEntityEvent(entityEvent.enterNewCell);
    }
    private enum entityEvent
    {
        enterNewCell,
    }


    private IList<ICell> currentPath = null;

    public void MoveAlongPath(IList<ICell> path)
    {
        if (path == null || path.Count == 0)
            return;

        Debug.Log(path[0].Point + ": " + CurrentPoint);
        if (path.Count > entityConfig.maxSingleMove)
        {
            for (int i = path.Count - 1; i >= 0; i--)
            {
                if (i + 1 > entityConfig.maxSingleMove)
                    path.RemoveAt(i);
                else
                    break;
            }
        }
        currentPath = path;
    }



    public bool IsEntityAtPoint(Vector2Int point)
    {
        return this.CurrentPoint == point;
    }

    public void UpdateEntityRuntime(float dt)
    {

    }

    public void CalledEverySeconds()
    {
        runtimeData.tili += 5;
        runtimeData.cd1 += 1;
        runtimeData.cd1 = Mathf.Clamp(runtimeData.cd1, -30, 0);

        runtimeData.cd2 += 1;
        runtimeData.cd2 = Mathf.Clamp(runtimeData.cd2, -30, 0);


        runtimeData.cd3 += 1;
        runtimeData.cd3 = Mathf.Clamp(runtimeData.cd3, -30, 0);


    }

    public bool PAttack(int i = 1)
    {
        bool cd = false;
        if (i == 1)
            cd = runtimeData.cd1 >= 0;
        else if (i == 2)
            cd = runtimeData.cd2 >= 0;
        else if (i == 3)
            cd = runtimeData.cd3 >= 0;
        return cd;
    }

    public bool IsTargetEntityInAttackSight()
    {
        if (entityConfig.attackSight == 1)
            return targetEntity != null && CurrentPoint.GetCellNeighbor().Contains(targetEntity.CurrentPoint);
        else
        {
            if (targetEntity != null)
            {
                return beInRange(entityConfig.attackSight, targetEntity.CurrentPoint, ForAttack);
            }
            return false;
        }
    }

    public bool IsTargetInPursueSight()
    {
        if (targetEntity == null)
            return false;
        if (entityConfig.pursueSight == 1)
            return CurrentPoint.GetCellNeighbor().Contains(targetEntity.CurrentPoint);
        else
            return beInRange(entityConfig.pursueSight, targetEntity.CurrentPoint, ForPursue);
    }


    static Collider[] forSensor = new Collider[400];


    private bool beInRange(int R, Vector2Int v, int useType)
    {
        if (R == 1)
        {
            return CurrentPoint.GetCellNeighbor().Contains(v);
        }
        setRange(R, useType);
        return rangeSightArea.Contains(v);
    }

    public void DoAttack(int i = 1)
    {
        if (targetEntity != null)
            this.m_Transform.LookAt(targetEntity.m_Transform.position);
        this.entityVisual.PlayAttack(i);
        this.targetEntity.SendCmd(entityID, ControllMsg.CaughtDamage, string.Empty);

        if (i == 1)
            runtimeData.cd1 -= 3;
        else if (i == 2)
            runtimeData.cd2 -= 5;
        else if (i == 3)
            runtimeData.cd3 -= 10;
    }



    private void SendCmd(int fromID, ControllMsg msg, string arg)
    {
        if (controllType != EntityControllType.AI)
            return;
        GameEntity fromEntity = GameCore.GetRegistServices<GameEntityMgr>().GetGameEntity(fromID);
        if (msg == ControllMsg.CaughtDamage)
        {
            if (fromEntity != null && fromEntity != targetEntity)
            {
                AimAtTargetEntity(fromEntity);
            }
            controllRemote.SendCmd(msg, arg);
        }
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
    public GameEntity targetEntity { private set; get; }
    List<GameEntity> entityWhoAimAtMeSet = new List<GameEntity>();

    public void AimAtTargetEntity(GameEntity target)
    {
        this.targetEntity = target;
        if (targetEntity != null)
        {
            actionRemote.Action2Entity(targetEntity);
        }
    }
    public void NoticeBeAimed(GameEntity who)
    {
        if (entityWhoAimAtMeSet.Contains(who))
            return;
        entityWhoAimAtMeSet.Add(who);
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (controllType == EntityControllType.Player)
        {
            if (SelectStatus == GameEntitySelectStatus.Selected)
            {
                GameEntityMgr.SetSelectedEntity(null);
            }
            else
            {
                GameEntityMgr.SetSelectedEntity(this);
            }
        }
        else
        {
            GameEntity selected = GameEntityMgr.GetSelectedEntity();
            if (selected != null && selected.controllType == EntityControllType.Player)
            {
                selected.AimAtTargetEntity(this);
                this.NoticeBeAimed(selected);
            }
        }


    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SelectStatus = GameEntitySelectStatus.PointerEnter;
        if (SelectStatus == GameEntitySelectStatus.PointerEnter)
            EnableOutLine(true);

    }

    private static List<Vector2Int> rangeSightArea = new List<Vector2Int>();//用作公用
    private static List<Vector2Int> attackSightArea = new List<Vector2Int>();
    void ShowEyeSight()
    {
        CleanLastEyeSight();
        UpdateCurrentEyeSight();
    }

    private void UpdateCurrentEyeSight()
    {
        setRange(entityConfig.eyeSight, ForEye);

        foreach (var kvp in CellSelector.allowClickSet)
        {
            CellView cellview = mapController.GetCellView(kvp.Key);
            if (cellview != null)
                cellview.SetCellViewStatus(CellViewStatus.EyeSight);
        }
    }


    private const int ForEye = 1;
    private const int ForPursue = 2;
    private const int ForAttack = 3;


    private void setRange(int R, int useType)
    {
        int length = Physics.OverlapSphereNonAlloc(HexCoords.GetHexVisualCoords(CurrentPoint), R, forSensor);
        rangeSightArea.Clear();
        if (useType == ForEye)
            CellSelector.allowClickSet.Clear();
        for (int i = 0; i < length; i++)
        {
            CellView view = forSensor[i].GetComponent<CellView>();
            if (view != null)
            {
                if (useType == ForEye)
                    CellSelector.allowClickSet.Add(view.GetPoint(), true);
                rangeSightArea.Add(view.GetPoint());
            }
        }
    }

    void CleanLastEyeSight()
    {
        foreach (var kvp in CellSelector.allowClickSet)
        {
            CellView cellview = mapController.GetCellView(kvp.Key);
            if (cellview != null)
                cellview.SetCellViewStatus(CellViewStatus.None);
        }
        rangeSightArea.Clear();
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
    private GameObject rootGo;
    private Transform rootTrans;
    public Material material;
    Vector3 originalSize;
    public GameEntityVisual(GameObject modelObj)
    {
        rootGo = modelObj;
        rootTrans = modelObj.transform;
        originalSize = rootTrans.localScale;
        material = modelObj.GetComponent<Renderer>().material;
    }

    public void SetColor(Color c)
    {
        material.SetColor("_Color", c);
    }

    internal async void PlayAttack(int i = 1)
    {
        rootTrans.localScale = originalSize * 2;
        await new WaitForSeconds(1);
        rootTrans.localScale = originalSize;
    }
}


[System.Serializable]
public class GameEntityConfig
{
    public int hp_config;
    public int atk_config;
    public int mag_config;
    public int tili_config;
    public string tili_recovery_config;
    public int maxSingleMove;
    public int speedFactor;
    public int eyeSight;
    public int attackSight;
    public int pursueSight;
}

[System.Serializable]
public struct GameEntityRuntimeData
{
    public float hp;
    public float atk;
    public float mag;
    public float tili;

    public float cd1;
    public float cd2;
    public float cd3;

    public float detlaTili;
    public float deltaTileTime;
}


public enum GameEntitySelectStatus
{
    None,
    PointerEnter,
    Selected,
    UnSelected,
}

