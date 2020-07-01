using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TeammemberAvatarUI : MonoBehaviour, PoolingGameObjectRemote
{

    public GameObject deathBg;
    public GameObject selectedBg;
    public GameObject beAttackBg;
    public GameObject beMovingBg;
    public Text memberName;
    public Button btnMember;

    private void Awake()
    {
        btnMember.onClick.AddListener(() =>
        {
            if (showEntity.BeAlive())
                GameEntityMgr.SetSelectedEntity(showEntity);
        });
        Action loopOneSeconds = () =>
        {
            if (showEntity == null)
                return;
            bool selected = GameEntityMgr.IsEntitySelected(showEntity.entityID);
            GameEntity gameEntity = GameEntityMgr.Instance.GetGameEntity(showEntity.entityID);
            selectedBg.SetActive(selected);
            EntityAnimStatus status = gameEntity.GetEntityVisual().Status;
            deathBg.SetActive(!gameEntity.BeAlive());
            beMovingBg.SetActive(status == EntityAnimStatus.Run);
            beAttackBg.SetActive(status == EntityAnimStatus.Battle);
        };
        GameTimer.AwaitLoopSecondsBaseOnCore(1, loopOneSeconds).ForgetAwait();
    }

    private void OnEnable()
    {
        deathBg.SetActive(false);
        selectedBg.SetActive(false);
        beAttackBg.SetActive(false);
        beMovingBg.SetActive(false);
        memberName.text = showEntity.GetEntityName();

        GameEntityMgr mgr = GameEntityMgr.Instance;
        mgr.AddEntityRuntimeValueChangedListenerByEntityID(showEntity.entityID, onEntityDataChange);
        mgr.onEntitySelected += UpdateSelectEntity;
    }

    private void UpdateSelectEntity()
    {
        bool selected = showEntity != null && GameEntityMgr.IsEntitySelected(showEntity.entityID);
        selectedBg.SetActive(selected);
    }

    private void onEntityDataChange(ValueChangeType changeType)
    {
        bool selected = GameEntityMgr.IsEntitySelected(showEntity.entityID);
        GameEntity gameEntity = GameEntityMgr.Instance.GetGameEntity(showEntity.entityID);
        selectedBg.SetActive(selected);
        EntityAnimStatus status = gameEntity.GetEntityVisual().Status;
        deathBg.SetActive(!gameEntity.BeAlive());
        beAttackBg.SetActive(changeType == ValueChangeType.HPDown);
        beMovingBg.SetActive(status == EntityAnimStatus.Run);
    }

    GameEntity showEntity = null;

    public void SetModel(GameEntity data)
    {
        showEntity = data;
    }
    private void OnDestroy()
    {
        OnEnterPool();
    }

    public void OnEnterPool()
    {
        if (showEntity)
            GameEntityMgr.Instance.RemoveRuntimeValueChangedListener(showEntity.entityID, onEntityDataChange);
        GameEntityMgr.Instance.onEntitySelected -= UpdateSelectEntity;
        showEntity = null;
    }

    public void OnExitPool()
    {
    }
}
