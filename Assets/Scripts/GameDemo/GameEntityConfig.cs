using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityConfig", menuName = "PathFind/EntityConfig")]

public class GameEntityConfig:ScriptableObject
{
    public int hp_config;
    public int atk_config;
    public int mag_config;
    public int tili_config;
    public string tili_recovery_config;
    public int maxSingleMove_config;
    public int speed_config;
    public int eyeSight_config;
    public int attackSight_config;
    public int pursueSight_config;
    public int hujia_config;


    public List<int> skillSocketSet1;
    public List<int> skillSocketSet2;


    public float cd1_config;
    public float cd2_config;
    public float cd3_config;

    public EntityZhiye zhiye;
}

