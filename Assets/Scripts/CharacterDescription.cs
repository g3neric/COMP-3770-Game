using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterDescription : MonoBehaviour
{
    // grabbing itself as a text object
    public TextMeshProUGUI self;
    public TMP_Dropdown cClass;
    public int thing;

    // Start is called before the first frame update
    void Start()
    {
        self.text = ("This is a Grunt");
    }

    // Update is called once per frame
    void Update()
    {
        switch(cClass.value){
            //checks dropdown menu value so it can be accurate
            case 0:
                // Grunt();
                self.text = ("Grunt:\nAP: 7 \nHP: 40 \nHeal Rate: 3 \nView Range: 10 \nLuck Multiplier: x1 \nStarting Items: 1x Assault Rifle + 1x Pistol");
                break;
            case 1:
                // Engineer();
                self.text = ("Engineer:\nAP: 7 \nHP: 45 \nHeal Rate: 3 \nView Range: 10 \nLuck Multiplier: x1 \nStarting Item: 1x Pistol");
                break;
            case 2:
                // Joker();
                self.text = ("Joker:\nAP: 7 \nHP: 35 \nHeal Rate: 3 \nView Range: 10 \nLuck Multiplier: x3 \nStarting Item: 1x Assault Rifle");
                break;
            case 3:
                // Saboteur();
                self.text = ("Saboteur:\nAP: 7 \nHP: 45 \nHeal Rate: 3 \nView Range: 10 \nLuck Multiplier: x1 \nStarting Item: 1x Pistol");	
                break;
            case 4:
                // Scout();
                self.text = ("Scout:\nAP: 9 \nHP: 45 \nHeal Rate: 3 \nView Range: 10 \nLuck Multiplier: x1 \nStarting Item: 1x Pistol");	
                break;
            case 5:
                // Sharpshooter();
                self.text = ("Sharpshooter:\nAP: 6 \nHP: 30 \nHeal Rate: 3 \nView Range: 10 \nLuck Multiplier: x1 \nStarting Items: 1x Sniper Rifle + 1x Pistol");	
                break;
            case 6:
                // Surgeon();
                self.text = ("Surgeon:\nAP: 6 \nHP: 70 \nHeal Rate: 7 \nView Range: 10 \nLuck Multiplier: x1 \nStarting Items: 1x Pistol");	
                break;
            case 7:
                // Tank();
                self.text = ("Grunt:\nAP: 7 \nHP: 80 \nHeal Rate:2 \nView Range: 10 \nLuck Multiplier: x1 \nStarting Items: 1x Assault Rifle + 1x Pistol");	
                break;
            default:
                print("error selecting class");
                return;

            }
    }
}
