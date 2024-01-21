
public partial class BehaviorTree
{
    public Task m_BaseTask;

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void Update()
    {
        m_BaseTask.ExecuteTask();
    }
}
