using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScrollPlayerEntityList : MonoBehaviour, ScrollViewRemote, IGameStart
{
    public string memberPrefabObj;
    private GameObject prefab;
    TeammemberAvatarUI[] teamMembers;

    void Awake()
    {
        GameCore.Instance.AddIGame(this);
        GameEntityMgr.Instance.OnAddNewPlayerDuringRun(UpdateUI);
    }
    void Start()
    {
        prefab = Resources.Load<GameObject>(memberPrefabObj);
    }

    void OnEnable()
    {
        UpdateUI();
    }

    private void OnDestroy()
    {
        PoolingSystem.Instance.FreePrefabPooling(prefab);
        prefab = null;
    }

    public void DestoryOld()
    {
        if (teamMembers != null)
            foreach (var cloneMono in teamMembers)
            {
                PoolingSystem.Instance.ReturnClone(cloneMono.gameObject);
            }
    }

    public void GenerateNew()
    {
        List<GameEntity> myTeam = GameEntityMgr.Instance.GetAllPlayers();
        if (myTeam == null)
            return;
        int cnt = myTeam.Count;
        teamMembers = new TeammemberAvatarUI[cnt];
        for (int i = 0; i < cnt; i++)
        {
            GameObject skillUIGo = //poolRemote.CreatePrefab(memberPrefabObj);
                PoolingSystem.Instance.GetOneClone_NotActive(prefab);
            Transform skillUIT = skillUIGo.transform;
            skillUIT.SetParent(this.transform);
            skillUIT.localScale = Vector3.one;
            skillUIT.localPosition = Vector3.zero;
            skillUIT.SetSiblingIndex(i);
            teamMembers[i] = skillUIGo.GetComponent<TeammemberAvatarUI>();
            GameEntity data = myTeam[i];
            if (teamMembers[i] == null)
                teamMembers[i] = skillUIGo.AddComponent<TeammemberAvatarUI>();
            teamMembers[i].SetModel(data);
            skillUIGo.SetActive(true);
        }
    }

    public void UpdateUI()
    {
        DestoryOld();
        GenerateNew();
    }

    public void OnStartGame()
    {
        UpdateUI();
    }
}
