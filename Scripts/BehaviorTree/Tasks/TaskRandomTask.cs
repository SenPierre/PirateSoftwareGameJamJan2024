using Godot;
using System;
using System.Collections.Generic;

public class TaskRandomTask : Task
{  
    private Task[] m_Subtasks;

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public TaskRandomTask(params Task[]tasks)
    {
        m_Subtasks = tasks;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override bool ExecuteTask()
    {
        List<Task> subTaskRandomList = new List<Task>(m_Subtasks);
        
        RandomManager.RandomizeList(ref subTaskRandomList);

        foreach(Task subTask in m_Subtasks)
        {
            if (subTask.ExecuteTask())
            {
                break;
            }
        }
        return true;
    }
}
