
using System;
using System.Collections.Generic;

public class TargetSelectTransferToNearestOther : TargetSelectTransfer
{
    private MushroomKind m_targetKind;
    
    public TargetSelectTransferToNearestOther(MushroomKind targetKind, MushroomAI ai) : base(ai){m_targetKind = targetKind;}

    public override bool ComputeTarget(ref Mushroom outSrc, ref Mushroom outTarget)
    {
        List<Mushroom> shroomList = MushroomManager.manager.GetAllMushroomOfAKind(m_AI.m_AIKind);
        List<Mushroom> mushList = MushroomManager.manager.GetAllMushroomOfAKind(m_targetKind);

        outSrc = null;
        outTarget = null;

        float targetSquaredDistance = 999999.0f;
        foreach(Mushroom shroom in shroomList)
        {
            foreach(Mushroom mush in mushList)
            {
                float squaredDist = mush.GlobalPosition.DistanceSquaredTo(shroom.GlobalPosition);
                if (shroom.CanSpawnAnOffspring() 
                && squaredDist < targetSquaredDistance)
                {
                    targetSquaredDistance = squaredDist;
                    outTarget = shroom;
                }
            }
        }

        if (outTarget != null)
        {
            foreach(Mushroom shroom in shroomList)
            {
                float shroomForecastPower = Math.Min(shroom.GetPower(), shroom.GetPrevisionalPower());
                if (shroom != outTarget && shroom.WillLooseConnexionIfTransfer() == false && shroomForecastPower > 200.0f &&
                    (outSrc == null || outSrc.GetPrevisionalPower() < shroomForecastPower))
                {
                    outSrc = shroom;
                }
            }
        }

        return outSrc != null;
    }
    
}
