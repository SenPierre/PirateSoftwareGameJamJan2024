
public abstract class TargetSelectTransfer
{
    public MushroomAI m_AI;

    public TargetSelectTransfer(MushroomAI ai) { m_AI = ai; }
    public abstract bool ComputeTarget(ref Mushroom outSrc, ref Mushroom outTarget);
    
}
