using UnityEngine;
using System.Collections;
using System.Collections.Generic;
////设计思路：sensor不停采集数据，前后数据不一致时置dirty，当被外界访问一次value之后，dirty标识复位。 
public abstract class GSNpcSensorBase{
	
}
public class GSSensor<InType,OutType>
{
    //用来降低update次数,副作用就是在某个关键时候sensor会表现出“故障”，因为没有及时更新数据。同时有可能造成对象null，灵活使用。
    public float sensorTime = 2 / 60.0f;
    protected float _time;

    private bool m_IsDirty = false;

    public bool IsDirty
    {
        get { return m_IsDirty; }
    }

    protected OutType m_Value;

    public OutType Value
    {
        get {
            m_IsDirty = false;
            return m_Value; }
        protected set {
            if(m_Value == null)
            {
                if(value != null)
                {
                    m_Value = value;
                    m_IsDirty = true;
                }
            }
            else
            {
                //if(!m_Value.Equals(value))//this will cause gc........... T_T
                {
                    m_Value = value;
                    m_IsDirty = true;
                }
            }
         }
    }

    
    
    public virtual void TakeSample(InType _in)
    {
        overtime();
    }

    protected bool overtime()
    {
        _time += Time.deltaTime;
        if (_time > sensorTime)
        {
            _time = 0;
            return true;
        }
        return false;
    }
}
