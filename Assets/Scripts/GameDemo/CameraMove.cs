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
            GameEntity gameEntity = e.GetRandomActiveEntity();
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
        this.transform.position = v;
    }

    void Start()
    {
    }


    bool beginMove = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FocusOnCurrentEntity();
        }

        if (beginMove)
        {

        }
    }
}
