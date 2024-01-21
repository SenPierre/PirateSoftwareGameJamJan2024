
using System;
using System.Collections.Generic;

public class TargetSelectTransferRandom : TargetSelectTransfer
{
    public TargetSelectTransferRandom(MushroomAI ai) : base(ai){}

    public override bool ComputeTarget(ref Mushroom outSrc, ref Mushroom outTarget)
    {
        List<Mushroom> shroomList = MushroomManager.manager.GetAllMushroomOfAKind(m_AI.m_AIKind);
        RandomManager.RandomizeList(ref shroomList);

        outSrc = null;
        outTarget = null;

        foreach(Mushroom shroom in shroomList)
        {
            if (outSrc == null && shroom.GetPower() > 500 && shroom.GetPrevisionalPower() > 500)
            {
                outSrc = shroom;
            }
            else if (outTarget == null && shroom.GetPower() < 2000 && shroom.GetPrevisionalPower() < 2000)
            {
                outTarget = shroom;
            }
        }

        return outSrc != null && outTarget != null;
    }
    
}
