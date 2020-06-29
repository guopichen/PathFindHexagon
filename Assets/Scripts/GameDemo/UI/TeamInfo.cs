using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TeamInfo : MonoBehaviour
{
    [SerializeField]
    private GameObject teamScrollPrefab;

    [SerializeField]
    private ScrollRect teamScroll;
    void Start()
    {
        teamScroll.CalculateLayoutInputVertical();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            GameObject clone = GameObject.Instantiate<GameObject>(teamScrollPrefab);
            clone.transform.SetParent(teamScroll.content);
            //teamScroll.content.GetComponent<ContentSizeFitter>().
            //teamScroll.Rebuild( CanvasUpdate.LatePreRender);
        }
    }

    public void UpdateInfo()
    {
        GameEntityMgr instance = GameEntityMgr.Instance;
        if (instance == null)
            return;

        List<GameEntity> players = instance.GetAllPlayers();

    }

    public void UpdateInfo(int entityID)
    {

    }

}
