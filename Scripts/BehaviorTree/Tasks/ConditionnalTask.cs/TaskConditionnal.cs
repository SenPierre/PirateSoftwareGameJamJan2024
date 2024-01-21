
public abstract class TaskConditionnal : Task
{  
    protected Task m_Subtask;

    public abstract bool CheckCondition();
    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public TaskConditionnal(Task subtask)
    {
        m_Subtask = subtask;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override bool ExecuteTask()
    {
        if (CheckCondition())
        {
            return m_Subtask.ExecuteTask();
        }
        return false;
    }
}
