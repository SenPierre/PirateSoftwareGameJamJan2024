using Godot;
using System;
using System.Collections.Generic;

public partial class MushroomHelper : Node2D
{
    [Export] public Color m_ValidColor;
    [Export] public Color m_WarningColor;
    [Export] public Color m_InvalidColor;
    [Export] public Color m_OriginColor;

    private bool m_showHelper = false;
    private bool m_valid = false;
    private bool m_warning = false;
    private bool m_WillBreakparent = false;

    private bool m_showTargetSprout = false;
    private bool m_showTargetTransfer = false;

    private Vector2 m_sproutSource;
    private Vector2 m_sproutTarget;
    private float m_sproutRadius;

    private List<Vector2> m_TransferPath = new List<Vector2>();
    private List<Mushroom> m_lostConnexion = new List<Mushroom>();

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _Process(double delta)
    {
        base._Process(delta);

        Mushroom currentMushroom = MushroomManager.manager.GetCurrentMushroom();
        m_showHelper = false;
        m_valid = true;
        m_warning = false;
        m_WillBreakparent = false;
        m_showTargetSprout = false;
        m_showTargetTransfer = false;

        if (currentMushroom != null)
        {
            Vector2 mousePos = GetViewport().GetMousePosition();

            if (currentMushroom.GetPower() < 400 || currentMushroom.GetPrevisionalPower() < 400)
            {
                m_valid = false;
            }

            Mushroom mushroomFound = MushroomManager.manager.CheckMushroomCloseness(mousePos);
            if (mushroomFound == null || mushroomFound.GetCurrentKind() != currentMushroom.GetCurrentKind())
            {
                m_showHelper = true;
                m_showTargetSprout = true;

                m_lostConnexion.Clear();
                currentMushroom.GetLooseConnexionIfCreateOffspring(ref m_lostConnexion);
                float mushroomForecastPower = Mathf.Min(currentMushroom.GetPrevisionalPower(),currentMushroom.GetPower());

                foreach(Mushroom lostMushroom in m_lostConnexion)
                {
                    m_warning = true;
                    if (lostMushroom.IsParentOf(currentMushroom))
                    {
                        m_WillBreakparent = true;
                        break;
                    }
                }

                if (currentMushroom.WillLooseConnexionIfCreateOffspring())
                {
                    m_warning = true;
                }

                Vector2 move = mousePos - currentMushroom.GlobalPosition;
                float currentPower = mushroomForecastPower;
                
                if (currentPower < move.LengthSquared())
                {
                    move = move.Normalized() * Mathf.Sqrt(mushroomForecastPower);
                    mousePos = currentMushroom.GlobalPosition + move;
                }
                
                mushroomFound = MushroomManager.manager.CheckMushroomCloseness(mousePos);
                if (mushroomFound != null)
                {
                    m_valid = false;
                }

                m_sproutSource =  currentMushroom.GlobalPosition;
                m_sproutTarget = mousePos;
                m_sproutRadius = Mathf.Sqrt(mushroomForecastPower / 2.0f);
            } 
            else if (mushroomFound != currentMushroom)
            {
                if (currentMushroom.WillLooseConnexionIfTransfer())
                {
                    m_valid = false;
                }

                List<Mushroom> path = new List<Mushroom>();
                if (mushroomFound.FindMushroomPath(currentMushroom, ref path))
                {
                    m_showHelper = true;
                    m_showTargetTransfer = true;
                    m_TransferPath.Clear();
                    foreach (Mushroom mush in path)
                    {
                        m_TransferPath.Add(mush.GlobalPosition);
                    }
                }
            }
        }
        QueueRedraw();
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _Draw()
    {
        base._Draw();

        if (m_showHelper)
        {
            Color color = m_valid && m_WillBreakparent == false ? (m_warning ? m_WarningColor : m_ValidColor) : m_InvalidColor;
            if (m_showTargetSprout)
            {
                DrawLine(m_sproutSource, m_sproutTarget, color, 2.0f);

                if (m_valid)
                {
                    for (float f = 0.0f; f < 360.0f; f += 30.0f)
                    {
                        DrawCircleArc(m_sproutSource, m_sproutRadius, f, f + 10.0f, 10, m_OriginColor, 1);
                        DrawCircleArc(m_sproutTarget, m_sproutRadius, f, f + 10.0f, 10, color, 2);
                    }
                }
                else
                {
                    DrawLine(   m_sproutTarget + (Vector2.Up + Vector2.Left) * m_sproutRadius / 2.0f, 
                                m_sproutTarget + (Vector2.Down + Vector2.Right) * m_sproutRadius / 2.0f,
                                color, 3.0f);
                                
                    DrawLine(   m_sproutTarget + (Vector2.Up + Vector2.Right) * m_sproutRadius / 2.0f, 
                                m_sproutTarget + (Vector2.Down + Vector2.Left) * m_sproutRadius / 2.0f,
                                color, 3.0f);
                }

                foreach(Mushroom shroom in m_lostConnexion)
                {
                    Vector2 lineCenter = (shroom.Position + m_sproutSource) / 2.0f;
                    
                    DrawLine(   lineCenter + (Vector2.Up + Vector2.Left) * 5.0f, 
                                lineCenter + (Vector2.Down + Vector2.Right) * 5.0f,
                                m_InvalidColor, 3.0f);
                                
                    DrawLine(   lineCenter + (Vector2.Up + Vector2.Right) * 5.0f, 
                                lineCenter + (Vector2.Down + Vector2.Left) * 5.0f,
                                m_InvalidColor, 3.0f);
                }

            }
            else if (m_showTargetTransfer)
            {
                for (int i = 1; i < m_TransferPath.Count; i++)
                {
                    DrawLine(m_TransferPath[i-1], m_TransferPath[i], color, 2.0f);
                }
            }
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    // Because DrawArc don't work ??
    public void DrawCircleArc(
        Vector2 pos, 
        float radius, 
        float angleStart, 
        float angleEnd, 
        int pointCount, 
        Color color, 
        float width)
    {
        float angle = Mathf.DegToRad(angleStart);
        
        float angleDelta = Mathf.DegToRad(angleEnd - angleStart) / (pointCount - 1);

        for (int i = 1; i < pointCount - 1; i++)
        {
            Vector2 offset = Vector2.FromAngle(angle) * radius;
            Vector2 offsetPlusOne = Vector2.FromAngle(angle + angleDelta) * radius;

            DrawLine(pos + offset, pos + offsetPlusOne, color, width);
            angle += angleDelta;
        }
    }

}
