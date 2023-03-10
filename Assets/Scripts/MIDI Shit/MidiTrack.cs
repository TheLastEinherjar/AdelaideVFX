using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AdelaideVFX;
namespace AdelaideVFX {
    public class MidiTrack
    {
        public string trackName;
        public uint NotesPlayed;
        public ulong TotalTime;
        public byte[] Programs;

        public MidiEvent[] MidiEvents;

        public bool ContainsProgram(byte program)
        {
            for (int x = 0; x < Programs.Length; x++)
            {
                if (Programs[x] == program)
                    return true;
            }
            return false;
        }

        /*
        public MidiEventData GetNoteData(MidiEvent _noteStartEvent) {
            for (int i = 0; i < MidiNotes.Length; i++) {
                if (MidiNotes[i]. && MidiNotes[i].channel == _noteStartEvent.channel && MidiNotes[i].NoteID == (byte)_noteStartEvent.Parameters[1]) {

                }
            }
        }
        */
    }
}
