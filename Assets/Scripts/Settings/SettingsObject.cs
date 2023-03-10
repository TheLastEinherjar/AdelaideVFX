using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AdelaideVFX;

[CreateAssetMenu(fileName = "NewSettingsProfile", menuName = "AdelaideVFX/New Settings Profile")]

public class SettingsObject : ScriptableObject
{
    public Dictionary<byte, ChannelSettings> channels = new Dictionary<byte, ChannelSettings>();
    //both
    public float QuarterNoteHight;
    public float baseQuarterNoteHight = 4;
    public bool FadeToKeyColor;
    public float startDistanceFromKey;
    public Color[] EffectsColors;
}
