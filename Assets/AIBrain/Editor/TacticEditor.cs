using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(Tactic))]
public class TacticEditor : Editor
{
    Tactic m_Target;

    IEnumerable<System.Type> m_TriggerTypes;
    string[] m_TriggerTypeNames;

    IEnumerable<System.Type> m_BehaviorTypes;
    string[] m_BehaviorTypeNames;
    string[] m_BehaviorPrettyNames;

    bool[] m_Displayed;

    void OnEnable()
    {
        m_Target = target as Tactic;
        if (m_Target.triggers == null)
            m_Target.triggers = new TacticTrigger[0];

        //trigger types
        m_TriggerTypes = typeof(TacticTrigger).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(TacticTrigger)) && !t.IsAbstract);

        m_TriggerTypeNames = new string[m_TriggerTypes.Count() + 1];
        m_TriggerTypeNames[0] = "None";
        int i = 1;
        foreach(System.Type t in m_TriggerTypes)
        {
            m_TriggerTypeNames[i] = t.Name;
            i++;
        }

        //behavior types
        m_BehaviorTypes = typeof(BehaviorModule).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(BehaviorModule)) && !t.IsAbstract);

        m_BehaviorTypeNames = new string[m_BehaviorTypes.Count() + 1];
        m_BehaviorPrettyNames = new string[m_BehaviorTypes.Count() + 1];
        m_BehaviorTypeNames[0] = "None";
        i = 1;
        foreach (System.Type t in m_BehaviorTypes)
        {
            m_BehaviorTypeNames[i] = t.Name;
            i++;
        }
    }

    public override void OnInspectorGUI()
    {
        if(m_Displayed == null || m_Displayed.Length != m_Target.triggers.Length)
        {
            m_Displayed = new bool[m_Target.triggers.Length];
        }


        int addType = EditorGUILayout.Popup("Create new Trigger", 0, m_TriggerTypeNames);
        if(addType != 0)
        {
            TacticTrigger t = (TacticTrigger)System.Activator.CreateInstance(m_TriggerTypes.ElementAt(addType - 1));
            ArrayUtility.Add(ref m_Target.triggers, t);

            EditorUtility.SetDirty(m_Target);
        }

        if (m_Target.triggers != null)
        {
            for (int i = 0; i < m_Target.triggers.Length; ++i)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal("box");
                m_Displayed[i] = EditorGUILayout.Foldout(m_Displayed[i], m_Target.triggers[i].ToString());
                EditorGUILayout.EndHorizontal();

                if (m_Displayed[i])
                {
                    FieldInfo[] infos = m_Target.triggers[i].GetType().GetFields(BindingFlags.DeclaredOnly|BindingFlags.Instance|BindingFlags.Public);
                    for(int k = 0; k < infos.Length; ++k)
                    {
                        EditorGUI.BeginChangeCheck();
                        var value = infos[k].GetValue(m_Target.triggers[i]);

                        value = DoTypeGUI(value, infos[k].FieldType, infos[k].Name);

                        if (EditorGUI.EndChangeCheck())
                        {
                            infos[k].SetValue(m_Target.triggers[i], value);
                        }
                    }
                }

                int current = 0;
                if (m_Target.triggers[i].behaviorModule != null && m_BehaviorTypeNames.Contains(m_Target.triggers[i].behaviorModule.GetType().Name))
                    current = System.Array.IndexOf(m_BehaviorTypeNames, m_Target.triggers[i].behaviorModule.GetType().Name);

                EditorGUI.BeginChangeCheck();
                current = EditorGUILayout.Popup("Behavior : ", current, m_BehaviorTypeNames);
                if(EditorGUI.EndChangeCheck())
                {
                    BehaviorModule module = (BehaviorModule)System.Activator.CreateInstance(m_BehaviorTypes.ElementAt(current - 1));
                    m_Target.triggers[i].behaviorModule = module;
                }

                EditorGUILayout.EndVertical();
            }
        }
    }

    object DoTypeGUI(object b, System.Type type, string name)
    {
        if(type == typeof(int))
        {
            return EditorGUILayout.DelayedIntField(name, (int)b);
        }
        else if(type == typeof(float))
        {
            return EditorGUILayout.DelayedFloatField(name, (float)b);
        }
        else if(type == typeof(string))
        {
            return EditorGUILayout.DelayedTextField(name, (string)b);
        }

        return null;
    }
}
