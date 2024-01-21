
using Godot;
using System.Collections.Generic;

public class TargetSelectSproutNearestOther : TargetSelectSprout
{
    private MushroomKind m_targetKind;
    private float m_marginRadius = 20.0f;

    public TargetSelectSproutNearestOther(MushroomKind targetKind, MushroomAI ai) : base(ai) { m_targetKind = targetKind; }
    public override bool ComputeTarget(ref Mushroom srcMush, ref Vector2 outPos)
    {
        List<Mushroom> shroomList = MushroomManager.manager.GetAllMushroomOfAKind(m_AI.m_AIKind);
        List<Mushroom> mushList = MushroomManager.manager.GetAllMushroomOfAKind(m_targetKind);

        srcMush = null;

        float targetSquaredDistance = 999999.0f;
        Mushroom src = null;
        Mushroom target = null;
        foreach(Mushroom shroom in shroomList)
        {
            foreach(Mushroom mush in mushList)
            {
                float squaredDist = mush.GlobalPosition.DistanceSquaredTo(shroom.GlobalPosition);
                if (shroom.CanSpawnAnOffspring() 
                && shroom.WillLooseConnexionIfCreateOffspring() == false
                && shroom.GetPower() > 1000.0f 
                && shroom.GetPrevisionalPower() > 1000.0f
                && squaredDist < targetSquaredDistance)
                {
                    targetSquaredDistance = squaredDist;
                    src = shroom;
                    target = mush;
                }
            }
        }

        if (targetSquaredDistance < 999999.0f)
        {
            float targetDistance = Mathf.Sqrt(targetSquaredDistance);
            float maxDistSprout = src.GetRadius();

            if (targetDistance - MushroomManager.c_TooCloseToSprout < maxDistSprout)
            {
                maxDistSprout = targetDistance - MushroomManager.c_TooCloseToSprout;
            }

            float distAfterMargin = maxDistSprout - m_marginRadius;
            Vector2 baseSproutMove = (target.GlobalPosition - src.GlobalPosition).Normalized() * distAfterMargin;
            bool validFound;
            int maxTry = 5;

            do {
                float angle = RandomManager.GetFloatRange(0, 360);
                float dist = RandomManager.GetFloatRange(0, m_marginRadius);
                outPos = src.GlobalPosition + baseSproutMove + Vector2.FromAngle(angle) * dist;
                validFound = MushroomManager.manager.CheckMushroomCloseness(outPos) == null;
                maxTry--;
            } while (maxTry > 0 && validFound == false);

            if (validFound)
            {
                srcMush = src;
                return true;
            }
        }

        return false;
    }
}
