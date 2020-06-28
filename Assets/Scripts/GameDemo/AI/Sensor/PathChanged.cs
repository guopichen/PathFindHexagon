using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PathChanged : GSSensor<GameEntity, bool>
{

    public override void TakeSample(GameEntity _in)
    {
        if(overtime())
        {
            if(_in.pathChanged)
            {
                Value = true;
                _in.pathChanged = false;
            }
        }
    }
}
