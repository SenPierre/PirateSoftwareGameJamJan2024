
using System;
using System.Collections.Generic;

public class TargetSelectTransferToNearestOther : TargetSelectTransfer
{
    private MushroomKind m_targetKind;
    
    public TargetSelectTransferToNearestOther(MushroomKind targetKind, MushroomAI ai) : base(ai){}

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
                if (shroom.WillLooseConnexionIfTransfer() == false &&
                    (outSrc == null || outSrc.GetPrevisionalPower() < shroom.GetPrevisionalPower()))
                {
                    outSrc = shroom;
                }
            }
        }

        return outSrc != null;
    }
    
}
