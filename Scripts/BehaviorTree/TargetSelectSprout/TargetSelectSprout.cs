
using Godot;

public abstract class TargetSelectSprout
{
    public MushroomAI m_AI;

    public TargetSelectSprout(MushroomAI ai) { m_AI = ai; }
    public abstract bool ComputeTarget(ref Mushroom srcMush, ref Vector2 outPos);
}
