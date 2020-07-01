using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SkillUI : MonoBehaviour,PoolingGameObjectRemote
{
    public Text text;

    private void OnEnable()
    {
        UpdateUI();
    }

    
    
    void Start()
    {
        if (text == null)
            text = transform.Find("Text").GetComponent<Text>();
    }

    

    internal void UpdateUI()
    {
        text.text = "技能id " + skillID;
    }


    int skillID;
    public void SetModel(int data)
    {
        skillID = data;
    }

    public void OnEnterPool()
    {
    }

    public void OnExitPool()
    {
    }
}
