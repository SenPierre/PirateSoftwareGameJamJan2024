
using Godot;

public class TaskConditionnalCooldown : TaskConditionnal
{  
    private float m_TimerCount;
    private float m_TimerCooldown = 0.0f;

    public TaskConditionnalCooldown(float timer, Task subtask) : base(subtask) {m_TimerCount = timer; }

    public override bool CheckCondition()
    {
        if (m_TimerCooldown > 0.0f)
        {
            m_TimerCooldown -= TimeManager.GetDeltaTime();
            return false;
        }
        return true;
    }

    public override bool ExecuteTask()
    {
        if (CheckCondition() && m_Subtask.ExecuteTask())
        {
            m_TimerCooldown = m_TimerCount;
            return true;
        }
        return false;
    }
}
