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
        selectBg?.SetActive(false);

        if (text == null)
            text = transform.Find("Text").GetComponent<Text>();

        btnSelectSkill?.onClick.AddListener(() =>
        {
            if (valid)
            {
                GameEntityMgr.GetSelectedEntity().SelectSkill(skillID);
                selectBg?.SetActive(true);
            }
        });
    }



    internal void UpdateUI()
    {
        text.text = "技能id " + skillID;
        selectBg.SetActive(GameEntityMgr.GetSelectedEntity().GetControllRemote().SelectedSkillID == skillID);

    }


    int skillID;
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
        valid = false;
    }

    public void OnExitPool()
    {
    }
}
