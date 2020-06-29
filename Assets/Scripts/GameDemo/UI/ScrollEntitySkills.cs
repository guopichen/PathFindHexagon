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
    private PrefabPoolableRemote poolRemote;

    void Awake()
    {
        poolRemote = new PrefabPoolable(this.gameObject);
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
        poolRemote.FreePooling();
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
            GameObject skillUIGo = poolRemote.CreatePrefab(SkillPrefabObj);
            Transform skillUIT = skillUIGo.transform;
            skillUIT.localScale = Vector3.one;
            skillUIT.localPosition = Vector3.zero;
            skillUIT.SetSiblingIndex(i);
            skillUI[i] = skillUIGo.GetComponent<SkillUI>();
            //var data = xxxx
            int data = skill[i];
            Debug.Log(data);
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
        poolRemote.FreePooling();
    }

    public void GenerateNew()
    {
        generateSkillsUI();
    }
}
