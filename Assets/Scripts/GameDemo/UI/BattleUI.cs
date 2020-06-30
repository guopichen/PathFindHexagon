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
        mgr.onEntitySelected += UpdateBattleUI;
        mgr.onSelectedEntityValueChange += () => {
            selectedRoleArea.UpdateInfo();
        };

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
