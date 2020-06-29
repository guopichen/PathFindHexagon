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

public interface IGameEntityInit
{
    int ModelID { get; set; }
}




public partial class GameEntity : MonoBehaviour, GameEntityRemote, IGameEntityInit, IDataForCalculateEntityRuntimeData
{
    public int entityID;
    private GameObject m_GameObject;
    private Transform m_Transform;

    public GameEntityConfig entityConfig;

    [SerializeField]
    private GameEntityRuntimeData runtimeData;

    private Vector2Int currentCell;

    public Vector2Int CurrentPoint { get { return currentCell; } }

    public int ModelID { get; set; }

    private MapController mapController;

    GameEntityVisual entityVisual;


    EntityControllType controllType = EntityControllType.None;
    GameEntityControllRemote controllRemote = GameEntityControllBase.emptyEntityControll;

    EntityActionEnum actionEnum = EntityActionEnum.None;
    GameEntityAction actionRemote;


    List<Skill> entitySkillSet;
    public void SetControllType(EntityControllType entityControllStatus)
    {
        this.controllType = entityControllStatus;
    }

    public EntityControllType GetControllType()
    {
        return this.controllType;
    }

    public GameEntityControllRemote GetControllRemote()
    {
        return controllRemote;
    }
    public bool BeAlive()
    {
        return controllRemote.BeAlive();
    }

    IEnumerator player_LogicSwitchA;
    IEnumerator player_AILogicSwitchB;
    IEnumerator runtimeSwitcher = null;


    IEnumerator Start()
    {
        m_Transform = this.transform;
        m_GameObject = this.gameObject;
        yield return null;
        mapController = GameCore.GetRegistServices<MapController>();
        currentCell = mapController.GetRandomCell().Point;
        //mapController.GetCellView(new Vector2Int(0, 0)).GetPoint();
        var mapSize = mapController.GetMapSize();
        this.transform.position = HexCoords.GetHexVisualCoords(currentCell, mapSize);

        //设定职业
        actionEnum = EntityActionEnum.Warrior;
        if (actionEnum == EntityActionEnum.Warrior)
            actionRemote = new WarriorEntityAction(this);
        else
            actionRemote = new GameEntityAction(this);

        player_LogicSwitchA = playerUpdate();
        player_AILogicSwitchB = actionRemote.AutoUpdate();

        //设定模型外观
        int playerIndex = GameEntityMgr.Instance.GetAllPlayers().IndexOf(this);
        entityVisual = new GameEntityVisual(this.transform.Find("Model").gameObject, ModelID, playerIndex);
        int overrideY = 15;
        entityVisual.SetCameraPosition(this.transform.position, overrideY);

        if (controllType == EntityControllType.Player)
        {
            PlayerEntitiyControll player = new PlayerEntitiyControll();
            player.SetEntityID(entityID);
            controllRemote = player;
            runtimeData = controllRemote.UpdateRuntimeData(this);

            //StartCoroutine(playerUpdate());
            runtimeSwitcher = player_LogicSwitchA;
            StartCoroutine(updateContainer());
        }
        else if (controllType == EntityControllType.AI)
        {
            AIEntitiyControll ai = new AIEntitiyControll();
            ai.SetEntityID(entityID);
            controllRemote = ai;
            runtimeData = controllRemote.UpdateRuntimeData(this);

            StartCoroutine(actionRemote.AutoUpdate());
        }




        GameTimer.AwaitLoopSeconds(1, controllRemote.CalledEverySeconds).ForgetAwait();


        //if (controllType == EntityControllType.AI)
        //{
        //    onReachDst += async () =>
        //    {
        //        await new WaitForSeconds(1);
        //        randomMove();
        //    };
        //    randomMove();
        //}
    }


    string entityStoryName = "亚瑟";
    public string GetEntityName()
    {
        return entityStoryName;
    }

