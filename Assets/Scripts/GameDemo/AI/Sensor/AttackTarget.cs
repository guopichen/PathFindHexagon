using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTarget : GSSensor<GameEntity, GameEntity>
{
    static GameEntity[] temp = new GameEntity[3];
    public override void TakeSample(GameEntity _in)
    {
        if (overtime())
        {
            if (_in.GetTargetEntity() == null)
            {
                int cnt = _in.GetPursueRangeEntity(temp);
                if (cnt > 0)
                    Value = temp[0];
            }

        }
    }
}

public class AttackTargetFullMap : GSSensor<GameEntity, GameEntity>
{
    public override void TakeSample(GameEntity _in)
    {

        if (overtime())
        {
            GameEntityMgr mgrRemote = GameEntityMgr.Instance;
            if (_in.GetTargetEntity() == null)
            {
                GameEntity nearest = mgrRemote.GetNearestOpSideEntity(_in);
                if (nearest)
                    Value = nearest;
            }
        }
    }
}
