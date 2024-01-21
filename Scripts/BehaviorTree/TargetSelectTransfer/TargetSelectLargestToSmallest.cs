
using System;
using System.Collections.Generic;

public class TargetSelectLargestToSmallest : TargetSelectTransfer
{
    public TargetSelectLargestToSmallest(MushroomAI ai) : base(ai){}

    public override bool ComputeTarget(ref Mushroom outSrc, ref Mushroom outTarget)
    {
        List<Mushroom> shroomList = MushroomManager.manager.GetAllMushroomOfAKind(m_AI.m_AIKind);
        RandomManager.RandomizeList(ref shroomList);

        outSrc = null;
        outTarget = null;

        foreach(Mushroom shroom in shroomList)
        {
            if (shroom.WillLooseConnexionIfTransfer() == false && 
                (outSrc == null || outSrc.GetPrevisionalPower() < shroom.GetPrevisionalPower()))
            {
                if (outTarget == null)
                {
                    outTarget = outSrc;
                }
                outSrc = shroom;
            }
            else if (outTarget == null || outTarget.GetPrevisionalPower() > shroom.GetPrevisionalPower())
            {
                outTarget = shroom;
            }
        }

        return outSrc != null && outTarget != null;
    }
    
}