    public GameEntityVisual GetEntityVisual()
    {
        return this.entityVisual;
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


    //用于快速切换玩家的自动状态 与 玩家操作

    IEnumerator updateContainer()
    {
        while (BeAlive())
        {
            if (runtimeSwitcher != null && runtimeSwitcher.MoveNext())
            {
                yield return runtimeSwitcher.Current;
            }
            else
                yield return null;
        }
        Debug.Log("Die");
        entityVisual.PlayAnim(EntityAnimEnum.Death);
    }
    public bool pathChanged = true;
    Action onReachDst = delegate { };
    PathChanged pathSensor = new PathChanged();
    IEnumerator playerUpdate()
    {
        while (!entityVisual.modelLoaded)
        {
            yield return null;
        }
        while (BeAlive())
        {
            pathSensor.TakeSample(this);
            if (pathSensor.IsDirty && pathSensor.Value)
            {
                currentPath?.Clear();
                AcceptPathChange();
            }
            if (currentPath == null || currentPath.Count == 0)
            {
                if (currentPath?.Count == 0)
                {
                    currentPath = null;
                    onReachDst();
                    entityVisual.SetAniStatus(EntityAnimStatus.Idle);
                    if (GameEntityMgr.GetSelectedEntity() == this)
                        ShowEyeSight();
                }
                //yield return new WaitForSeconds(0.5f);
                yield return new WaitForEndOfFrame();
            }

            if (currentPath != null && currentPath.Count > 0)
            {
                if (controllRemote.PTiliMove())
                {
                    if (currentPath.Count == 1)
                    {
                        //yield return movefromApoint2Bpoint(currentPath[0], currentPath[0]);
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
                controllRemote.ChangeTili();
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
            //t += (0.1f * entityConfig.speed_config);
            t += 0.1f * runtimeData.speed;
            if (t / total < 0.5)
            {
                //enterCellPoint(from.Point);
            }
            else
            {
                enterCellPoint(to.Point);
            }

            this.transform.position = Vector3.Lerp(fromVisualPos, toVisualPos, t / total);
            entityVisual.PlayAnim(EntityAnimEnum.Run);

            yield return null;
        }
    }

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

        int maxCnt = runtimeData.maxSingleMove;
        //if (path.Count > entityConfig.maxSingleMove_config)
        if (path.Count > maxCnt)
        {
            for (int i = path.Count - 1; i >= 0; i--)
            {
                if (i + 1 > maxCnt)
                    path.RemoveAt(i);
                else
                    break;
            }
        }
        currentPath = path;
    }

    public void PredictPathWillChange()
    {
        pathChanged = true;
    }

    private void AcceptPathChange()
    {
        mapController.QuestNewPath();
    }



    public bool IsEntityAtPoint(Vector2Int point)
    {
        return this.CurrentPoint == point;
    }

    public void UpdateEntityRuntime(float dt)
    {
#if UNITY_EDITOR
        unityTest();
#endif

    }

    private void unityTest()
    {
        if (controllType == EntityControllType.Player)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                runtimeSwitcher = player_LogicSwitchA;
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                ChangeAutoPlayStrategy(GSNPCStrategyEnum.AutoFight);
            }
            else if (Input.GetKeyDown(KeyCode.O))
            {
                runtimeSwitcher = player_AILogicSwitchB;
            }
        }
    }

    public void ChangeAutoPlayStrategy(GSNPCStrategyEnum strategy)
    {
        if (controllType == EntityControllType.Player)
        {
            runtimeSwitcher = player_AILogicSwitchB;
        }
        actionRemote.ChangeStrategy(strategy);
    }

    public void Back2Manual()
    {
        ChangeAutoPlayStrategy(GSNPCStrategyEnum.Empty);
        runtimeSwitcher = player_LogicSwitchA;
    }


    public void CalledEverySeconds()
    {

    }

    public bool PAttack(int i = 1)
    {
        return controllRemote.PAttack(i);
    }

    public bool IsTargetEntityInAttackSight()
    {
        return IsEntityInAttackSight(targetEntity);
        //if (entityConfig.attackSight == 1)
        //    return targetEntity != null && CurrentPoint.GetCellNeighbor().Contains(targetEntity.CurrentPoint);
        //else
        //{
        //    if (targetEntity != null)
        //    {
        //        return beInRange(entityConfig.attackSight, targetEntity.CurrentPoint, ForAttack);
        //    }
        //    return false;
        //}
    }


    public bool IsTargetInPursueSight()
    {
        if (targetEntity == null)
            return false;
        //if (entityConfig.pursueSight_config == 1)
        if (runtimeData.pursueSight == 1)
            return CurrentPoint.GetCellNeighbor().Contains(targetEntity.CurrentPoint);
        else
            //return beInRange(entityConfig.pursueSight_config, targetEntity.CurrentPoint, ForPursue);
            return beInRange(runtimeData.pursueSight, targetEntity.CurrentPoint, ForPursue);
    }

    public bool IsEntityInAttackSight(GameEntity gameEntity)
    {
        if (gameEntity == null)
            return false;

        //if (entityConfig.attackSight_config == 1)
        if (runtimeData.attackSight == 1)
            return CurrentPoint.GetCellNeighbor().Contains(gameEntity.CurrentPoint);
        else
            //return beInRange(entityConfig.attackSight_config, gameEntity.CurrentPoint, ForAttack);
            return beInRange(runtimeData.attackSight, gameEntity.CurrentPoint, ForAttack);

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

    private int GetRangeEntity(int R, GameEntity[] range)
    {
        int length = Physics.OverlapSphereNonAlloc(HexCoords.GetHexVisualCoords(CurrentPoint), R, forSensor, 1 << LayerMask.NameToLayer(ProjectConsts.Layer_Entity));
        int i = 0;
        for (; i < length && i < range.Length; i++)
        {
            GameEntity entity = forSensor[i].GetComponentInParent<GameEntity>();
            if (entity != null && entity != this)
            {
                range[i] = entity;
            }
        }
        return Mathf.Min(length, i + 1);
    }

    public int GetPursueRangeEntity(GameEntity[] range)
    {
        //return GetRangeEntity(entityConfig.pursueSight_config, range);
        return GetRangeEntity(runtimeData.pursueSight, range);
    }


    public void DoAttack(int i = 1)
    {
        if (targetEntity != null)
            this.m_Transform.LookAt(targetEntity.m_Transform.position);
        this.entityVisual.PlayAttack(i);
        this.controllRemote.DoAttack(i);

        BattleService battle = GameCore.GetRegistServices<BattleService>();
        int skillID = 1;
        if (entityConfig.skillSocketSet1 == null)
        {
            skillID = entityConfig.skillSocketSet1[i];
        }
        battle.QuestSkillCalculate(skillID, this.controllRemote, targetEntity.GetControllRemote());
        this.targetEntity.SendCmd(entityID, Command.CaughtDamage, string.Empty);
    }

    public List<int> GetdifferentSkills(List<int> skills)
    {
        if (skills == entityConfig.skillSocketSet1)
            return entityConfig.skillSocketSet2;
        else
            return entityConfig.skillSocketSet1;
    }

    public List<int> GetNowSkillSockets()
    {
        return controllRemote.UpdateRuntimeData(null).skillSockets;
    }

    public void ChangeNowSkillSockets(List<int> skills)
    {
        controllRemote.UpdateRuntimeData(null).skillSockets = skills;
    }



    private void SendCmd(int fromID, Command msg, string arg)
    {
        actionRemote.SendCmd(fromID, msg, arg);
    }

    public GameEntityConfig GetStaticConfig()
    {
        return entityConfig;
    }

    public List<ItemsCanAffectRuntimeData> GetAffectSet()
    {
        return null;
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
    private GameEntity targetEntity;
    List<GameEntity> entityWhoAimAtMeSet = new List<GameEntity>();

    public GameEntity GetTargetEntity()
    {
        return targetEntity;
    }

    public void AimAtTargetEntity(GameEntity target, bool takeAction = true)
    {
        this.targetEntity = target;
        if (targetEntity != null && takeAction)
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
                CleanLastEyeSight();
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
        //setRange(entityConfig.eyeSight_config, ForEye);
        setRange(runtimeData.eyeSight, ForEye);

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
    private GameObject visualRootGo;
    private Transform rootTrans;
    public Material material;

    private GameObject myModelRoot;
    private Transform body;
    private Transform weapon;
    private Animator m_anim;
    public bool modelLoaded = false;

    private Camera myVisualCamera;

    Vector3 originalSize;
    int playerIndex;
    public GameEntityVisual(GameObject rootObj, int modelID, int playerIndexOfTeam)
    {
        visualRootGo = rootObj;
        rootTrans = rootObj.transform;
        originalSize = rootTrans.localScale;
        playerIndex = playerIndexOfTeam;
        loadModel("M" + modelID);
    }


    async void loadModel(string resName)
    {
        ResourceRequest quest = Resources.LoadAsync<ModelView>(resName);
        await quest;
        ModelView view = (ModelView)quest.asset;
        if (view == null || view.GetPrefab() == null)
            return;

        myModelRoot = GameObject.Instantiate(view.GetPrefab());
        myModelRoot.transform.SetParent(visualRootGo.transform);
        myModelRoot.transform.localPosition = (Vector3.zero);
        myModelRoot.transform.localEulerAngles = Vector3.zero;
        body = myModelRoot.transform.Find(view.GetBodyTransformName());
        body = body == null ? myModelRoot.transform : body;
        weapon = myModelRoot.transform.Find(view.GetWeaponTransformName());
        weapon = weapon == null ? body : weapon;
        material = body?.GetComponent<Renderer>().material;
        m_anim = myModelRoot.GetComponent<Animator>();

        string layername = "player";
        if (playerIndex >= 0)
        {
            layername += (playerIndex + 1);

        }
        int layer = LayerMask.NameToLayer(layername);
        body.gameObject.layer =
        weapon.gameObject.layer = layer;

        if (playerIndex >= 0 && false)
        {
            GameObject cameraObject = new GameObject(visualRootGo.name + playerIndex + "camera");
            cameraObject.SetActive(false);
            myVisualCamera = cameraObject.AddComponent<Camera>();

            //Vector3 v = HexCoords.GetHexVisualCoords(dest);
            //v.y = 20;

            myVisualCamera.targetTexture = new RenderTexture(60, 60, 8);
            myVisualCamera.cullingMask = 1 << layer;
            cameraObject.SetActive(true);
        }


        modelLoaded = true;
    }


    public void SetColor(Color c)
    {
        material?.SetColor("_Color", c);
    }

    internal async void PlayAttack(int i = 1)
    {
        //rootTrans.localScale = originalSize * 2;
        PlayAnim(EntityAnimEnum.Attack);
        //await new WaitForSeconds(1);
        //rootTrans.localScale = originalSize;
    }


    private EntityAnimStatus aniStatus = EntityAnimStatus.None;

    public EntityAnimStatus AniStatus
    {
        get => aniStatus;
        set
        {
            if (aniStatus == value || aniStatus == EntityAnimStatus.Death)
                return;
            statusLoseFocus();
            aniStatus = value;
            statusGainFocus();
        }
    }

    private void statusLoseFocus()
    {
        switch (aniStatus)
        {

            case EntityAnimStatus.Battle:
                m_anim.SetBool("Battle", false);
                m_anim.SetBool("Controlled", false);
                break;
            case EntityAnimStatus.Death:
                break;
            case EntityAnimStatus.Run:
                m_anim.SetBool("Run", false);
                break;
            case EntityAnimStatus.None:
            case EntityAnimStatus.Idle:
            default:
                break;
        }
    }

    private void statusGainFocus()
    {
        switch (aniStatus)
        {

            case EntityAnimStatus.Battle:
                Battle();
                break;
            case EntityAnimStatus.Death:
                Die();
                break;
            case EntityAnimStatus.Run:
                Run();
                break;
            case EntityAnimStatus.None:
            case EntityAnimStatus.Idle:
            default:
                Idle();
                break;
        }
    }

    private void Run()
    {
        m_anim.SetBool(Animator.StringToHash("Run"), true);
        m_anim.SetBool(Animator.StringToHash("Battle"), false);
    }

    private void Idle()
    {
        m_anim.SetBool(Animator.StringToHash("Run"), false);
        m_anim.SetBool(Animator.StringToHash("Battle"), false);
    }

    private void Battle()
    {
        m_anim.SetBool(Animator.StringToHash("Battle"), true);
        m_anim.SetBool(Animator.StringToHash("Run"), false);
    }

    private void Die()
    {
        m_anim.SetTrigger(Animator.StringToHash("Die"));
    }

    static Dictionary<EntityAnimEnum, EntityAnimStatus> anim2status = new Dictionary<EntityAnimEnum, EntityAnimStatus>()
    {
        {EntityAnimEnum.None,EntityAnimStatus.None },
        {EntityAnimEnum.Attack,EntityAnimStatus.Battle },
        {EntityAnimEnum.Controlled,EntityAnimStatus.Battle },
        {EntityAnimEnum.Skill,EntityAnimStatus.Battle },
        {EntityAnimEnum.Hit,EntityAnimStatus.Battle },
        {EntityAnimEnum.Death,EntityAnimStatus.Death },
        {EntityAnimEnum.Idle,EntityAnimStatus.Idle },
        {EntityAnimEnum.Run,EntityAnimStatus.Run }
    };


    public async void SetAniStatus(EntityAnimStatus status)
    {
        while (!modelLoaded)
        {
            await new WaitForEndOfFrame();
        }
        AniStatus = status;//会播放动画
        //aniStatus = status;//不会播放动画
    }

    public void PlayAnim(EntityAnimEnum anim)
    {
        SetAniStatus(anim2status[anim]);//设置主状态
        //设置子状态所需
        switch (anim)
        {
            case EntityAnimEnum.Attack:
                m_anim.SetTrigger("Attack");
                break;
            case EntityAnimEnum.Controlled:
                m_anim.SetBool("Controlled", true);
                break;
            case EntityAnimEnum.Hit:
                m_anim.SetTrigger("Hit");
                break;
            case EntityAnimEnum.Skill:
                m_anim.SetTrigger("Skill");
                break;
        }
    }

    public async void SetCameraPosition(Vector3 position, int overrideY)
    {
        while (!modelLoaded)
        {
            await new WaitForEndOfFrame();
        }
        if (myVisualCamera != null)
        {
            position.y = overrideY;
            myVisualCamera.transform.position = position;
            myVisualCamera.transform.LookAt(myModelRoot.transform);
        }
    }
}

//说明动作的归属状态
public enum EntityAnimStatus
{
    None,
    Battle,    //Attack,    //Controlled,    //Skill,    //Hit,
    Death,
    Idle,
    Run,
}


//说明有哪些动作
public enum EntityAnimEnum
{
    None,
    Attack,
    Controlled,
    Skill,
    Hit,
    Death,
    Idle,
    Run,
}


[System.Serializable]
public class GameEntityConfig
{
    public int hp_config;
    public int atk_config;
    public int mag_config;
    public int tili_config;
    public string tili_recovery_config;
    public int maxSingleMove_config;
    public int speed_config;
    public int eyeSight_config;
    public int attackSight_config;
    public int pursueSight_config;
    public int hujia_config;


    public List<int> skillSocketSet1;
    public List<int> skillSocketSet2;


    public float cd1_config;
    public float cd2_config;
    public float cd3_config;
}




public enum GameEntitySelectStatus
{
    None,
    PointerEnter,
    Selected,
    UnSelected,
}

