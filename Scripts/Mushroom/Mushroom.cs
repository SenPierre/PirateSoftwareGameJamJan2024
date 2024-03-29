using Godot;
using System;
using System.Collections.Generic;

public enum MushroomKind
{
    GOOD,
    EVIL1,
    EVIL2,
    NEUTRAL
}

[Tool]
public partial class Mushroom : Node2D
{
    Color[] c_kindColor =
    {
        new Color(1.0f, 0.0f, 0.0f),
        new Color(0.0f, 1.0f, 0.0f),
        new Color(1.0f, 1.0f, 0.0f),
        new Color(0.0f, 0.0f, 1.0f),
    };

    static float c_AllegeanceTimer = 3.0f;

    [Export] public float m_baseRadius;
    [Export] public MushroomKind m_BaseKind;
    [Export] public Texture2D m_ForcedSprite;
    [Export] public Texture2D m_ForcedSelectSprite;
    [Export] public Curve m_GenerationCurve;

    private MaskDrawNode m_Mask;
    private Sprite2D m_Sprite;
    private Sprite2D m_SelectedSprite;
    private Line2D m_Line2D;
    private Label m_PowerLabel;
    private SoundPlayer m_SoundPlayer;
    private Node2D m_Particles;

    private bool m_ParentConnectionBroken = false;

    private bool m_IsDead = false;

    private bool m_Selected = false;

    private MushroomKind m_currentKind;
    private float m_radius;

    private float m_ShowPowerCooldown = 0.0f;

    private Mushroom m_Parent;
    private List<Mushroom> m_Children = new List<Mushroom>();

    private List<Mushroom> m_challengers = new List<Mushroom>();

    private bool m_generatePower = true;

    private MushroomSeed m_deployedSeed = null;
    private float m_lostPower = 0.0f;
    private float m_powerLossFromSeedLerp = 0.0f;

    private bool m_isSpawning = false;
    private float m_SpawningLerp = 1.0f;
    private float m_TrueSpawningLerp = 1.0f;

    private float m_transferRemaining = 0.0f;

    private float m_LoosingAlledgance = c_AllegeanceTimer;

