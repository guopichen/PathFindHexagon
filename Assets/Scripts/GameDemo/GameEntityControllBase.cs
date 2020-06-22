using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ControllMsg
{
    None,
    CaughtDamage,
}
public interface GameEntityControllRemote
{
    void SendCmd(ControllMsg cmd, string arg);
}
public class GameEntityControllBase : GameEntityControllRemote
{
    public static GameEntityControllBase emptyEntityControll = new GameEntityControllBase();
    protected GameEntityControllBase()
    {

    }

    public void SendCmd(ControllMsg cmd, string arg)
    {

    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

public enum EntityControllType
{
    None,
    Player,
    AI,
}
