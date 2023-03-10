using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using AdelaideVFX;

namespace AdelaideVFX {

    public enum TimingType{
        TicksPerBeat,
        FamesPerSecond
    }

    public enum MidiEventType{
    startNote,
    endNote,
    sustainPedal,
    endOfTrack,
    timeSignature,
    keySignature,
    trackName,
    instrumentName,
    lyrics,
    midiChannelPrefix,
    tempo,
    smpteOffset,
    controller,
    programChange,
    someOtherThingIDontCareAbout
    //for now
    }


    public class MidiExtractor {
        //assuming i don't go on a tangent this will get the data from a file
        private int midiFileFormat;
        private TimingType timeFormat;
        private int ticksPerQuarter;
        private MidiTrack[] allTracks;

        public MidiTrack[] AllTracks {
            get { return allTracks; }
        }

        public int TicksPerQuarter {
            get {return ticksPerQuarter;}
        }

        public void ExtractFile(string fileToExtract){
            Stream midiStream = new MemoryStream();
            try
            {
                //get data from the file
                midiStream = File.Open(fileToExtract, FileMode.Open);
                //extracts data from the file and stores it in this class
                ExtractFromStream(midiStream);
            }
            catch (Exception ex)
            {
                throw new Exception("Midi Failed to Load!", ex);
            }
            finally
            {
                if (midiStream != null) 
                {
                    midiStream.Dispose();
                }
            }
                
        }

        public MidiTrack[] getTracks(){
            return allTracks;
        }


