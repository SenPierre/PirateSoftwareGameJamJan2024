using Godot;

[GlobalClass]
public partial class MushroomSpriteData : Resource
{
    [Export] public Texture2D m_Sprite;
    [Export] public Texture2D m_SpriteSelect;
    [Export] public float weight = 1;
}