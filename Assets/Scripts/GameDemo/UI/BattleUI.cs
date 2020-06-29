using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUI : MonoBehaviour
{
    public TeamInfo teamInfo;
    public SelectedRoleArea selectedRoleArea;

    void Start()
    {
        GameEntityMgr mgr = GameCore.GetRegistServices<GameEntityMgr>();
        mgr.OnSelectedEntityChanged += UpdateBattleUI;
        mgr.AddEntityRuntimeValueChangedListener((entityID) => {
            teamInfo.UpdateInfo(entityID);
            if (mgr.SelectedEntity != null && entityID == mgr.SelectedEntity.entityID)
            {
                selectedRoleArea.UpdateInfo();
            }

        });

    }

    void UpdateBattleUI()
    {
        selectedRoleArea.UpdateInfo();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            teamInfo.UpdateInfo();
            selectedRoleArea.UpdateInfo();
        }
    }

    private void OnDestroy()
    {
        
    }
}