    private float m_MassTransferAbsorbTimer = 0.0f;
    private MushroomPowerTransfer m_lastSentTransfer = null;

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _EnterTree()
    {
        m_Mask = GetNode<MaskDrawNode>("MaskDrawer");
        m_Sprite = GetNode<Sprite2D>("Sprite2D");
        m_Line2D = GetNode<Line2D>("Line2D");
        m_SelectedSprite = m_Sprite.GetNode<Sprite2D>("Select");
        m_Particles = m_Sprite.GetNode<Node2D>("CPUParticles2D");
        m_PowerLabel = GetNode<Label>("PowerLabel");
        m_SoundPlayer = GetNode<SoundPlayer>("SoundPlayer");
        base._EnterTree();
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _Ready()
    {
        base._Ready();

        PickASprite();

        SetSelected(false);
        UpdateKind(m_BaseKind);
        UpdateRadius(m_baseRadius, false);

        if (Engine.IsEditorHint() == false)
        {
            MushroomManager.manager.AddShroom(this);
        }
        m_isSpawning = true;
        m_SpawningLerp = 0.0f;
        m_TrueSpawningLerp = 0.0f;

        m_Particles.Visible = m_generatePower;

        m_SoundPlayer.PlayPop();
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    private void PickASprite()
    {
        if (m_ForcedSprite != null && m_ForcedSelectSprite != null)
        {
            m_Sprite.Texture = m_ForcedSprite;
            m_SelectedSprite.Texture = m_ForcedSelectSprite;
        }
        else
        {
            MushroomSpriteData data = MushroomSpriteDatabase.database.PickARandomSprite();
            m_Sprite.Texture = data.m_Sprite;
            m_SelectedSprite.Texture = data.m_SpriteSelect;
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _Process(double delta)
    {
        base._Process(delta);

        if (m_IsDead)
        {
            return;
        }

        float trueDelta = 0.0f;
        if (TimeManager.GetManager() != null)
        {
            trueDelta = TimeManager.GetDeltaTime();
        }

        if (m_MassTransferAbsorbTimer > 0.0f)
        {
            m_MassTransferAbsorbTimer -= trueDelta;
        }

        UpdateSpawning(trueDelta);
        UpdateColor();
        UpdatePowerFluctuating(trueDelta);

        if (m_radius <= 2.0f)
        {
            DEAD();
        }
        else
        {
            m_ShowPowerCooldown -= trueDelta;
            UpdatePowerLabel();
        }


        if (m_currentKind == MushroomKind.NEUTRAL && Engine.IsEditorHint() == false)
        {
            TryToReconnectToNearbyShroom(false);
        }

        if (m_ParentConnectionBroken && m_currentKind != MushroomKind.NEUTRAL)
        {
            TryToReconnectToNearbyShroom(true);
            if (m_ParentConnectionBroken)
            {
                m_LoosingAlledgance -= (float)delta;
                if (m_LoosingAlledgance <= 0.0f)
                {
                    UpdateKind(MushroomKind.NEUTRAL);
                    SetParent(null);
                }
            }
        }
        QueueRedraw();
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    private void UpdateSpawning(double delta)
    {
        if (m_isSpawning)
        {
            m_TrueSpawningLerp += (float)delta;
            if (m_TrueSpawningLerp >= 1.0f)
            {
                m_TrueSpawningLerp = 1.0f;
                m_isSpawning = false;
            }
            m_SpawningLerp = Mathf.Ease(m_TrueSpawningLerp, 0.25f);
            UpdateRadius(m_radius, false);
        }

    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    private void UpdatePowerFluctuating(double delta)
    {
        float powerToAdd = 0.0f;

        foreach (Mushroom challenger in m_challengers)
        {
            powerToAdd -= (float)delta * 400.0f;
        }

        if (m_transferRemaining != 0.0f)
        {
            float transferSpeed = m_transferRemaining > 0.0f ? 500.0f : 5000.0f;
            float transferedThisFrame = Math.Min((float)delta * transferSpeed, Math.Abs(m_transferRemaining)) * Math.Sign(m_transferRemaining);
            powerToAdd += transferedThisFrame;
            m_transferRemaining -= transferedThisFrame;
        }

        float currentPower = m_radius * m_radius;
        if (m_generatePower && m_currentKind != MushroomKind.NEUTRAL && currentPower < 10000.0f)
        {
            float generationRate = m_GenerationCurve.Sample(currentPower / 10000.0f);
            powerToAdd += generationRate * (float)delta;
        }

        /*
                if (ConnectedToRoot() == false && HasChildConnected() == false)
                {
                    powerToRemove += (float)delta * 100.0f;
                }
        */

        if (m_deployedSeed != null)
        {
            float newlostPower = m_powerLossFromSeedLerp * m_deployedSeed.m_currentLerp;
            powerToAdd -= newlostPower - m_lostPower;
            m_lostPower = newlostPower;
        }

        if (powerToAdd != 0.0f)
        {
            float newPower = m_radius * m_radius + powerToAdd;
            if (newPower <= 0)
            {
                UpdateRadius(0.0f, true);
            }
            else
            {
                UpdateRadius(Mathf.Sqrt(newPower), true);
            }
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    private void UpdatePowerLabel()
    {
        Color modulate = new Color(1, 1, 1, m_Selected ? 1.0f : Mathf.Min(m_ShowPowerCooldown, 1.0f));
        m_PowerLabel.SelfModulate = modulate;


        m_PowerLabel.Text = Mathf.Round(m_SpawningLerp * m_radius * m_radius).ToString();
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    private void UpdateColor()
    {
        m_Sprite.SelfModulate = IsRoot() == false
            ? new Color(0.72f, 0.42f, 0.1f, 1.0f)
            : new Color(0.22f, 0.12f, 0.0f, 1.0f);


        if (m_Mask != null) // set of members are done before the _ready.so we need to test the mask validity
        {
            Color offsetConnection = new Color(0, 0, 0, 0);
            if (m_ParentConnectionBroken)
            {
                offsetConnection.B = (c_AllegeanceTimer - m_LoosingAlledgance) / c_AllegeanceTimer;
            }
            m_Mask.UpdateColor(c_kindColor[(int)m_currentKind] + offsetConnection);
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    private void DEAD()
    {
        ClearFights();
        if (m_Parent != null)
        {
            m_Parent.m_Children.Remove(this);
        }
        m_IsDead = true;
        m_PowerLabel.Visible = false;
        MushroomManager.manager.RemoveShroom(this);
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    private void CheckParentConnection()
    {
        if (m_Parent != null && m_ParentConnectionBroken == false)
        {
            if (CheckIfCanConnect(m_Parent) == false)
            {
                BreakParentConnection();
            }
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public float GetRadius()
    {
        return m_radius;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public float GetPower()
    {
        return m_radius * m_radius;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public float GetPrevisionalPower()
    {
        return m_radius * m_radius + m_transferRemaining;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    void BreakParentConnection()
    {
        m_ParentConnectionBroken = true;
        m_Line2D.Visible = false;

        if (TryToReconnectToNearbyShroom(true) == false)
        {
            m_LoosingAlledgance = c_AllegeanceTimer;
            if (MushroomManager.manager.GetCurrentMushroom() == this)
            {
                MushroomManager.manager.SetCurrentMushroom(null);
            }
        }

        for (int i = m_Children.Count - 1; i >= 0; i--)
        {
            Mushroom child = m_Children[i];
            child.BreakParentConnection();
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    bool TryToReconnectToNearbyShroom(bool mustBeSameType)
    {
        Mushroom newParent = MushroomManager.manager.GetNearbyValidMushroom(this, mustBeSameType);
        if (newParent != null)
        {
            SetParent(newParent);
            return true;
        }
        return false;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void SetCanGeneratePower(bool val)
    {
        m_generatePower = val;
        m_Particles.Visible = val;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public MushroomKind GetCurrentKind()
    {
        return m_currentKind;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public bool CheckIfCanConnect(Mushroom otherMushroom)
    {
        float DistSquared = (Position - otherMushroom.Position).LengthSquared();
        float connexionCapacity = m_radius + otherMushroom.m_radius;
        float connexionCapacitySquared = connexionCapacity * connexionCapacity;
        return DistSquared < connexionCapacitySquared;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public bool IsRoot()
    {
        return m_Parent == null && m_currentKind != MushroomKind.NEUTRAL;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void PresetParent(Mushroom shroom)
    {
        m_Parent = shroom;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void SetParent(Mushroom shroom)
    {
        if (m_Parent != null)
        {
            m_Parent.m_Children.Remove(this);
        }

        m_Parent = shroom;

        if (m_Parent != null)
        {
            m_Line2D.Visible = true;
            m_Line2D.SetPointPosition(0, Vector2.Zero);
            m_Line2D.SetPointPosition(1, shroom.Position - Position);
            m_ParentConnectionBroken = false;
            m_Parent.m_Children.Add(this);
            UpdateKind(m_Parent.m_currentKind);
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public bool ConnectedToRoot()
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
        foreach (Mushroom child in m_Children)
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
        for (int i = m_Children.Count - 1; i >= 0; i--)
        {
            Mushroom child = m_Children[i];
            child.CheckParentConnection();
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void CheckStartFight(Mushroom challenger)
    {
        if (m_challengers.Contains(challenger) == false)
        {
            Vector2 Dist = challenger.Position - Position;
            float radiusCombined = m_radius + challenger.m_radius;
            if (Dist.LengthSquared() < radiusCombined * radiusCombined)
            {
                m_challengers.Add(challenger);
                challenger.m_challengers.Add(this);
            }
        }
    }


    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void CheckEndFight()
    {
        for (int i = m_challengers.Count - 1; i >= 0; i--)
        {
            Mushroom challenger = m_challengers[i];
            Vector2 Dist = challenger.Position - Position;
            float radiusCombined = m_radius + challenger.m_radius;
            if (Dist.LengthSquared() > radiusCombined * radiusCombined)
            {
                m_challengers.Remove(challenger);
                challenger.m_challengers.Remove(this);
            }
        }
    }


    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void ClearFights()
    {
        for (int i = m_challengers.Count - 1; i >= 0; i--)
        {
            Mushroom challenger = m_challengers[i];
            m_challengers.Remove(challenger);
            challenger.m_challengers.Remove(this);
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public bool IsSameKind(Mushroom mushymushy)
    {
        return mushymushy.m_currentKind == m_currentKind;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public bool IsIndirectParentOf(Mushroom thisisridiculous)
    {
        if (thisisridiculous == this)
        {
            return true;
        }
        else if (thisisridiculous.m_Parent == null)
        {
            return false;
        }
        else
        {
            return IsIndirectParentOf(thisisridiculous.m_Parent);
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public bool IsParentOf(Mushroom mewmew)
    {
        return mewmew.m_Parent == this;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public bool WillLooseConnexionIfCreateOffspring()
    {
        List<Mushroom> toCheck = new List<Mushroom>(m_Children);
        if (m_Parent != null)
        {
            toCheck.Add(m_Parent);
        }

        float foreCastPower = Math.Min(GetPower(), GetPrevisionalPower());

        float newRadiusWouldBe = Mathf.Sqrt(foreCastPower / 2.0f);
        foreach (Mushroom shroom in toCheck)
        {
            float otherforeCastPower = Math.Min(shroom.GetPower(), shroom.GetPrevisionalPower());
            float otherforeCastRadius = (float)Math.Sqrt(otherforeCastPower);
            float DistSquared = (Position - shroom.Position).LengthSquared();
            float connexionCapacity = newRadiusWouldBe + otherforeCastRadius;
            if (DistSquared > (connexionCapacity * connexionCapacity))
            {
                return true;
            }
        }
        return false;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void GetLooseConnexionIfCreateOffspring(ref List<Mushroom> LostConnection)
    {
        List<Mushroom> toCheck = new List<Mushroom>(m_Children);
        if (m_Parent != null)
        {
            toCheck.Add(m_Parent);
        }

        float foreCastPower = Math.Min(GetPower(), GetPrevisionalPower());

        float newRadiusWouldBe = Mathf.Sqrt(foreCastPower / 2.0f);
        foreach (Mushroom shroom in toCheck)
        {
            if (shroom.m_ParentConnectionBroken == false)
            {
                float otherforeCastPower = Math.Min(shroom.GetPower(), shroom.GetPrevisionalPower());
                float otherforeCastRadius = (float)Math.Sqrt(otherforeCastPower);
                float DistSquared = (Position - shroom.Position).LengthSquared();
                float connexionCapacity = newRadiusWouldBe + otherforeCastRadius;
                if (DistSquared > (connexionCapacity * connexionCapacity))
                {
                    LostConnection.Add(shroom);
                }
            }
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public bool WillLooseConnexionIfTransfer()
    {
        List<Mushroom> toCheck = new List<Mushroom>(m_Children);
        if (m_Parent != null)
        {
            toCheck.Add(m_Parent);
        }

        float foreCastPower = Math.Min(GetPower(), GetPrevisionalPower());

        float newRadiusWouldBe = Mathf.Sqrt(foreCastPower - 100.0f);
        foreach (Mushroom shroom in toCheck)
        {
            float otherforeCastPower = Math.Min(shroom.GetPower(), shroom.GetPrevisionalPower());
            float otherforeCastRadius = (float)Math.Sqrt(otherforeCastPower);
            float DistSquared = (Position - shroom.Position).LengthSquared();
            float connexionCapacity = newRadiusWouldBe + otherforeCastRadius;
            if (DistSquared > (connexionCapacity * connexionCapacity))
            {
                return true;
            }
        }

        return false;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public float ComputeMaxTransferPossible()
    {
        float foreCastPower = Math.Min(GetPower(), GetPrevisionalPower());
        float foreCastRadius = (float)Math.Sqrt(foreCastPower);

        float maxTransfer = foreCastPower - 200.0f;

        List<Mushroom> toCheck = new List<Mushroom>(m_Children);
        if (m_Parent != null)
        {
            toCheck.Add(m_Parent);
        }
        
        foreach (Mushroom shroom in toCheck)
        {
            float otherforeCastPower = Math.Min(shroom.GetPower(), shroom.GetPrevisionalPower());
            float otherforeCastRadius = (float)Math.Sqrt(otherforeCastPower);
            float Dist = (Position - shroom.Position).Length();
            float currentConnexionCapacity = foreCastRadius + otherforeCastRadius;
            if (Dist > currentConnexionCapacity)
            {
                return 0.0f;
            }
            else
            {
                float targetRadius = Dist - otherforeCastRadius;
                float diff = foreCastPower - targetRadius * targetRadius;
                maxTransfer = Mathf.Min(maxTransfer, diff);
            }
        }

        return maxTransfer;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public bool TransferToOther(Mushroom otherMushroom, bool massTransfer = false)
    {
        List<Mushroom> path = new List<Mushroom>();
        if (otherMushroom.FindMushroomPath(this, ref path))
        {
            PackedScene shroomPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/MushroomPowerTransfer.tscn");

            MushroomPowerTransfer transfer = shroomPrefab.Instantiate<MushroomPowerTransfer>();
            transfer.m_Path = path;
            transfer.Position = Position;
            if (massTransfer)
            {

                transfer.m_PowerTransfered = ComputeMaxTransferPossible() * 0.95f;
                Transfer(-transfer.m_PowerTransfered);

                if (m_MassTransferAbsorbTimer > 0.0f)
                {
                    transfer.m_PowerTransfered += m_lastSentTransfer.m_PowerTransfered;
                    m_lastSentTransfer.QueueFree();
                    m_SoundPlayer.PlayBouip();
                }
            }
            else
            {
                transfer.m_PowerTransfered = 100.0f;
                Transfer(-100.0f);
            }
            AddSibling(transfer);
            m_MassTransferAbsorbTimer = 0.5f;
            m_lastSentTransfer = transfer;
            return true;
        }
        return false;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void Transfer(float power)
    {
        m_transferRemaining += power;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void EndTransfer(float power)
    {
        Transfer(power);
        if (power > 100)
        {
            m_SoundPlayer.PlayPiuob();
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public bool IsOpponentKind(Mushroom vioooom)
    {
        return vioooom.m_currentKind != m_currentKind && m_currentKind != MushroomKind.NEUTRAL && vioooom.m_currentKind != MushroomKind.NEUTRAL;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void SetSelected(bool val)
    {
        m_Selected = val;
        m_SelectedSprite.Visible = val;
        m_ShowPowerCooldown = 4.0f;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void UpdateRadius(float newRadius, bool Check)
    {
        m_radius = newRadius;

        if (m_Mask != null) // set of members are done before the _ready.so we need to test the mask validity
        {
            m_Mask.UpdateRadius(m_radius * m_SpawningLerp);
            m_Sprite.Scale = Vector2.One / 80.0f * (m_radius + 50.0f) * m_SpawningLerp;
        }

        if (Engine.IsEditorHint() == false)
        {
            if (Check)
            {
                CheckEndFight();
                CheckParentConnection();
                CheckChildrenConnection();
            }
            m_ShowPowerCooldown = 4.0f;

            MushroomManager.manager.CheckNearbyOpposingMushroom(this);
        }
    }



    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public bool FindMushroomPath(Mushroom target, ref List<Mushroom> result)
    {
        return _RecFindMushroomPath(this, target, result);
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    private bool _RecFindMushroomPath(Mushroom previous, Mushroom target, List<Mushroom> result)
    {
        if (this == target)
        {
            result.Add(this);
            return true;
        }

        foreach (Mushroom child in m_Children)
        {
            if (previous != child)
            {
                if (child._RecFindMushroomPath(this, target, result))
                {
                    result.Add(this);
                    return true;
                }
            }
        }

        if (m_Parent != null && previous != m_Parent)
        {
            if (m_Parent._RecFindMushroomPath(this, target, result))
            {
                result.Add(this);
                return true;
            }
        }

        return false;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void UpdateKind(MushroomKind kind)
    {
        m_currentKind = kind;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void InputEvent(Node viewport, InputEvent generatedEvent, int shapeIdx)
    {
        m_ShowPowerCooldown = 4.0f;

        InputEventMouse mouseEvent = (InputEventMouse)generatedEvent;
        int buttonLeftPressed = ((int)mouseEvent.ButtonMask) & ((int)MouseButton.Left);

        if (buttonLeftPressed != 0 && MushroomManager.manager.cooldown <= 0.0f)
        {
            if (MushroomManager.manager.HasCurrentMushroom() == false
            && m_ParentConnectionBroken == false
            && m_currentKind == MushroomKind.GOOD
            )
            {
                MushroomManager.manager.SetCurrentMushroom(this);
                MushroomManager.manager.cooldown = 0.2f;
            }
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public bool CanSpawnAnOffspring()
    {
        return m_deployedSeed == null && ConnectedToRoot();
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public bool SpawnAnOffspring(Vector2 pos)
    {
        if (m_deployedSeed == null && (pos - GlobalPosition).LengthSquared() > 400.0f)
        {
            float newPower = Mathf.Min(GetPrevisionalPower(),GetPower()) / 2.0f;
            float newRadius = Mathf.Sqrt(newPower);
            if (m_radius * m_radius > (pos - GlobalPosition).LengthSquared())
            {
                PackedScene shroomPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/MushroomSeed.tscn");

                MushroomSeed newSeed = shroomPrefab.Instantiate<MushroomSeed>();
                newSeed.GlobalPosition = Position;
                newSeed.m_targetPos = pos;
                newSeed.m_CarriedRadius = newRadius;
                newSeed.m_parent = this;
                AddSibling(newSeed);
                m_deployedSeed = newSeed;
                m_lostPower = 0.0f;
                m_powerLossFromSeedLerp = newPower;
                return true;
            }
        }
        return false;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void SeedEndDeployment(bool result)
    {
        if (result)
        {
            UpdateRadius((float)Mathf.Sqrt((double)(m_radius * m_radius - m_powerLossFromSeedLerp + m_lostPower)), true);
        }
        else
        {
            UpdateRadius((float)Mathf.Sqrt((double)(m_radius * m_radius + m_lostPower)), true);
        }

        m_deployedSeed = null;
    }

    /*
    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _Draw()
    {
        

        if (m_Selected)
        {
            DrawArc(Vector2.Zero, m_radius - 1.5f, 0.0f, 360.0f, 360, new Color(1,0,0,1), 1.0f);
        }

        float mouseDist = GetGlobalMousePosition().DistanceTo(Position);
        if (mouseDist < MushroomManager.c_TooCloseToSprout + 10.0f);
        {
            float alpha = Mathf.Min(1.0f, (MushroomManager.c_TooCloseToSprout + 10.0f - mouseDist) / 10.0f);
            DrawArc(Vector2.Zero, MushroomManager.c_TooCloseToSprout, 0.0f, 360.0f, 60, new Color(1,0,0,alpha), 1.0f);
        }
    }*/
}
