using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// Brain handle the runtime decisions.
/// It gather all the currently object in the fov & max view distance and pass them to the triggers.
/// They will handle decision making based on that, switching currently active module if condition are satisfied
/// </summary>
public class Brain : MonoBehaviour
{
    public Tactic tactic;

    public float fov;
    public float viewDistance;

    public List<Targetable> currentTargetables;

    public Targetable currentTarget;

    protected BehaviorModule m_CurrentModule;

	// Use this for initialization
	void Start () {
        currentTargetables = new List<Targetable>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        GetCurrentTargetable();
        GetCurrentBehavior();

        if(m_CurrentModule!=null)
            m_CurrentModule.Execute(this);
    }

    void GetCurrentTargetable()
    {
        currentTargetables.Clear();

        float sqrViewDist = viewDistance * viewDistance;
        float dotValue = Mathf.Cos(fov * 0.5f);

        for (int  i = 0 ; i < Targetable.targets.Count; ++i)
        {
            Vector3 targetToSelf = Targetable.targets[i].transform.position - transform.position;
            if(targetToSelf.sqrMagnitude < sqrViewDist)
            {
                targetToSelf.Normalize();
                if(Vector3.Dot(targetToSelf, transform.forward) > dotValue)
                {
                    currentTargetables.Add(Targetable.targets[i]);
                }
            }
        }
    }

    void GetCurrentBehavior()
    {
        for(int  i = 0; i < tactic.triggers.Length;++i)
        {
            if(tactic.triggers[i].IsValid(this))
            {
                m_CurrentModule = tactic.triggers[i].behaviorModule;
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Brain))]
public class BrainEditor : Editor
{
    Brain m_Target;

    void OnEnable()
    {
        m_Target = target as Brain;
    }

    void OnSceneGUI()
    {
        Color c = Handles.color;
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        Vector3 start = m_Target.transform.forward;
        start = Quaternion.AngleAxis(-m_Target.fov * 0.5f, m_Target.transform.up) * start;

        Handles.DrawSolidArc(m_Target.transform.position, m_Target.transform.up, start, m_Target.fov, m_Target.viewDistance);
        Handles.color = c;
    }
}
#endif