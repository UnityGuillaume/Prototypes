using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompilingScriptTest : MonoBehaviour
{
    SpecialRendering special;


	// Use this for initialization
	void Start () {
        special.BroadcastMessage("Test");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
