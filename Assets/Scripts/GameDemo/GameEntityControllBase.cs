using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface GameEntityControllRemote
{

}
public class GameEntityControllBase : GameEntityControllRemote
{
    public static GameEntityControllBase emptyEntityControll = new GameEntityControllBase();
    protected GameEntityControllBase()
    {

    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

public enum EntityControllStatus
{
    None,
    Player,
    AI,
}
