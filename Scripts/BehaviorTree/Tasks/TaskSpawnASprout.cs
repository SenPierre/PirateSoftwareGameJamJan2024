
using Godot;

public class TaskSpawnASprout : Task
{  
    TargetSelectSprout m_computer;

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public TaskSpawnASprout(TargetSelectSprout computer)
    {
        m_computer = computer;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override bool ExecuteTask()
    {
        Vector2 targetPos = new Vector2();
        Mushroom srcMush = null;
        if (m_computer.ComputeTarget(ref srcMush, ref targetPos))
        {
            if (srcMush.SpawnAnOffspring(targetPos))
            {
                return true;
            }
        }

        return false;
    }
}
