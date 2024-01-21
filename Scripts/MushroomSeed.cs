using Godot;
using System;

public partial class MushroomSeed : Node2D
{
    [Export] public float m_Speed;
    [Export] public Line2D m_Line;

    public Mushroom m_parent;
    public Vector2 m_targetPos;
    public float m_CarriedRadius;

    public float m_currentLerp = 0.0f;

    private Vector2 m_startPos;
    private float m_lerpSpeed; 
    private bool m_BACKDASH = false;

    public override void _Ready()
    {
        base._Ready();
        m_startPos = Position;
        Vector2 MoveToMake = m_targetPos - Position;
        float dist = MoveToMake.Length();
        m_lerpSpeed = 1 / (dist / m_Speed);
        m_currentLerp = 0.0f;
        m_BACKDASH = false;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (m_BACKDASH == false && m_parent.ConnectedToRoot())
        {
            m_currentLerp += m_lerpSpeed * (float)delta;
            if (m_currentLerp >= 1.0f)
            {
                // Move ending
                Position = m_targetPos;
                PackedScene shroomPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/Mushroom.tscn");

                Mushroom newShroom = shroomPrefab.Instantiate<Mushroom>();
                newShroom.Position = m_targetPos;
                newShroom.m_baseRadius = m_CarriedRadius;
                newShroom.m_BaseKind = m_parent.GetCurrentKind();
                m_parent.AddSibling(newShroom);
                m_parent.SeedEndDeployment();
                newShroom.SetParent(m_parent);
                newShroom.UpdateRadius(m_CarriedRadius, false);
                newShroom.SetCanGeneratePower(false);
                QueueFree();
            }
        }
        else
        {
            m_BACKDASH = true;
            m_currentLerp -= m_lerpSpeed * (float)delta;
            if (m_currentLerp <= 0.0f)
            {
                m_parent.SeedEndDeployment();
                m_parent.Transfer(m_CarriedRadius * m_CarriedRadius);
                QueueFree();
            }
        }
        
        Position = m_startPos.Lerp(m_targetPos, m_currentLerp);
        m_Line.SetPointPosition(1, m_parent.Position - Position);
    }


}
