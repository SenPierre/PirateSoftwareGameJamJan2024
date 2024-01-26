using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public partial class MushroomManager : Node2D
{
    [Export] public CanvasItem PauseScreen;
    [Export] public CanvasItem ResumeButton;
    [Export] public CanvasItem VictoryPanel;
    [Export] public CanvasItem GameOverPanel;
    
    [Export] public CanvasItem SoundCross;

    public static MushroomManager manager;
    public const float c_TooCloseToSprout = 20.0f;

    private Mushroom m_currentMushroom;
    public float cooldown = 0.2f;

    public List<Mushroom> m_RootMushrooms = new List<Mushroom>();
    public List<Mushroom> m_AllMushrooms = new List<Mushroom>();
    public List<Node2D> m_ToRemove = new List<Node2D>();

    private Node2D m_Level;

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _EnterTree()
    {
        manager = this;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _Ready()
    {
        PackedScene levelToLoad = GameManager.manager.m_CurrentLevel;
        m_Level = levelToLoad.Instantiate<Node2D>();
        SoundCross.Visible = !GameManager.manager.m_bSoundActive;
        AddChild(m_Level);
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (cooldown > 0.0f)
        {
            cooldown -= (float)TimeManager.GetDeltaTime();
        }
        
        foreach(Node2D dead in m_ToRemove)
        {
            m_Level.RemoveChild(dead);
        }
        m_ToRemove.Clear();
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void ToggleSound()
    {
        GameManager.manager.m_bSoundActive = !GameManager.manager.m_bSoundActive;
        SoundCross.Visible = !GameManager.manager.m_bSoundActive;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void SetCurrentMushroom(Mushroom mush)
    {
        if (m_currentMushroom != null)
        {
            m_currentMushroom.SetSelected(false);
        }

        m_currentMushroom = mush;
        
        if (m_currentMushroom != null)
        {
            m_currentMushroom.SetSelected(true);
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public bool HasCurrentMushroom()
    {
        return m_currentMushroom != null;
    }


    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public Mushroom GetCurrentMushroom()
    {
        return m_currentMushroom;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void AddShroom(Mushroom mushroom)
    {
        m_AllMushrooms.Add(mushroom);
        if (mushroom.IsRoot())
        {
            m_RootMushrooms.Add(mushroom);
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void RemoveShroom(Mushroom mush)
    {
        m_ToRemove.Add(mush);
        m_AllMushrooms.Remove(mush);
        if (mush.IsRoot())
        {
            m_RootMushrooms.Remove(mush);
            CheckVictoryCondition();
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public bool CanReconnectIfCreateOffspring(Mushroom mush)
    {
        float newRadius = Mathf.Sqrt(mush.GetPower() / 2.0f);
        foreach (Mushroom shroom in m_AllMushrooms)
        {
            if (shroom.IsSameKind(mush) && mush.IsIndirectParentOf(shroom) == false)
            {
                float dist = mush.Position.DistanceTo(shroom.Position);
                if (dist < newRadius + shroom.GetRadius())
                {
                    return true;
                }
            }
        }

        return false;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void CheckVictoryCondition()
    {
        bool defeat = true;
        bool victory = true;
        foreach(Mushroom shroom in m_RootMushrooms)
        {
            if (shroom.GetCurrentKind() == MushroomKind.GOOD)
            {
                defeat = false;
            }
            else if (shroom.GetCurrentKind() == MushroomKind.EVIL1)
            {
                victory = false;
            }
        }

        if (victory)
        {
            TimeManager.GetManager().Pause();
            PauseScreen.Visible = true;
            VictoryPanel.Visible = true;
        }
        else if (defeat)
        {
            TimeManager.GetManager().Pause();
            PauseScreen.Visible = true;
            GameOverPanel.Visible = true;
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void Pause()
    {
        TimeManager.GetManager().Pause();
        PauseScreen.Visible = true;
        ResumeButton.Visible = true;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void EndPause()
    {
        ResumeButton.Visible = false;
        PauseScreen.Visible = false;
        TimeManager.GetManager().Unpause();
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void Quit()
    {
        GameManager.manager.GoToMenu();
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public Mushroom CheckMushroomCloseness(Vector2 referencial)
    {
        foreach(Mushroom shroom in m_AllMushrooms)
        {
            float distSquared = referencial.DistanceSquaredTo(shroom.GlobalPosition); 
            if (distSquared < c_TooCloseToSprout * c_TooCloseToSprout)
            {
                return shroom;
            }
        }

        return null;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public List<Mushroom> GetAllMushroomOfAKind(MushroomKind kind)
    {
        List<Mushroom> shroomList = new List<Mushroom>();
        foreach(Mushroom shroom in m_AllMushrooms)
        {
            if (shroom.GetCurrentKind() == kind)
            {
                shroomList.Add(shroom);
            }
        }

        return shroomList;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public Mushroom GetNearbyValidMushroom(Mushroom mush, bool mustBeSameType)
    {
        float nearestDistanceSquared = 9999.0f * 9999.0f;
        Mushroom result = null;
        foreach(Mushroom shroom in m_AllMushrooms)
        {
            if (mush != shroom 
            && shroom.GetCurrentKind() != MushroomKind.NEUTRAL 
            && (mush.IsSameKind(shroom) || mustBeSameType == false) 
            && shroom.ConnectedToRoot())
            {
                if (mush.CheckIfCanConnect(shroom))
                {
                    float targetDistSquared = (mush.Position - shroom.Position).LengthSquared();
                    if (targetDistSquared < nearestDistanceSquared)
                    {
                        nearestDistanceSquared = targetDistSquared;
                        result = shroom;
                    }
                }
            }
        }

        return result;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void CheckNearbyOpposingMushroom(Mushroom mush)
    {
        foreach(Mushroom shroom in m_AllMushrooms)
        {
            if (mush != shroom && mush.IsOpponentKind(shroom))
            {
                mush.CheckStartFight(shroom);
            }
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void InputEvent(Node viewport, InputEvent generatedEvent, int shapeIdx)
    {
        InputEventMouseButton mouseEvent = generatedEvent as InputEventMouseButton;
        if (mouseEvent != null)
        {
            int buttonLeftPressed = ((int)mouseEvent.ButtonMask) & ((int)MouseButton.Left);
            int buttonRightPressed = ((int)mouseEvent.ButtonMask) & ((int)MouseButton.Right);
            if (m_currentMushroom != null && cooldown <= 0.0f)
            {
                if (buttonRightPressed != 0)
                {
                    SetCurrentMushroom(null);
                    cooldown = 0.2f;
                }
                else if (buttonLeftPressed != 0)
                {
                    if (m_currentMushroom.GetPower() < 200.0 || m_currentMushroom.GetPrevisionalPower() < 200.0f)
                    {
                        SetCurrentMushroom(null);
                        cooldown = 0.2f;
                        return;
                    }

                    Mushroom MushroomFound = CheckMushroomCloseness(GetViewport().GetMousePosition());

                    if (MushroomFound != null && MushroomFound.IsSameKind(m_currentMushroom))
                    {
                        if (m_currentMushroom.WillLooseConnexionIfTransfer() == false)
                        {
                            m_currentMushroom.TransferToOther(MushroomFound, mouseEvent.DoubleClick);
                            cooldown = 0.05f;
                        }
                    }
                    else
                    {
                        Vector2 pos = GetViewport().GetMousePosition();
                        Vector2 move = pos - m_currentMushroom.GlobalPosition;
                        float currentPower = Mathf.Min(m_currentMushroom.GetPower(), m_currentMushroom.GetPrevisionalPower());
                        
                        if (currentPower < move.LengthSquared())
                        {
                            move = move.Normalized() * Mathf.Sqrt(currentPower) * 0.99f;
                            pos = m_currentMushroom.GlobalPosition + move;
                        }

                        MushroomFound = CheckMushroomCloseness(pos);

                        if (MushroomFound == null)
                        {
                            m_currentMushroom.SpawnAnOffspring(pos);
                            SetCurrentMushroom(null);
                            cooldown = 0.2f;
                        }
                    }
                }
            }
        }
    }
}
