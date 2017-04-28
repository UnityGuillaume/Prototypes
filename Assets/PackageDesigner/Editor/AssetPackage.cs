using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AssetPackage : ScriptableObject
{
    public string packageName = "Package";
    public string[] dependencies = new string[0];
}
