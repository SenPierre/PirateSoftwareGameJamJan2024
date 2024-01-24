using Godot;
using System;

public partial class SceneButton : Node2D
{
    [Export] public Texture2D m_image;
    [Export] public PackedScene m_level;

    public override void _Ready()
    {
        base._Ready();
        TextureRect texture = FindChild("TextureRect", true) as TextureRect;
        texture.Texture = m_image;
        texture.Size = new Vector2(160, 90);
    }

    void OnClick()
    {
        GameManager.manager.GoToGame(m_level);
    }
}
