using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameControler : MonoBehaviour
{
    [Header("References")]
    public PlayerCont PlayerCont;
    public Text stateCheck;
    public Text speed;

    string stateText;
    string speedText;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        setStateText();   
    }
    public void setStateText()
    {
        stateText = PlayerCont.state.ToString();
        stateCheck.text = stateText;
        speed.text = PlayerCont.moveSpeed.ToString();
        
    }
}
