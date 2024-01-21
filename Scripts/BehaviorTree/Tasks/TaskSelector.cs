
public class TaskSelector : Task
{  
    private Task[] m_Subtasks;

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public TaskSelector(params Task[]tasks)
    {
        m_Subtasks = tasks;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override bool ExecuteTask()
    {
        foreach(Task subTask in m_Subtasks)
        {
            if (subTask.ExecuteTask())
            {
                return true;
            }
        }
        return false;
    }
}
