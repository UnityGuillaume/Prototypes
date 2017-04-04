using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
/*
public class UIStylingEditor : EditorWindow
{
    [MenuItem("UI/Styling Editor")]
    static void Open()
    {
        GetWindow<UIStylingEditor>();
    }

    UIStyle m_CurrentStyle = null;

    UIBehaviour[] m_UIElements;

    int m_CurrentEditor = 0;
    string[] m_Editors = { "UIElement", "Override" };

    Dictionary<string, List<UIBehaviour>> m_orderedElements = new Dictionary<string, List<UIBehaviour>>();
    string[] m_TypeNames;
    int m_SelectedType = 0;

    PropertyInfo[] m_Properties;
    string[] m_AvailableOverride;
    int m_SelectedOverride = 0;
    
    int m_SelectedElement = 0;
    Vector2 m_ScrollingValue;

    void OnEnable()
    {
        m_SelectedType = 0;

        m_CurrentStyle = Selection.activeObject as UIStyle;

        UpdateReferences();
        UpdateValue();
    }

    void OnSelectionChange()
    {
        UIStyle selected = Selection.activeObject as UIStyle;
        if (selected != null)
            m_CurrentStyle = selected;
    }

    void OnGUI()
    {
        if(m_CurrentStyle == null)
        {
            EditorGUILayout.HelpBox("Please select a UIStyle to edit in your project folder", MessageType.Error);
            return;
        }

        int editor = GUILayout.Toolbar(m_CurrentEditor, m_Editors);
        if(editor != m_CurrentEditor)
        {
            m_ScrollingValue = Vector2.zero;
        }


        GUILayout.Label("Editing UIStyle " + m_CurrentStyle.name, "box");
        if (m_CurrentEditor == 0)
            DoAssignementGUI();
        else
            DoOverrideGUI();
        
    }

    void DoAssignementGUI()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();

        EditorGUI.BeginChangeCheck();
        m_SelectedType = EditorGUILayout.Popup("Type : ", m_SelectedType, m_TypeNames);
        if (EditorGUI.EndChangeCheck())
        {
            UpdateValue();
            m_SelectedElement = 0;
            m_ScrollingValue = Vector2.zero;
        }

        List<UIBehaviour> m_CurrentBehaviors = m_orderedElements[m_TypeNames[m_SelectedType]];
        for (int i = 0; i < m_CurrentBehaviors.Count; ++i)
        {
            if (i == m_SelectedElement)
            {
                Selection.activeGameObject = m_CurrentBehaviors[i].gameObject;
                GUI.enabled = false;
            }

            if (GUILayout.Button(m_CurrentBehaviors[i].name))
            {
                m_SelectedElement = i;
            }

            GUI.enabled = true;
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Property", "box", new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
        GUILayout.FlexibleSpace();
        GUILayout.Label("Override", "box", new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
        GUILayout.EndHorizontal();

        m_ScrollingValue = GUILayout.BeginScrollView(m_ScrollingValue);
        for (int i = 0; i < m_Properties.Length; ++i)
        {
            if (m_AvailableOverride == null)
                continue;

            GUILayout.BeginHorizontal();

            GUILayout.Label(m_Properties[i].Name, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
            GUILayout.FlexibleSpace();
            EditorGUILayout.Popup(0, m_AvailableOverride, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });

            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    void DoOverrideGUI()
    {
        m_ScrollingValue = GUILayout.BeginScrollView(m_ScrollingValue);
        for(int  i =0; i < m_CurrentStyle.overrides.Count; ++i)
        {
            
        }
        GUILayout.EndScrollView();
    }

    void UpdateValue()
    {
        if (m_CurrentStyle == null || m_TypeNames.Length == 0)
            return;

        m_Properties = m_orderedElements[m_TypeNames[m_SelectedType]][0].GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

        int styleCount = 0;
        int idx = m_CurrentStyle.types.IndexOf(m_TypeNames[m_SelectedType]);
        if (idx != -1)
            styleCount = m_CurrentStyle.overrides.Count;

        m_AvailableOverride = new string[styleCount + 2];
        m_AvailableOverride[0] = "None";
        for(int i = 0; i < styleCount; ++i)
        {
            m_AvailableOverride[i + 1] = m_CurrentStyle.overrides[idx][i].name;
        }
        m_AvailableOverride[styleCount+1] = "New...";
    }

    void UpdateReferences()
    {
        m_SelectedType = 0;
        m_orderedElements.Clear();

        m_UIElements = GameObject.FindObjectsOfType<UIBehaviour>();

        List<string> typeNames = new List<string>();
        for(int i = 0; i < m_UIElements.Length; ++i)
        {
            string type = m_UIElements[i].GetType().Name;
            if(!m_orderedElements.ContainsKey(type))
            {
                m_orderedElements[type] = new List<UIBehaviour>();
                typeNames.Add(type);
            }

            m_orderedElements[type].Add(m_UIElements[i]);

            UIStyleContainer container = m_UIElements[i].GetComponent<UIStyleContainer>();
            if (container == null) m_UIElements[i].gameObject.AddComponent<UIStyleContainer>();
        }

        m_TypeNames = typeNames.ToArray();
    }


}*/