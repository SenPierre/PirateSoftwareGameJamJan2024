using Godot;
using System;
using System.Collections.Generic;

public partial class MushroomPowerTransfer : Node2D
{
    [Export] public float m_speed;

    public List<Mushroom> m_Path;
    public float m_PowerTransfered = 100.0f;

    public override void _Process(double delta)
    {
        base._Process(delta);

        float moveDuringThatFrame = (float)delta * m_speed;
        Vector2 remainingMoveUntilNextNode = m_Path[0].Position - Position;
        if (remainingMoveUntilNextNode.LengthSquared() > moveDuringThatFrame * moveDuringThatFrame)
        {
            Position += remainingMoveUntilNextNode.Normalized() * moveDuringThatFrame;
        }
        else
        {
            Mushroom mushroom = m_Path[0];
            Position = mushroom.Position;
            m_Path.RemoveAt(0);
            if (m_Path.Count == 0)
            {
                mushroom.Transfer(m_PowerTransfered);
                QueueFree();
            }
            else
            {
                // ?
            }
        }
    }
}
