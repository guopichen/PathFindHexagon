using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SelectedRoleArea : MonoBehaviour
{

    private OnRuntimeValueChanged hpevent;
    //public GameObject avatarpanel;
    public GameObject a_statusscroll;
    public Slider hpSlider;
    public Slider magicSlider;
    public Text entityname;



    //public GameObject skillpanel;
    public GameObject s_scroll;
    public Button s_switchSkillList;
    public Button s_item1;
    public Button s_item2;

    private Dictionary<int, int> button2itemid = new Dictionary<int, int>();

    //public GameObject strategypanel;
    public Button st_Autofight;
    public Button st_Jingjie;
    public Button st_Daiji;
    public Button st_BackToManual;

    public GameObject nonePlayer;

    private void Start()
    {
        button2itemid.Add(s_item1.GetInstanceID(), 0);
        button2itemid.Add(s_item2.GetInstanceID(), 0);
        st_Daiji.onClick.AddListener(() =>
        {
            doStrategy(GSNPCStrategyEnum.Daiji);
        });
        st_Jingjie.onClick.AddListener(() =>
        {
            doStrategy(GSNPCStrategyEnum.Jingjie);
        });
        st_Autofight.onClick.AddListener(() =>
        {
            doStrategy(GSNPCStrategyEnum.AutoFight);
        });

        st_BackToManual.onClick.AddListener(() =>
        {
            GameEntityMgr.GetSelectedEntity()?.Back2Manual();
        });

        s_item1.onClick.AddListener(() =>
        {
            startUseItem(button2itemid[s_item1.GetInstanceID()]);
        });
        s_item2.onClick.AddListener(() =>
        {
            startUseItem(button2itemid[s_item2.GetInstanceID()]);
        });
        s_switchSkillList.onClick.AddListener(() =>
        {
            GameEntity entity = GameEntityMgr.GetSelectedEntity();
            if (entity == null)
                return;
            List<int> newSkills = entity.GetdifferentSkills(entity.GetNowSkillSockets());
            entity.ChangeNowSkillSockets(newSkills);

            ScrollViewRemote remote = s_scroll.GetComponent<ScrollViewRemote>();
            remote.UpdateUI();
        });

    }

    private void startUseItem(int itemid)
    {
        bool item_with_notarget = true;
        if (item_with_notarget)
        {
            //consume it right now
        }
        else
        {
            //select a target, currently no implementaion
        }
    }



    private void doStrategy(GSNPCStrategyEnum strategy)
    {
        GameEntityMgr.GetSelectedEntity()?.ChangeAutoPlayStrategy(strategy);
    }


    internal void UpdateInfo()
    {
        if (GameEntityMgr.Instance == null || this.gameObject.activeInHierarchy == false)
            return;
        GameEntity selected = GameEntityMgr.GetSelectedEntity();
        if (selected != null)
        {
            nonePlayer.SetActive(false);
            ScrollViewRemote remote = s_scroll.GetComponent<ScrollViewRemote>();
            remote.UpdateUI();

            entityname.text = selected.GetEntityName();
            hpSlider.value = selected.GetControllRemote().GetHPPer();
            magicSlider.value = selected.GetControllRemote().GetMagicPer();
        }
        else
        {
            nonePlayer.SetActive(true);
        }
    }
}
