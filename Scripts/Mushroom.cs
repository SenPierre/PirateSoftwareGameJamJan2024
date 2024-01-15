using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class Mushroom : Node2D
{
    [Export] public float m_BaseRadius
    {
      get => Mathf.Sqrt(m_basePower);
      set => SetBaseRadius(value);
    }
    
    
    private MaskDrawNode m_Mask;
    private Sprite2D m_Sprite;
    private Sprite2D m_SelectedSprite;
    private Line2D m_Line2D;
    private float m_basePower;

    private float m_Power = 30.0f;
    private bool m_ParentConnectionBroken = false;

    private bool m_Selected = false;

    private Mushroom m_Parent;
    private List<Mushroom> m_Children = new List<Mushroom>();

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _EnterTree()
    {
        m_Mask = GetNode<MaskDrawNode>("MaskDrawer");
        m_Sprite = GetNode<Sprite2D>("Sprite2D");
        m_Line2D = GetNode<Line2D>("Line2D");
        m_SelectedSprite = m_Sprite.GetNode<Sprite2D>("Select");
        base._EnterTree();
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _Ready()
    {
        base._Ready();
        SetSelected(false);
        UpdatePower(m_basePower, false);
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _Process(double delta)
    {
        base._Process(delta);
        
        if (ConnectedToRoot() == false)
        {
            m_Sprite.SelfModulate = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }

        if (ConnectedToRoot() == false && HasChildConnected() == false)
        {
            UpdatePower(m_Power - (float)delta * 100.0f, true);
            if (m_Power <= 0.0f)
            {
                DEAD();
            }
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    private void DEAD()
    {
        m_Parent.m_Children.Remove(this);
        QueueFree();
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    private void SetBaseRadius(float val)
    {
        m_basePower = val * val;
        UpdatePower(m_basePower, false);
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    private void CheckParentConnection()
    {
        if (m_Parent != null && m_ParentConnectionBroken == false)
        {
            float DistParentsquared = (Position - m_Parent.Position).LengthSquared();
            float connexionCapacity = Mathf.Sqrt(m_Power) + Mathf.Sqrt(m_Parent.m_Power);
            float connexionCapacitySquared = connexionCapacity * connexionCapacity;
            if (DistParentsquared > connexionCapacitySquared)
            {
                BreakParentConnection();
            }
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    void BreakParentConnection()
    {
        m_ParentConnectionBroken = true;
        m_Line2D.Visible = false;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    bool ConnectedToRoot()
    {
        if (m_ParentConnectionBroken)
        {
            return false;
        }

        if (m_Parent == null)
        {
            return true;
        }

        return m_Parent.ConnectedToRoot();
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    bool HasChildConnected()
    {
        foreach(Mushroom child in m_Children)
        {
            if (child.m_ParentConnectionBroken == false)
            {
                return true;
            }
        }

        return false;    
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    private void CheckChildrenConnection()
    {
        foreach(Mushroom child in m_Children)
        {
            child.CheckParentConnection();
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void SetSelected(bool val)
    {
        m_Selected = val;
        m_SelectedSprite.Visible = val;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void UpdatePower(float newRadius, bool Check)
    {
        m_Power = newRadius;
        
        if (m_Mask != null) // set of members are done before the _ready.so we need to test the mask validity
        {
            m_Mask.UpdateRadius(Mathf.Sqrt(m_Power));
            m_Sprite.Scale = Vector2.One / 6000.0f * (m_Power + 3000.0f);
        }

        if (Check)
        {
            CheckParentConnection();
            CheckChildrenConnection();
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void InputEvent(Node viewport, InputEvent generatedEvent, int shapeIdx)
    {
        InputEventMouse mouseEvent = (InputEventMouse)generatedEvent;
        int buttonLeftPressed = ((int)mouseEvent.ButtonMask) & ((int)MouseButton.Left);

        if (buttonLeftPressed != 0 && MushroomManager.manager.cooldown <= 0.0f)
        {
            MushroomManager.manager.SetCurrentMushroom(this);
            MushroomManager.manager.cooldown = 0.5f;
            GD.Print("SET");
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void SpawnAnOffspring(Vector2 pos)
    {
        if ((pos - GlobalPosition).LengthSquared() > 400.0f)
        {
            float newPower = m_Power / 2.0f;
            float connectionCapacity = Mathf.Sqrt(newPower) * 2;
            if (connectionCapacity * connectionCapacity > (pos - GlobalPosition).LengthSquared())
            {
                PackedScene shroomPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/Mushroom.tscn");
                
                Mushroom newShroom = shroomPrefab.Instantiate<Mushroom>();
                AddSibling(newShroom);
                newShroom.GlobalPosition = pos;
                newShroom.m_Parent = this;
                newShroom.UpdatePower(newPower, false);
                UpdatePower(newPower, true);
                m_Children.Add(newShroom);
                newShroom.m_Sprite.SelfModulate -= new Color(0.2f, 0.2f, 0.2f, 0.0f);
                newShroom.m_Line2D.Visible = true;
                newShroom.m_Line2D.SetPointPosition(0, Vector2.Zero);
                newShroom.m_Line2D.SetPointPosition(1, Position - pos);
            }
        }
    }
}
