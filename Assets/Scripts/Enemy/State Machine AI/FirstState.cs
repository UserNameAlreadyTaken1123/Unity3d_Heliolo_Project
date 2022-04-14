﻿using UnityEngine;
using StateStuff;

public class FirstState : State<AI>
{
    private static FirstState _instance;

    private FirstState()
    {
        if(_instance != null)
        {
            return;
        }

        _instance = this;
    }

    public static FirstState Instance
    {
        get
        {
            if(_instance == null)
            {
                new FirstState();
            }

            return _instance;
        }
    }

	public override bool EnterState(AI _owner)
    {
        Debug.Log("Entering First State");
		return true;
    }

    public override void ExitState(AI _owner)
    {
        Debug.Log("Exiting First State");
    }

    public override void UpdateState(AI _owner)
    {

    }
}
