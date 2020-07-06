using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
[CreateAssetMenu(fileName = "EntityConfigSet", menuName = "PathFind/EntityConfigSet")]

public class GameEntityConfigSet : ScriptableObject,ISerializationCallbackReceiver
{
    public List<GameEntityConfig> allConfig;

    private Dictionary<EntityZhiye, GameEntityConfig> action2Config = new Dictionary<EntityZhiye, GameEntityConfig>();

    public void OnAfterDeserialize()
    {
        //action2Config = new Dictionary<EntityActionEnum, GameEntityConfig>();
        //allConfig.ForEach((x) => {
        //    if (!action2Config.ContainsKey(x.zhiye))
        //        action2Config.Add(x.zhiye,x);
        //});
    }

    public GameEntityConfig GetConfig(EntityZhiye zhiye)
    {
        if (action2Config.TryGetValue(zhiye, out GameEntityConfig c))
            return c;
        return null;
    }

    public void OnBeforeSerialize()
    {

    }

    public void init()
    {
        allConfig.ForEach((x) => {
            if (!action2Config.ContainsKey(x.zhiye))
                action2Config.Add(x.zhiye, x);
        });
    }
}