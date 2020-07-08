using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SkillUI : MonoBehaviour, PoolingGameObjectRemote
{
    public Text text;
    public Button btnSelectSkill;
    public GameObject selectBg;
    private bool valid = false;


    private void OnEnable()
    {
        UpdateUI();
    }


    void Start()
    {
        if (text == null)
            text = transform.Find("Text").GetComponent<Text>();

        btnSelectSkill?.onClick.AddListener(() =>
        {
            if (valid)
            {
                GameEntityMgr.GetSelectedEntity().SelectSkill(skillID);
            }
        });
    }



    internal void UpdateUI()
    {
        Skill sk = GameCore.GetRegistServices<BattleService>().GetSkillByID(skillID);
        if (sk != null)
            text.text = "技能 " + GameCore.GetRegistServices<BattleService>().GetSkillByID(skillID).desc;
        
        selectBg.SetActive(GameEntityMgr.GetSelectedEntity().GetControllRemote().SelectedSkillID == skillID && sk != null);
    }


    public int skillID;
    public void SetModel(int data)
    {
        skillID = data;
        valid = true;
    }

    public int GetData()
    {
        return skillID;
    }


    public void OnEnterPool()
    {
        skillID = -1;
        selectBg.SetActive(false);
        valid = false;
    }

    public void OnExitPool()
    {
    }
}
