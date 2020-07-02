using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//擅长提供buff的职业，比如魔兽中的牧师
public class MushiEntityAction : GameEntityAction
{
    public MushiEntityAction(GameEntity entity) : base(entity)
    {


    }

    //public override async void Action2Entity(GameEntity gameEntity)
    //{
    //    //对于牧师这种即可以有伤害技能（指定敌方） 又 可能有恢复技能（指定友方）
    //    //那么在操作上的流程统一为
    //    //1. 选中牧师
    //    //2. 从 移动 / 技能 选择 技能
    //    //3. 选择技能并指定目标
    //    //4. 移动到目标的自己可以攻击的范围
    //    //5. 基于选中的技能进行挂机释放技能

    //    if (entity.GetControllType() == EntityControllType.Player)
    //    {
    //        await playerAction2Entity();
    //    }
    //}
}
