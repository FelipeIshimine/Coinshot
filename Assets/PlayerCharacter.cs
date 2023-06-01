using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{

    private bool _pushing;
    private bool _pulling;
    
    public void Jump()
    {
    }

    public void StartPush()
    {
        _pushing = true;
    }

    public void StopPush()
    {
        _pushing = false;
    }

    public void StartPull()
    {
        _pulling = true;
    }

    public void StopPull()
    {
        _pulling = false;
    }
}
