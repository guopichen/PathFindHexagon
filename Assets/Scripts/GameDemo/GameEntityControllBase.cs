using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public interface GameEntityControllRemote
{
    //void SendCmd(ControllMsg cmd, string arg);
    void UpdateControll();
}
public class GameEntityControllBase : GameEntityControllRemote
{
    public static GameEntityControllBase emptyEntityControll = new GameEntityControllBase();
    protected GameEntityControllBase()
    {

    }

    //public void SendCmd(ControllMsg cmd, string arg)
    //{

    //}

    public void UpdateControll()
    {
        
    }
}

public enum EntityControllType
{
    None,
    Player,
    AI,
}
