using System.Collections;
//近战（类似战士）
public partial class WarriorEntityAction
{
}
public partial class WarriorEntityAction : GameEntityAction
{
    public WarriorEntityAction(GameEntity entity) : base(entity)
    {

    }
    //public override async void Action2Entity(GameEntity targetEntity)
    //{
    //    if (entity.GetControllType() == EntityControllType.Player)
    //    {
    //        await playerAction2Entity();
    //    }
    //}


    #region AI 测试原型

    private IEnumerator aiAction2Entity()
    {
        while (entity.GetTargetEntity() != null)
        {
            yield return aiLogic();
        }
    }

    IEnumerator aiLogic()
    {
        if (entity.IsTargetInPursueSight())
        {
            yield return move2target();
        }
        else
        {
            //out of pursue,stop action to targetentity
            entity.AimAtTargetEntity(null);
        }
    }

   
   
    #endregion
}