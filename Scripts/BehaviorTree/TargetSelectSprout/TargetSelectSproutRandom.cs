
using Godot;
using System.Collections.Generic;

public class TargetSelectSproutRandom : TargetSelectSprout
{
    public TargetSelectSproutRandom(MushroomAI ai) : base(ai) {}
    public override bool ComputeTarget(ref Mushroom srcMush, ref Vector2 outPos)
    {
        List<Mushroom> shroomList = MushroomManager.manager.GetAllMushroomOfAKind(m_AI.m_AIKind);
        RandomManager.RandomizeList(ref shroomList);

        srcMush = null;

        foreach(Mushroom shroom in shroomList)
        {
            if (shroom.CanSpawnAnOffspring() && shroom.GetPower() > 900 && shroom.GetPrevisionalPower() > 900)
            {
                int maxTry = 5;
                bool validFound = false;

                do {
                    float angle = RandomManager.GetFloatRange(0, 360);
                    float dist = RandomManager.GetFloatRange(30, shroom.GetRadius());
                    outPos = shroom.GlobalPosition + Vector2.FromAngle(angle) * dist;
                    validFound = MushroomManager.manager.CheckMushroomCloseness(outPos) == null;
                    maxTry--;
                } while (maxTry > 0 && validFound == false);

                if (validFound)
                {
                    srcMush = shroom;
                    return true;
                }
            }
        }

        return false;
    }
}
