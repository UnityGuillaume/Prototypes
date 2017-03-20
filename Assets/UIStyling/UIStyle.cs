using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="UIStyle", menuName = "UI/Style")]
public class UIStyle : ScriptableObject
{
    [System.Serializable]
    public class Override
    {
        public string name;
        public string fullType;
        public object value;
    }

    public List<Override> overrides;
}
