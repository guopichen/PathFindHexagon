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


    EntityType controllType = EntityType.None;
    GameEntityControllRemote controllRemote = GameEntityControllBase.emptyEntityControll;

    EntityZhiye actionEnum = EntityZhiye.None;
    GameEntityAction actionRemote;


    public GameEntityActionRemote GetActionRemote()
    {
        return actionRemote;
    }


    List<Skill> entitySkillSet;
    public void SetControllType(EntityType entityControllStatus)
    {
        this.controllType = entityControllStatus;
    }

    public EntityType GetControllType()
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
    IEnumerator playerAndAI_LogicSwitchB;
    IEnumerator runtimeSwitcher = null;


    IEnumerator Start()
    {
        m_Transform = this.transform;
        m_GameObject = this.gameObject;
        yield return null;
        mapController = GameCore.GetRegistServices<MapController>();
        currentCell = mapController.GetRandomCell().Point;
        var mapSize = mapController.GetMapSize();
        this.transform.position = HexCoords.GetHexVisualCoords(currentCell, mapSize);

        //设定职业
        applyZhiye();

        player_LogicSwitchA = playerUpdate();
        playerAndAI_LogicSwitchB = actionRemote.AutoUpdate();

        //设定模型外观
        int index = GameEntityMgr.Instance.GetAllEntities().IndexOf(this);
        entityVisual = new GameEntityVisual(this.transform.Find("Model").gameObject, ModelID, index);
        int overrideY = 15;
        entityVisual.SetCameraPosition(this.transform.position, overrideY);

        if (controllType == EntityType.Player)
        {
            PlayerEntitiyControll player = new PlayerEntitiyControll();
            player.SetEntityID(entityID);
            controllRemote = player;
            runtimeData = controllRemote.GetOrUpdateRuntimeData(this);

            runtimeSwitcher = player_LogicSwitchA;
            StartCoroutine(updateContainer());
        }
        else if (controllType == EntityType.AI)
        {
            entityVisual.ChangeHPColor(Color.cyan);
            AIEntitiyControll ai = new AIEntitiyControll();
            ai.SetEntityID(entityID);
            controllRemote = ai;
            runtimeData = controllRemote.GetOrUpdateRuntimeData(this);
            runtimeSwitcher = playerAndAI_LogicSwitchB;
            StartCoroutine(updateContainer());
        }
        else if(controllType == EntityType.PlayerSummon)
        {
            entityVisual.ChangeHPColor(Color.yellow);
            AIEntitiyControll ai = new AIEntitiyControll();
            ai.SetEntityID(entityID);
            controllRemote = ai;
            runtimeData = controllRemote.GetOrUpdateRuntimeData(this);
            runtimeSwitcher = playerAndAI_LogicSwitchB;
            StartCoroutine(updateContainer());
        }



        GameTimer.AwaitLoopSecondsBaseOnCore(1, controllRemote.CalledEverySeconds).ForgetAwait();


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

    private void applyZhiye()
    {
        entityConfig = GameEntityMgr.Instance.GetConfigByZhiye(actionEnum);
        switch (actionEnum)
        {
            case EntityZhiye.Warrior:
                actionRemote = new WarriorEntityAction(this);
                break;
            case EntityZhiye.Magical:
                actionRemote = new FashiEntityAction(this);
                break;
            case EntityZhiye.Mushi:
                actionRemote = new MushiEntityAction(this);
                break;
            case EntityZhiye.None:
            default:
                actionRemote = new GameEntityAction(this);
                break;
        }
        
    }

    public void SetEntityZhiyeConfig(EntityZhiye zhiye)
    {
        actionEnum = zhiye;
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
        entityVisual.PlayAnim(EntityAnimEnum.Death);
        yield return new WaitForSeconds(3);
        GameEntityMgr.Respawn(this);
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
                if (controllRemote.PTiliMove(1))
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
                controllRemote.ChangeTili(-1);
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
        controllRemote?.CalledEveryFrame();
#if UNITY_EDITOR
        unityTest();
#endif

    }

    private void unityTest()
    {
        if (controllType == EntityType.Player)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                runtimeSwitcher = player_LogicSwitchA;
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                ChangeAutoPlayStrategy(GSNPCStrategy.AutoFight);
            }
            else if (Input.GetKeyDown(KeyCode.O))
            {
                runtimeSwitcher = playerAndAI_LogicSwitchB;
            }
        }
    }

    public void ChangeAutoPlayStrategy(GSNPCStrategy strategy)
    {
        if (controllType == EntityType.Player)
        {
            runtimeSwitcher = playerAndAI_LogicSwitchB;
        }
        actionRemote.ChangeStrategy(strategy);
    }

    public void Back2Manual()
    {
        ChangeAutoPlayStrategy(GSNPCStrategy.Empty);
        runtimeSwitcher = player_LogicSwitchA;
    }


    public void CalledEverySeconds()
    {

    }

    public void SelectSkill(int skillID)
    {
        controllRemote.ChangeSelectedSkill(skillID);
    }

    public bool PReleaseSkill(int selectSkillID)
    {
        return controllRemote.PReleaseSkill(selectSkillID);
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

    private int GetRangeEntity(int R, GameEntity[] range, bool wantopsite = true)
    {
        int length = Physics.OverlapSphereNonAlloc(HexCoords.GetHexVisualCoords(CurrentPoint), R, forSensor, 1 << LayerMask.NameToLayer(ProjectConsts.Layer_Entity));
        int i = 0;

        for (int m = 0; m < length && m < range.Length; m++)
        {
            GameEntity entity = forSensor[m].GetComponentInParent<GameEntity>();
            if (wantopsite)
            {
                if (entity != null && beEneymyToMe(entity))
                {
                    range[i++] = entity;
                }
            }
            else
            {
                if (entity != null && entity != this)
                {
                    range[i++] = entity;
                }
            }
        }
        return Mathf.Min(length, i);
    }

    public bool beEneymyToMe(GameEntity target)
    {
        if (target == null)
            return false;
        EntityType entityType = GetControllType();
        if (entityType == target.GetControllType())
            return false;
        if (entityType == EntityType.Player)
            return target.GetControllType() == EntityType.AI;
        else if (entityType == EntityType.AI)
            return target.GetControllType() != EntityType.AI;
        else if (entityType == EntityType.PlayerSummon)
            return target.GetControllType() == EntityType.AI;
        return false;
    }

    public int GetPursueRangeEntity(GameEntity[] range)
    {
        return GetRangeEntity(runtimeData.pursueSight, range);
    }


    public void DoReleaseSkill(int skillID)
    {
        if (targetEntity != null)
            this.m_Transform.LookAt(targetEntity.m_Transform.position);

        EntityAnimEnum s = EntityAnimEnum.Attack;
        Skill skill = GameCore.GetRegistServices<BattleService>().GetSkillByID(skillID);
        if (skill != null)
            s = skill.playAniWhenRelease;
        this.entityVisual.PlayReleaseSkill(s);//动画
        this.controllRemote.DoReleaseSkill(skill);//更新cd

        //技能结算
        BattleService battle = GameCore.GetRegistServices<BattleService>();
        //battle.QuestSkillCalculate(selectSkillID, this.controllRemote, targetEntity.GetControllRemote());
        battle.QuestSkillCalculate(skillID, this);
        //this.targetEntity.SendCmd(entityID, Command.CaughtDamage, string.Empty);
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
        return controllRemote.GetOrUpdateRuntimeData(null).activeSkillSockets;
    }

    public void ChangeNowSkillSockets(List<int> skills)
    {
        controllRemote.GetOrUpdateRuntimeData(null).ChangeActiveSkillSockets(skills);
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
        if (controllType == EntityType.Player)
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
            if (beEneymyToMe(selected))
                //selected != null && selected.controllType == EntityType.Player)
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

    private GameObject hudRoot;
    private Material hudMaterial;

    private GameObject myModelRoot;
    private Transform body;
    private Transform weapon;
    private Animator m_anim;
    public bool modelLoaded = false;

    private Camera myVisualCamera;

    Vector3 originalSize;
    int index;
    public GameEntityVisual(GameObject rootObj, int modelID, int index)
    {
        visualRootGo = rootObj;
        rootTrans = rootObj.transform;
        originalSize = rootTrans.localScale;
        this.index = index;

        hudRoot = rootTrans.Find("Visual/hud")?.gameObject;
        if (hudRoot)
            hudMaterial = hudRoot.GetComponent<Renderer>().material;
        loadModel("M" + modelID);

        GameEntityMgr.Instance.AddEntityRuntimeValueChangedListenerByIndex(this.index, (changeType) =>
        {
            if (hudMaterial != null)
            {
                GameEntity entity = GameEntityMgr.Instance.GetAllEntities()[this.index];
                float hp = entity.GetControllRemote().GetHPPer();
                SetHPPer(hp);
            }
        });
    }

    public void ChangeHPColor(Color c)
    {
        hudMaterial.SetColor("_Color_Blood", c);
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
        //if (index >= 0)
        //{
        //    layername += (index + 1);
        //}
        int layer = LayerMask.NameToLayer(layername);
        body.gameObject.layer =
        weapon.gameObject.layer = layer;

        //if (index >= 0 && false)
        //{
        //    GameObject cameraObject = new GameObject(visualRootGo.name + index + "camera");
        //    cameraObject.SetActive(false);
        //    myVisualCamera = cameraObject.AddComponent<Camera>();

        //    //Vector3 v = HexCoords.GetHexVisualCoords(dest);
        //    //v.y = 20;

        //    myVisualCamera.targetTexture = new RenderTexture(60, 60, 8);
        //    myVisualCamera.cullingMask = 1 << layer;
        //    cameraObject.SetActive(true);
        //}


        modelLoaded = true;
    }


    public void SetColor(Color c)
    {
        material?.SetColor("_Color", c);
    }

    internal void PlayReleaseSkill(EntityAnimEnum i)
    {
        PlayAnim(i);
    }


    private EntityAnimStatus statuse = EntityAnimStatus.None;

    public EntityAnimStatus Status
    {
        get => statuse;
        set
        {
            if (statuse == value || statuse == EntityAnimStatus.Death)
                return;
            statusLoseFocus();
            statuse = value;
            statusGainFocus();
        }
    }

    private void statusLoseFocus()
    {
        switch (statuse)
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
        switch (statuse)
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
        Status = status;//会播放动画
        //aniStatus = status;//不会播放动画
    }

    public void PlayAnim(EntityAnimEnum anim)
    {
        SetAniStatus(anim2status[anim]);//设置主状态
                                        //设置子状态所需

        switch (anim)
        {
            case EntityAnimEnum.Attack:
                m_anim?.SetTrigger("Attack");
                break;
            case EntityAnimEnum.Controlled:
                m_anim?.SetBool("Controlled", true);
                break;
            case EntityAnimEnum.Hit:
                m_anim?.SetTrigger("Hit");
                break;
            case EntityAnimEnum.Skill:
                m_anim?.SetTrigger("Skill");
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

    void SetHPPer(float hpper)
    {
        if (hpper <= 0)
            hudRoot?.SetActive(false);
        hudMaterial?.SetFloat("_Progress_A", hpper);
    }
}


public partial class GameEntity
{
    public IEnumerator playerAction2Entity()
    {
        while (BeAlive() && GetTargetEntity() != null && GetTargetEntity().BeAlive())
        {
            yield return move2target();
            yield return doSkill2target();
        }
        if (BeAlive())
            GetEntityVisual().Status = EntityAnimStatus.Idle;
    }
    public IEnumerator doSkill2target()
    {
        if (IsTargetEntityInAttackSight())
        {
            if (PReleaseSelectedSkill())
            {
                DoReleaseSelectedSkill();
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator move2target()
    {
        MapController map = GameCore.GetRegistServices<MapController>();
        IList<ICell> path = map.GetPathFinder().FindPathOnMap(
            map.GetMap().GetCell(CurrentPoint),
            map.GetMap().GetCell(GetTargetEntity().CurrentPoint),
            map.GetMap());
        if (path != null && path.Count >= 2)
        {
            path.RemoveAt(path.Count - 1);
            ICell dest = path[path.Count - 1];
            int startIndex = 0;
            int destIndex = path.Count - 1;
            while (true && startIndex < path.Count - 1)
            {
                if (startIndex + 1 <= path.Count - 1)
                {
                    if (IsTargetEntityInAttackSight() || GetControllRemote().PTiliMove(1) == false)
                    {
                        if (entityVisual.Status == EntityAnimStatus.Run)
                            GetEntityVisual().SetAniStatus(EntityAnimStatus.Idle);
                        yield break;
                    }
                    yield return movefromApoint2Bpoint(path[startIndex], path[startIndex + 1]);
                }
                startIndex = startIndex + 1;
            }
        }
    }
    public void DoReleaseSelectedSkill()
    {
        DoReleaseSkill(GetControllRemote().SelectedSkillID);
        HDebug.Log("entity " + gameObject.name + " release skill :" + GetControllRemote().SelectedSkillID + " target " + GetTargetEntity().gameObject.name);
    }
    public bool PReleaseSelectedSkill()
    {
        return PReleaseSkill(GetControllRemote().SelectedSkillID);
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






public enum GameEntitySelectStatus
{
    None,
    PointerEnter,
    Selected,
    UnSelected,
}

