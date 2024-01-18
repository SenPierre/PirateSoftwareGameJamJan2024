using Godot;
using System;
using System.Collections.Generic;

public partial class MushroomManager : Node2D
{
    public static MushroomManager manager;
    
    [Export] public Texture2D m_maskField;

    private Mushroom m_currentMushroom;
    public float cooldown = 0.2f;

    public List<Mushroom> m_RootMushrooms = new List<Mushroom>();
    public List<Mushroom> m_AllMushrooms = new List<Mushroom>();

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
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (cooldown > 0.0f)
        {
            cooldown -= (float)delta;
        }
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
        m_AllMushrooms.Remove(mush);
        if (mush.IsRoot())
        {
            m_RootMushrooms.Remove(mush);
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public Mushroom GetNearbyValidMushroom(Mushroom mush)
    {
        float nearestDistanceSquared = 9999.0f * 9999.0f;
        Mushroom result = null;
        foreach(Mushroom shroom in m_AllMushrooms)
        {
            if (mush != shroom && mush.IsSameKind(shroom) && shroom.ConnectedToRoot())
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
        InputEventMouse mouseEvent = (InputEventMouse)generatedEvent;
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
                Mushroom MushroomFound = null;
                foreach(Mushroom mush in m_AllMushrooms)
                {
                    if (GetViewport().GetMousePosition().DistanceSquaredTo(mush.Position) < 100.0f)
                    {
                        MushroomFound = mush;
                        break;
                    }
                }

                if (MushroomFound != null)
                {
                    List<Mushroom> path = new List<Mushroom>();
                    if (MushroomFound.FindMushroomPath(GetCurrentMushroom(), ref path))
                    {
                        PackedScene shroomPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/MushroomPowerTransfer.tscn");
                    
                        MushroomPowerTransfer transfer = shroomPrefab.Instantiate<MushroomPowerTransfer>();
                        transfer.m_Path = path;
                        transfer.Position = GetCurrentMushroom().Position;
                        transfer.m_PowerTransfered = 100.0f;
                        AddSibling(transfer);
                        m_currentMushroom.Transfer(-100.0f);

                        cooldown = 0.2f;
                    }
                }
                else
                {
                    Vector2 pos = GetViewport().GetMousePosition();
                    Color maskColor = m_maskField.GetImage().GetPixel((int)pos.X, (int)pos.Y);
                    m_currentMushroom.SpawnAnOffspring(pos);
                    
                    SetCurrentMushroom(null);
                    cooldown = 0.2f;
                }
            }
        }
    }
}
