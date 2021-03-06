﻿using PathFind;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PathFind
{
    public class PathInfoView : MonoBehaviour
    {
        [SerializeField] private MapController m_mapController  = null;
        [SerializeField] private TMP_Text m_startCelllValue = null;
        [SerializeField] private TMP_Text m_endCelllValue = null;
        [SerializeField] private TMP_Text m_distanceValue = null;


        [SerializeField]
        private Button addPlayer = null;
        [SerializeField]
        private Button addNPC = null;
        [SerializeField]
        private Button runGameOrPauseGame = null;



        private void Start()
        {
            m_mapController.OnStartCellSelect += OnStartCellSelect;
            m_mapController.OnEndCellSelect += OnEndCellSelect;
            m_mapController.OnPathFind += OnFindPath;

            addPlayer.onClick.AddListener(()=> { GameCore.SpawnPlayer(EntityZhiye.Mushi); });
            addNPC.onClick.AddListener(()=> { GameCore.SpawnNPC(EntityZhiye.Mushi); });
            runGameOrPauseGame.onClick.AddListener(GameCore.RunOrPauseCore);

            autoStartGameCore();
        }

        private async void autoStartGameCore()
        {
            await new WaitForSeconds(1);
            addPlayer.onClick.Invoke();
            addNPC.onClick.Invoke();
            runGameOrPauseGame.onClick.Invoke();
        }

        private void OnFindPath(IList<ICell> path)
        {
            if (path != null && path.Count > 0)
            {
                m_distanceValue.text = path.Count.ToString();
            }
            else
            {
                m_distanceValue.text = "path no find";
            }
        }

        private void OnStartCellSelect(ICell cell)
        {
            //m_startCelllValue.text = cell.Point.ToString();
        }
        private void OnEndCellSelect(ICell cell)
        {
            m_endCelllValue.text = cell.Point.ToString();
        }

        private void OnDestroy()
        {
            m_mapController.OnStartCellSelect -= OnStartCellSelect;
            m_mapController.OnEndCellSelect -= OnEndCellSelect;
            m_mapController.OnPathFind -= OnFindPath;
        }
    }
}