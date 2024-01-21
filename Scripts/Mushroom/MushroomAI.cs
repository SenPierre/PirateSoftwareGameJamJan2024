using Godot;
using System;

public partial class MushroomAI : Node2D
{
    [Export] public MushroomKind m_AIKind;

    private BehaviorTree m_BT = new BehaviorTree();

    public override void _Ready()
    {
        base._Ready();

        SetupBehaviorTree();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        m_BT.Update();
    }

    public void SetupBehaviorTree()
    {
        m_BT.m_BaseTask =
            new TaskSelector(
                new TaskConditionnalCooldown(5.0f, 
                    new TaskSelector(
                        new TaskSpawnASprout(new TargetSelectSproutNearestOther(MushroomKind.NEUTRAL, this)),
                        new TaskSpawnASprout(new TargetSelectSproutNearestOther(MushroomKind.GOOD, this))         
                    )
                ),
                new TaskConditionnalCooldown(0.5f, 
                    new TaskSelector(
                        new TaskTransferEnergy(new TargetSelectTransferToNearestOther(MushroomKind.NEUTRAL, this)),
                        new TaskTransferEnergy(new TargetSelectTransferToNearestOther(MushroomKind.GOOD, this))
                    )
                )
            );
    }
}
