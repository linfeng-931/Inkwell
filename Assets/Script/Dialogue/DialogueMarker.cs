using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class DialogueMarker : Marker, INotification
{
    public string dialogueId;
    public PropertyName id { get; } = new PropertyName();
}
