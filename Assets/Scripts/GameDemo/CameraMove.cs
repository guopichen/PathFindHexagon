using PathFind;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface CameraMoveRemote
{
    void FocusOnCurrentEntity();


}
public class CameraMove : MonoBehaviour, CameraMoveRemote
{
    [Range(1,10)]
    public float focusSpeed = 1;
    [SerializeField]
    Vector2Int dest;
    public void FocusOnCurrentEntity()
    {
        GameEntityMgr e = GameCore.GetRegistServices<GameEntityMgr>();
        if (e.SelectedEntity != null)
        {
            dest = e.SelectedEntity.CurrentPoint;
            applyView();
        }
        else
        {
            GameEntity gameEntity = e.GetRandomAlivePlayerEntity();
            if (gameEntity != null)
            {
                dest = gameEntity.CurrentPoint;
                applyView();
            }
        }
    }

    private void applyView()
    {
        Vector3 v = HexCoords.GetHexVisualCoords(dest);
        v.y = 20;
        this.transform.position = Vector3.Lerp(this.transform.position, v, Time.deltaTime * focusSpeed);
        //this.transform.position = v;
    }

    void Start()
    {
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FocusOnCurrentEntity();
        }
        FocusOnCurrentEntity();
        
    }
}
