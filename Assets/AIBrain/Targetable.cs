using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Designate a GameObject as possible target for Brain's tactic
/// </summary>
public class Targetable : MonoBehaviour
{
    static List<Targetable> s_Targets = new List<Targetable>();

    static public List<Targetable> targets {get { return s_Targets; }}

    public string category;

    void OnEnable()
    {
        s_Targets.Add(this);
    }

    void OnDisable()
    {
        s_Targets.Remove(this);
    }
}
