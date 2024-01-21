using Godot;
using System;

public partial class GameManager : Node2D
{
    static public GameManager manager = null;

    Node2D m_currentSubscene;

    public override void _Ready()
    {
        manager = this;
        base._Ready();
        GoToMenu();
    }

    public void ChangeScene(PackedScene scene)
    {
        if (m_currentSubscene != null)
        {
            RemoveChild(m_currentSubscene);
        }

        m_currentSubscene = scene.Instantiate<Node2D>();

        if (m_currentSubscene != null)
        {
            AddChild(m_currentSubscene);
        }
    }

    public void GoToMenu()
    {
        ChangeScene(ResourceLoader.Load<PackedScene>("res://Scenes/Menu.tscn"));
    }

    public void GoToGame()
    {
        ChangeScene(ResourceLoader.Load<PackedScene>("res://Scenes/Game.tscn"));
    }

    public void Quit()
    {
        GetTree().Quit();
    }

}
