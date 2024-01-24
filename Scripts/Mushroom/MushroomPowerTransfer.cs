using Godot;
using System;
using System.Collections.Generic;

public partial class MushroomPowerTransfer : Node2D
{
    [Export] public float m_speed;
    [Export] public Sprite2D m_Sprite;

    public MushroomKind m_kind;
    public List<Mushroom> m_Path;
    public float m_PowerTransfered = 100.0f;

    private Mushroom m_previousMushroom = null;

    public override void _Ready()
    {
        base._Ready();
        if (m_PowerTransfered > 100.0)
        {
            m_Sprite.Scale = Vector2.One * 3.0f;
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        float moveDuringThatFrame = (float)TimeManager.GetDeltaTime() * m_speed;
        Vector2 remainingMoveUntilNextNode = m_Path[0].Position - Position;
        if (remainingMoveUntilNextNode.LengthSquared() > moveDuringThatFrame * moveDuringThatFrame)
        {
            if ((m_previousMushroom != null 
                && (m_previousMushroom.GetCurrentKind() != m_kind 
                 || m_previousMushroom.ConnectedToRoot() == false))
            || (m_Path[0].GetCurrentKind() != m_kind)
            || (m_Path[0].ConnectedToRoot() == false))
            {
                QueueFree();
            }
            else
            {
                Position += remainingMoveUntilNextNode.Normalized() * moveDuringThatFrame;
            }
        }
        else
        {
            Mushroom mushroom = m_Path[0];
            Position = mushroom.Position;
            m_Path.RemoveAt(0);
            bool pathBroken = false;
            foreach (Mushroom room in m_Path)
            {
                if (room.GetCurrentKind() != m_kind || room.ConnectedToRoot() == false)
                {
                    pathBroken = true;
                    break;
                }
            }
            
            if (m_Path.Count == 0 || pathBroken)
            {
                mushroom.Transfer(m_PowerTransfered);
                QueueFree();
            }
        }
    }
}
