using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageStats", menuName = "Scripts/ScriptableObjects/Stage/StageStats")]
public class StageStats : ScriptableObject
{
    [Header("Visuals")]
    [SerializeField] public Sprite BackgroundImage;
    public float BackgroundImageScale = 12.99f;

    [Header("Gameplay")]
    [SerializeField] public Vector2 MaxMoveBounds = new Vector2(10.0f, 10.0f);
    [SerializeField] public Vector2 MaxPlayerSpacingBounds = new Vector2(10.0f, 10.0f);


}
