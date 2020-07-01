using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public interface ScrollViewRemote
{
    void UpdateUI();
    void DestoryOld();
    void GenerateNew();
}

public class ScrollEntitySkills : MonoBehaviour, ScrollViewRemote
{
    public string SkillPrefabObj;

    private SkillUI[] skillUI;
    //private PrefabPoolableRemote poolRemote;
    private GameObject prefab;

    void Awake()
    {
        //poolRemote = new PrefabPoolable(this.gameObject);
        prefab = Resources.Load<GameObject>(SkillPrefabObj);

    }
    void OnEnable()
    {
        UpdateUI();
    }

    void Start()
    {

    }
    void OnDestory()
    {
        PoolingSystem.Instance.FreePrefabPooling(prefab);
        //poolRemote.FreePooling();
        prefab = null;
    }

    private void generateSkillsUI()
    {
        List<int> skill = GameEntityMgr.GetSelectedEntity()?.GetNowSkillSockets();
        if (skill == null)
            return;
        int cnt = skill.Count;
        skillUI = new SkillUI[cnt];
        for (int i = 0; i < cnt; i++)
        {
            GameObject skillUIGo = //poolRemote.CreatePrefab(SkillPrefabObj);
                PoolingSystem.Instance.GetOneClone_NotActive(prefab);
            Transform skillUIT = skillUIGo.transform;
            skillUIT.SetParent(this.transform);
            skillUIT.localScale = Vector3.one;
            skillUIT.localPosition = Vector3.zero;
            skillUIT.SetSiblingIndex(i);
            skillUI[i] = skillUIGo.GetComponent<SkillUI>();
            //var data = xxxx
            int data = skill[i];
            skillUI[i].SetModel(data);
            skillUIGo.SetActive(true);
        }
        GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.MinSize;
    }
    private void updateSkillsUI()
    {
        if (skillUI != null)
        {
            for (int i = 0; i < skillUI.Length; i++)
            {
                skillUI[i].UpdateUI();
            }
        }
    }

    public void UpdateUI()
    {
        DestoryOld();
        GenerateNew();
    }

    public void DestoryOld()
    {
        if (skillUI != null)
        {
            foreach (var sk in skillUI)
            {
                PoolingSystem.Instance.ReturnClone(sk.gameObject);
            }
        }
    }

    public void GenerateNew()
    {
        generateSkillsUI();
    }

    public void OnStartGame()
    {
        UpdateUI();
    }
}
