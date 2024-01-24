using Godot;
using System;
using System.Collections.Generic;

public partial class MushroomSpriteDatabase : Node2D
{
    // STATIC PART ----------------------------
    static public MushroomSpriteDatabase database;

    [Export] public MushroomSpriteData[] m_spriteData;

    public override void _EnterTree()
    {
        base._EnterTree();
        database = this;
    }

    public MushroomSpriteData PickARandomSprite()
    {
        MushroomSpriteData result = null;
        float totalWeight = 0.0f;
        foreach(MushroomSpriteData data in m_spriteData)
        {
            totalWeight += data.weight;
        }

        float selectedWeight = RandomManager.GetFloatRange(0.0f, totalWeight);

        foreach(MushroomSpriteData data in m_spriteData)
        {
            result = data;
            selectedWeight -= data.weight;
            if (selectedWeight <= 0.0f)
            {
                break;
            }
        }

        return result;
    }
}