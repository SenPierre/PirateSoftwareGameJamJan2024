
public class TaskTransferEnergy : Task
{  
    TargetSelectTransfer m_computer;

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public TaskTransferEnergy(TargetSelectTransfer computer)
    {
        m_computer = computer;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override bool ExecuteTask()
    {
        Mushroom srcMush = null;
        Mushroom trgMush = null;
        if (m_computer.ComputeTarget(ref srcMush, ref trgMush))
        {
            if (srcMush.TransferToOther(trgMush))
            {
                return true;
            }
        }

        return false;
    }
}
