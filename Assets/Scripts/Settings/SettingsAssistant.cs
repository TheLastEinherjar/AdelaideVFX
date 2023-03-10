using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdelaideVFX {
    public class SettingsAssistant
    {

    }

    public class ChannelSettings {
        public string channelName;

        public GameObject whiteNotePrefab;
        public NoteAndKeyColors whiteNotes;

        public GameObject blackNotePrefab;
        public NoteAndKeyColors blackNotes;
    }

    [System.Serializable]
    public class NoteAndKeyColors {
        public VFXColors[] NoteColors;
        public VFXColors[] KeyColors;
        public VFXColors[] EffectsColor;
        public bool noteChangesToNewColor;
    }

    [System.Serializable]
    public class VFXColors {
        public string ColorUseName;
        public Color SaidColor;
    }
}