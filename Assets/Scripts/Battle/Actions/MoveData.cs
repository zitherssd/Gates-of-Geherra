using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Move", menuName = "Combat/Move Data")]
public class MoveData : ScriptableObject
{
    [Header("Identity")]
    public string moveName;
    public AnimationClip clip;

    [Header("Logic (Frame Data @ 60 FPS)")]
    public int startupFrames = 10;
    public int activeFrames = 4;   // Ignored if useNaturalActiveSpeed is true
    public int recoveryFrames = 20;

    [Header("Visual Sync (Normalized 0.0 to 1.0)")]
    [Tooltip("Point in the clip where the Hitbox should turn ON")]
    [Range(0f, 1f)] public float visualActiveStart = 0.3f; 
    
    [Tooltip("Point in the clip where the Hitbox should turn OFF")]
    [Range(0f, 1f)] public float visualActiveEnd = 0.4f;

    [Header("Scaling Mode")]
    [Tooltip("If TRUE: Plays active phase at 1.0x speed (Good for multi-hit/breath). If FALSE: Stretches active phase to fit activeFrames (Good for snappy jabs).")]
    public bool useNaturalActiveSpeed = false;

    [Header("Stats")]
    public float damage = 10f;
    public float staminaCost = 10f;
    public float momentumGain = 10f; // + for attacker, - for defender

    [Header("Special Flags")]
    public bool isPerilous; // Triggers "Spider-Sense" warning
    public bool isDefensive; // Is this a Block/Dodge?
}
