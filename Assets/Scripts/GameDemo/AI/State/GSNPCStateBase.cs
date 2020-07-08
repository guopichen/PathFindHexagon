using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public interface GSNPCStateRemote : GameEntityMsg
{
    //void UpdateSensor(GSNPCRemote npc);
    void UpdateSensor();
    void EvalRule();
    void ExecuteAction();

    void LoseFocus();
    void GainFocus(GameEntity npc);

    Task ExecuteActionAsync();
}
public class GSNPCStateBase : GSNPCStateRemote
{

    public void EvalRule()
    {

    }


    #region GSNPCStateRemote Members

    public void ExecuteAction()
    {

    }

    #endregion

    #region GSNPCStateRemote Members

    //public void UpdateSensor(GSNPCRemote npc)
    public void UpdateSensor()
    {

    }

    #endregion

    #region GSNPCStateRemote Members


    public void LoseFocus()
    {
    }

    public void GainFocus(GameEntity npc)
    {
    }

    public async Task ExecuteActionAsync()
    {
        await new WaitForEndOfFrame();
    }

    public void SendCmd(int fromID, Command msg, string arg)
    {
    }

    #endregion
}