        private bool ExtractFromStream(Stream _streamToExtract){
            byte[] tmp = new byte[4];
            _streamToExtract.Read(tmp, 0, 4);
            if (UTF8Encoding.UTF8.GetString(tmp, 0, tmp.Length) != "MThd") {
                return false;
                throw new Exception("Not a valid midi file!");
            }
            //Read header length
            _streamToExtract.Read(tmp, 0, 4);
            Array.Reverse(tmp); //Reverse the bytes
            int headerLength = BitConverter.ToInt32(tmp, 0);
            //Read midi format
            tmp = new byte[2];
            _streamToExtract.Read(tmp, 0, 2);
            Array.Reverse(tmp); //Reverse the bytes
            midiFileFormat = BitConverter.ToInt16(tmp, 0);
            //Read Track Count
            _streamToExtract.Read(tmp, 0, 2);
            Array.Reverse(tmp); //Reverse the bytes
            int trackCount = BitConverter.ToInt16(tmp, 0);
            allTracks = new MidiTrack[trackCount];
            //Read Delta Time
            _streamToExtract.Read(tmp, 0, 2);
            Array.Reverse(tmp); //Reverse the bytes
            int delta = BitConverter.ToInt16(tmp, 0);
            ticksPerQuarter = (delta & 0x7FFF);
            //Time Format
            timeFormat = ((delta & 0x8000) > 0) ? TimingType.FamesPerSecond : TimingType.TicksPerBeat;

            //For each track
            for (int x = 0; x < trackCount; x++)
            {
                List<byte> Programs = new List<byte>();
                List<MidiEvent> midiEventList = new List<MidiEvent>();
                allTracks[x] = new MidiTrack();
                Programs.Add(0); //assume the track uses program at 0 in case no program changes are used
                tmp = new byte[4];      //reset the size again
                _streamToExtract.Read(tmp, 0, 4);
                if (UTF8Encoding.UTF8.GetString(tmp, 0, tmp.Length) != "MTrk")
                    throw new Exception("Invalid track!");
                _streamToExtract.Read(tmp, 0, 4);
                Array.Reverse(tmp); //Reverse the bytes
                int TrackLength = BitConverter.ToInt32(tmp, 0);
                //Read The Rest of The Track
                tmp = new byte[TrackLength];
                _streamToExtract.Read(tmp, 0, TrackLength);
                int index = 0;
                byte prevByte = 0;
                int prevChan = 0;
                while (index < tmp.Length)
                {
                    UInt16 numofbytes = 0;
                    UInt32 ScrmbledDta = BitConverter.ToUInt32(tmp, index);
                    MidiEvent thisEvent = new MidiEvent();
                    thisEvent.timeFromLastEvent = GetTime(ScrmbledDta, ref numofbytes);
                    index += 4 - (4 - numofbytes);
                    byte statusByte = tmp[index];
                    int CHANNEL = GetChannel(statusByte);
                    if (statusByte < 0x80)
                    {
                        statusByte = prevByte;
                        CHANNEL = prevChan;
                        index--;
                    }
                    if (statusByte != 0xFF)
                        statusByte &= 0xF0;
                    prevByte = statusByte;
                    prevChan = CHANNEL;
                    switch (statusByte)
                    {
                        case 0x80:
                            {
                                //ends a note
                                thisEvent.typeOfEvent = MidiEventType.endNote;
                                ++index;
                                thisEvent.channel = (byte)CHANNEL;
                                thisEvent.Parameters[0] = tmp[index++]; // note Id
                                thisEvent.Velocity = tmp[index++];

                            }
                            break;
                        case 0x90:
                            {
                                //this starts a note
                                thisEvent.typeOfEvent = MidiEventType.startNote;
                                ++index;
                                thisEvent.channel = (byte)CHANNEL;
                                thisEvent.Parameters[0] = tmp[index++];//note id
                                thisEvent.Velocity = tmp[index++];
                                if (thisEvent.Velocity == 0x00) //Setting velocity to 0 is actually just tuthisEvent.Parameters[0] off.
                                    thisEvent.typeOfEvent = MidiEventType.endNote;
                                allTracks[x].NotesPlayed++;
                            }
                            break;
                        case 0xA0:
                            {
                                //this is aftertouch
                                //I dont plan on using it
                                //it allows you to modify the sound of a note after it has been started
                                thisEvent.typeOfEvent = MidiEventType.someOtherThingIDontCareAbout;
                                thisEvent.channel = (byte)CHANNEL;
                                ++index;
                                thisEvent.Parameters[0] = tmp[++index];
                                thisEvent.Velocity = tmp[++index];//Amount
                            }
                            break;
                        case 0xB0:
                            {
                                //Controller event
                                thisEvent.typeOfEvent = MidiEventType.controller;
                                thisEvent.channel = (byte)CHANNEL;
                                thisEvent.Parameters[0] = thisEvent.channel;
                                ++index;
                                thisEvent.Parameters[1] = tmp[index++]; //type
                                thisEvent.Parameters[2] = tmp[index++]; //value
                            }
                            break;
                        case 0xC0:
                            {
                                //this is a program change
                                thisEvent.typeOfEvent = MidiEventType.programChange;
                                thisEvent.channel = (byte)CHANNEL;
                                thisEvent.Parameters[0] = thisEvent.channel;
                                ++index;
                                thisEvent.Parameters[0] = tmp[index++];
                                thisEvent.Parameters[1] = thisEvent.Parameters[0];
                                //record which programs are used by the track
                                if (thisEvent.channel != 9)
                                {
                                    if (Programs.Contains((byte)thisEvent.Parameters[0]) == false)
                                        Programs.Add((byte)thisEvent.Parameters[0]);
                                }
                            }
                            break;
                        case 0xD0:
                            {
                                //Channel after touch I dont what it and dont need it
                                thisEvent.typeOfEvent = MidiEventType.someOtherThingIDontCareAbout;
                                thisEvent.channel = (byte)CHANNEL;
                                thisEvent.Parameters[0] = thisEvent.channel;
                                ++index;
                                //Amount
                                thisEvent.Parameters[0] = tmp[++index];
                            }
                            break;
                        case 0xE0:
                            {
                                //pitch bend dont want it for now
                                thisEvent.typeOfEvent = MidiEventType.someOtherThingIDontCareAbout;
                                thisEvent.channel = (byte)CHANNEL;
                                thisEvent.Parameters[0] = thisEvent.channel;
                                ++index;
                                thisEvent.Parameters[0] = tmp[++index];
                                thisEvent.Velocity = tmp[++index];
                                ushort s = (ushort)thisEvent.Parameters[0];
                                s <<= 7;
                                s |= (ushort)thisEvent.Velocity;
                                thisEvent.Parameters[1] = ((double)s - 8192.0) / 8192.0;
                            }
                            break;
                        case 0xFF:
                            statusByte = tmp[++index];
                            switch (statusByte)
                            {
                                case 0x00:
                                    //this is the sequence number... I dont know what it does or if I needed it
                                    //so until I do its junk
                                    thisEvent.typeOfEvent = MidiEventType.someOtherThingIDontCareAbout; ++index;
                                    break;
                                case 0x01:
                                    //I am not totally sure what a text event is but if it is what I think it could possibly be I dont really think I need it?
                                    thisEvent.typeOfEvent = MidiEventType.someOtherThingIDontCareAbout; ++index;
                                    //Get the length of the string
                                    thisEvent.Parameters[0] = tmp[index++];
                                    //Set the string in the parameter list
                                    thisEvent.Parameters[1] = UTF8Encoding.UTF8.GetString(tmp, index, ((int)tmp[index - 1])); index += (int)tmp[index - 1];
                                    break;
                                case 0x02:
                                    //who cares about copyright... right?
                                    thisEvent.typeOfEvent = MidiEventType.someOtherThingIDontCareAbout; ++index;
                                    //Get the length of the string
                                    thisEvent.Parameters[0] = tmp[index++];
                                    //Set the string in the parameter list
                                    thisEvent.Parameters[1] = UTF8Encoding.UTF8.GetString(tmp, index, ((int)tmp[index - 1])); index += (int)tmp[index - 1];
                                    break;
                                case 0x03:
                                    //I guess this is the track name
                                    thisEvent.typeOfEvent = MidiEventType.trackName; ++index;
                                    //Get the length of the string
                                    thisEvent.Parameters[0] = tmp[index++];
                                    //Set the string in the parameter list
                                    allTracks[x].trackName = UTF8Encoding.UTF8.GetString(tmp, index, ((int)tmp[index - 1])); index += (int)tmp[index - 1];
                                    break;
                                case 0x04:
                                    //name of the instrument you could change color etc with this
                                    thisEvent.typeOfEvent = MidiEventType.instrumentName; ++index;
                                    //Set the instrument name
                                    thisEvent.Parameters[0] = UTF8Encoding.UTF8.GetString(tmp, index + 1, (int)tmp[index]);
                                    index += (int)tmp[index] + 1;
                                    break;
                                case 0x05:
                                    //these are lyric they will do stuff... maybe
                                    thisEvent.typeOfEvent = MidiEventType.lyrics; ++index;
                                    //Set the lyric string
                                    thisEvent.Parameters[0] = UTF8Encoding.UTF8.GetString(tmp, index + 1, (int)tmp[index]);
                                    index += (int)tmp[index] + 1;
                                    break;
                                case 0x06:
                                    //don't care
                                    thisEvent.typeOfEvent = MidiEventType.someOtherThingIDontCareAbout; ++index;
                                    //Set the marker
                                    thisEvent.Parameters[0] = UTF8Encoding.UTF8.GetString(tmp, index + 1, (int)tmp[index]);
                                    index += (int)tmp[index] + 1;
                                    break;
                                case 0x07:
                                    //don't know what I would do with these
                                    thisEvent.typeOfEvent = MidiEventType.someOtherThingIDontCareAbout; ++index;
                                    //Set the cue point
                                    thisEvent.Parameters[0] = UTF8Encoding.UTF8.GetString(tmp, index + 1, (int)tmp[index]);
                                    index += (int)tmp[index] + 1;
                                    break;
                                case 0x20:
                                    //who knows but it sounds important
                                    thisEvent.typeOfEvent = MidiEventType.midiChannelPrefix; index++;
                                    //Get the length of the data
                                    thisEvent.Parameters[0] = tmp[index++];
                                    //Set the string in the parameter list
                                    thisEvent.Parameters[1] = tmp[index++];
                                    break;
                                case 0x2F:
                                    //duh
                                    thisEvent.typeOfEvent = MidiEventType.endOfTrack;
                                    index += 2;
                                    break;
                                case 0x51:
                                    //this is a tempo event
                                    thisEvent.typeOfEvent = MidiEventType.tempo; ++index;
                                    //Get the length of the data
                                    thisEvent.Parameters[4] = tmp[index++];
                                    //Put the data into an array
                                    byte[] mS = new byte[4]; for (int i = 0; i < 3; i++) mS[i + 1] = tmp[i + index]; index += 3;
                                    //Put it into a readable format
                                    byte[] mS2 = new byte[4]; for (int i = 0; i < 4; i++) mS2[3 - i] = mS[i];
                                    //Get the value from the array
                                    UInt32 Val = BitConverter.ToUInt32(mS2, 0);
                                    //Set the value
                                    thisEvent.Parameters[0] = Val;
                                    break;
                                case 0x54:
                                    //I think this one is a start delay
                                    thisEvent.typeOfEvent = MidiEventType.smpteOffset; ++index;
                                    int v = tmp[index++];
                                    if (v >= 4) {
                                        for (int i = 0; i < 4; i++) {
                                            thisEvent.Parameters[i] = tmp[index++];
                                        }
                                    }
                                    else {
                                        for (int i = 0; i < v; i++) {
                                            thisEvent.Parameters[i] = tmp[index++];
                                        }
                                    }

                                    for (int i = 4; i < v; i++) {
                                        index++;
                                    }
                                    break;
                                case 0x58:
                                    //self explanatory
                                    thisEvent.typeOfEvent = MidiEventType.timeSignature; ++index;
                                    int v1 = tmp[index++];
                                    if (v1 >= 4) {
                                        for (int i = 0; i < 4; i++) { 
                                            thisEvent.Parameters[i] = tmp[index++];
                                        }
                                    }
                                    else {
                                        for (int i = 0; i < v1; i++) {
                                            thisEvent.Parameters[i] = tmp[index++];
                                        }
                                    }

                                    for (int i = 4; i < v1; i++) {
                                        index++;
                                    }
                                    break;
                                case 0x59:
                                    //also self explanatory
                                    thisEvent.typeOfEvent = MidiEventType.keySignature; ++index;
                                    int v2 = tmp[index++];
                                    if (v2 >= 4) {
                                        for (int i = 0; i < 4; i++) {
                                            thisEvent.Parameters[i] = tmp[index++];
                                        }
                                    }
                                    else {
                                        for (int i = 0; i < v2; i++) {
                                            thisEvent.Parameters[i] = tmp[index++];
                                        }
                                    }
                                    for (int i = 4; i < v2; i++) {
                                        index++;
                                    }
                                    break;
                                case 0x7F:
                                    //Sequencer specific events
                                    //I don't know and I don't care
                                    thisEvent.typeOfEvent = MidiEventType.someOtherThingIDontCareAbout; ++index;    //increment the indexer
                                    //Get the length of the data
                                    thisEvent.Parameters[4] = tmp[index++];
                                    //Get the byte length
                                    byte[] len = new byte[(byte)thisEvent.Parameters[4]];
                                    //get the byte info
                                    for (int i = 0; i < len.Length; i++) len[i] = tmp[index++];
                                    thisEvent.Parameters[0] = len;
                                    break;
                            }
                            break;
                        //System exclusive
                        case 0xF0:
                            while (tmp[index] != 0xF7)
                                index++;
                            index++;
                            break;
                    }
                    midiEventList.Add(thisEvent);
                    allTracks[x].TotalTime = allTracks[x].TotalTime + thisEvent.timeFromLastEvent;
                }
                allTracks[x].Programs = Programs.ToArray();
                allTracks[x].MidiEvents = midiEventList.ToArray();
            }
            return true;
        }

        private int GetChannel(byte statusbyte)
        {
            statusbyte = (byte)(statusbyte << 4);
            return statusbyte >> 4;
        }

        private uint GetTime(UInt32 data, ref UInt16 numOfBytes)
        {
            byte[] buff = BitConverter.GetBytes(data); numOfBytes++;
            for (int i = 0; i < buff.Length; i++) { if ((buff[i] & 0x80) > 0) { numOfBytes++; } else { break; } }
            for (int i = numOfBytes; i < 4; i++) buff[i] = 0x00;
            Array.Reverse(buff);
            data = BitConverter.ToUInt32(buff, 0);
            data >>= (32 - (numOfBytes * 8));
            UInt32 b = data;
            UInt32 bffr = (data & 0x7F);
            int c = 1;
            while ((data >>= 8) > 0)
            {
                bffr |= ((data & 0x7F) << (7 * c)); c++;
            }
            return bffr;
        }
    }
}