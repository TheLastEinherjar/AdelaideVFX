using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AdelaideVFX;

public class EventManager : MonoBehaviour
{

    public static EventManager Instance;
    public static Action<float> updateBPMEvent;
    public static Action<MidiEvent> keyEvent;
    //in keyevent the int is note id
    public string filePath;
    private int TicksPerQuarter;
    private float currentBPM = 120;
    private int currentEventIndex;
    public float tmpTimeFromStart = 0;
    public List<MidiTrack> tracksToPlay = new List<MidiTrack>();
    public MidiExtractor extractor = new MidiExtractor();

    public float CurrentBPM {
        get {return currentBPM;}
    }



    private void Awake() {
        Instance = this;
    }

    private void Start() {
        LoadNotes();
    }

    private void Update() {
        tmpTimeFromStart += (currentBPM/60) * TicksPerQuarter * Time.deltaTime;
        PlayNotes();
    }

    public void PlayNotes(){
        foreach (MidiTrack track in extractor.AllTracks){
            List<MidiEvent> processing = new List<MidiEvent>();
            for (int i = currentEventIndex; i < track.MidiEvents.Length; i++) {
                if (track.MidiEvents[i].TimeFromStart > tmpTimeFromStart) {
                    currentEventIndex = i;
                    break;
                } else {
                    if (track.MidiEvents[i].typeOfEvent == MidiEventType.startNote) {
                        //we multiply length by the current speed
                        track.MidiEvents[i].Parameters[1] = (float)track.MidiEvents[i].Parameters[1] * SettingsManager.Instance.GetNoteHight;
                        //and send off the event
                        keyEvent(track.MidiEvents[i]);
                    } else if (track.MidiEvents[i].typeOfEvent == MidiEventType.tempo) {
                        currentBPM = 60000000/(UInt32)track.MidiEvents[i].Parameters[0];
                        Debug.Log((UInt32)track.MidiEvents[i].Parameters[0]/TicksPerQuarter);
                        SettingsManager.Instance.UpdateNoteSpeed((currentBPM/60) * SettingsManager.Instance.GetNoteHight);
                    }
                }
            }
        }
    }
    //SettingsManager.Instance.GetNoteSpeed returns 0 because it has not been set yet
    //so load lenght while playing I think that will work but am to sleepy to really know 
    public void LoadNotes(){
        extractor.ExtractFile(filePath);
        TicksPerQuarter = extractor.TicksPerQuarter;
        foreach (MidiTrack t in extractor.AllTracks){
            //makes a list of the index in t.MidiEvents
            List<int>_notesBeingProcessed = new List<int>();
            //this is the time from start of each event
            UInt32 tmpTimeFromStart = 0;
            //can you read this code... NO!!
            //Does it work... Lets hope so!!
            for (int e = 0; e < t.MidiEvents.Length; e++) {
                //set current time
                tmpTimeFromStart += t.MidiEvents[e].timeFromLastEvent;
                //set time from start to current time
                t.MidiEvents[e].TimeFromStart = tmpTimeFromStart;
                //if it is a start note
                
                if (t.MidiEvents[e].typeOfEvent == MidiEventType.startNote) {
                    //save the index of this event
                    _notesBeingProcessed.Add(e);
                    //set the time from the songs start
                    t.MidiEvents[e].TimeFromStart = tmpTimeFromStart;
                    //else if its a end note
                } else if (t.MidiEvents[e].typeOfEvent == MidiEventType.endNote) {
                    //for each index in notesbeingprocessed
                    for (int n = 0; n < _notesBeingProcessed.Count; n++) {
                        //if this is the coresponding start note
                        if (t.MidiEvents[_notesBeingProcessed[n]].typeOfEvent == MidiEventType.startNote && (byte)t.MidiEvents[e].Parameters[0] == (byte)t.MidiEvents[_notesBeingProcessed[n]].Parameters[0] && t.MidiEvents[_notesBeingProcessed[n]].channel == t.MidiEvents[e].channel) {
                            //then set Parameters 1 to = the note length
                            //Debug.Log(((UInt32)tmpTimeFromStart) - (UInt32)t.MidiEvents[_notesBeingProcessed[n]].TimeFromStart);
                            t.MidiEvents[_notesBeingProcessed[n]].Parameters[1] = ((float)tmpTimeFromStart - (float)t.MidiEvents[_notesBeingProcessed[n]].TimeFromStart)/TicksPerQuarter;
                            //and remove this value from _notesBeingProcessed
                            _notesBeingProcessed.RemoveAt(n);
                        }
                    }
                    
                }
                
                //add more stuff here
            }
        }
    }
}
