using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Message {
    public GameObject messageObject;
    public int messageTime; // time since message was created. divide by 50 to get num of seconds
    public int messageTurnTime; // record which turn the message was created in

    public Message(int messageTurnTime, GameObject messageObject) {
        this.messageTime = 0;
        this.messageTurnTime = messageTurnTime;
        this.messageObject = messageObject;
    }
}
