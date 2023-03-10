using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AdelaideVFX;

namespace AdelaideVFX {
    public class MidiEvent
    {
        public MidiEventType typeOfEvent;
        public byte channel;
        public uint timeFromLastEvent = 0;
        public uint TimeFromStart = 0;
        public float Velocity = 0;

        public object[] Parameters;

        public MidiEvent()
        {
            this.Parameters = new object[5];
            this.typeOfEvent = MidiEventType.someOtherThingIDontCareAbout;
        }
    }
}
