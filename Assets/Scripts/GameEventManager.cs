using UnityEngine;
using IntrovertStudios.Messaging;

public class GameEventManager : MonoBehaviour
{
    public enum Message
    {
        AllKingsKilled
    }

    public IMessageHub<Message> hub;

    void Awake()
    {
        hub = new MessageHub<Message>();
    }

    void Start()
    {
        hub.Connect(Message.AllKingsKilled, OnAllKingsKilled);
    }

    void OnAllKingsKilled()
    {

    }
}