using Godot;
using System;
using System.Collections.Generic;

public partial class MushroomPowerTransfer : Node2D
{
    [Export] public float m_speed;
    [Export] public Sprite2D m_Sprite;

    public List<Mushroom> m_Path;
    public float m_PowerTransfered = 100.0f;

    private Mushroom m_previousMushroom = null;
    private bool m_reversing = false;

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
        Mushroom curshroom = m_Path[0];
        Vector2 remainingMoveUntilNextNode = curshroom.Position - Position;
        if (remainingMoveUntilNextNode.LengthSquared() > moveDuringThatFrame * moveDuringThatFrame)
        {
            if (curshroom != m_previousMushroom &&  
            ((m_previousMushroom.IsParentOf(curshroom) == false && curshroom.IsParentOf(m_previousMushroom) == false )  
            || curshroom.ConnectedToRoot() == false))
            {
                m_Path.Clear();
                m_Path.Add(m_previousMushroom);
                m_reversing = true;
            }
            else
            {
                Position += remainingMoveUntilNextNode.Normalized() * moveDuringThatFrame;
            }
        }
        else
        {
            m_previousMushroom = curshroom;
            Position = m_previousMushroom.Position;
            m_Path.RemoveAt(0);
            bool pathBroken = false;
            if (m_Path.Count > 0)
            {
                Mushroom nextshroom = m_Path[0];
                if ((m_previousMushroom.IsParentOf(nextshroom) == false 
                && nextshroom.IsParentOf(m_previousMushroom) == false ) 
                || m_previousMushroom.ConnectedToRoot() == false)
                {
                    pathBroken = true;
                }
            }
            
            if (m_Path.Count == 0 || pathBroken)
            {
                m_previousMushroom.EndTransfer(m_PowerTransfered);
                QueueFree();
            }
        }
    }
}
