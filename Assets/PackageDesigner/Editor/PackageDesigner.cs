using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Text;

public class PackageDesigner : EditorWindow
{
    AssetPackage[] m_AssetPackageList;
    Vector2 m_PackageListScrollPos;

    AssetPackage m_CurrentlyEdited = null;

    DependencyTreeView m_DepTreeView;
    TreeViewState m_tvstate;

    [MenuItem("Content/PackageDesigner")]
    static void Open()
    {
        GetWindow<PackageDesigner>();
    }

    private void OnEnable()
    {
        GetAllPackageList();
        m_tvstate = new TreeViewState();
        m_DepTreeView = new DependencyTreeView(m_tvstate);

        Undo.undoRedoPerformed += UndoPerformed;

        PopulateTreeview();
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= UndoPerformed;
    }

    void UndoPerformed()
    {
        PopulateTreeview();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Export All Package"))
        {
            ExportAll();
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUILayout.Width(128));
        if(GUILayout.Button("New..."))
        {
            string savePath = EditorUtility.SaveFilePanelInProject("Save package file", "package", "asset", "Save package");
            if(savePath != "")
            {
                AssetPackage package = CreateInstance<AssetPackage>();
                AssetDatabase.CreateAsset(package, savePath.Replace(Application.dataPath, "Assets"));
                m_CurrentlyEdited = package;
                ArrayUtility.Add(ref m_AssetPackageList, package);
                AssetDatabase.Refresh();
            }
        }

        m_PackageListScrollPos = EditorGUILayout.BeginScrollView(m_PackageListScrollPos, "box");

        for (int i = 0; i < m_AssetPackageList.Length; ++i)
        {
            if (m_AssetPackageList[i] != m_CurrentlyEdited)
            {
                if (GUILayout.Button(m_AssetPackageList[i].packageName))
                {
                    m_CurrentlyEdited = m_AssetPackageList[i];
                    PopulateTreeview();
                }
            }
            else
            {
                GUILayout.Label(m_AssetPackageList[i].packageName);
            }
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        if (m_CurrentlyEdited != null)
            EditedUI();

        EditorGUILayout.EndHorizontal();
    }

    void ExportCurrentPackage(string saveTo = "")
    {
        if(saveTo == "")
            saveTo = EditorUtility.SaveFilePanel("Export package", Application.dataPath.Replace("/Assets", ""), m_CurrentlyEdited.packageName, "unitypackage");

        AssetDatabase.ExportPackage(m_CurrentlyEdited.dependencies, saveTo, ExportPackageOptions.Default);
    }

    void ExportAll()
    {
        string saveFolder = EditorUtility.SaveFolderPanel("Choose output Folder", Application.dataPath.Replace("/Assets", ""), "Package Ouput");
        if (saveFolder == "")
            return;

        AssetPackage current = m_CurrentlyEdited;
        for(int i = 0; i < m_AssetPackageList.Length; ++i)
        {
            m_CurrentlyEdited = m_AssetPackageList[i];
            ExportCurrentPackage(saveFolder + "/" + m_CurrentlyEdited.packageName + ".unitypackage");
        }
    }

    void EditedUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        m_CurrentlyEdited.packageName = EditorGUILayout.DelayedTextField("Package Name", m_CurrentlyEdited.packageName);
        if(GUILayout.Button("Export ..."))
        {
            ExportCurrentPackage();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();

        if (Event.current.type == EventType.DragUpdated)
        {
            Rect rect = GUILayoutUtility.GetLastRect();
            DragAndDrop.visualMode = rect.Contains(Event.current.mousePosition) ? DragAndDropVisualMode.Move : DragAndDropVisualMode.Rejected;
        }
        else if (Event.current.type == EventType.DragPerform)
        {
            GetAllAssetsDependency(DragAndDrop.objectReferences);
        }

        Rect r = GUILayoutUtility.GetLastRect();
        m_DepTreeView.OnGUI(r);

        if (m_DepTreeView.assetPreviews != null && m_DepTreeView.assetPreviews.Length > 0)
        {
            EditorGUILayout.BeginHorizontal("box", GUILayout.Height(128.0f));
            if (m_DepTreeView.assetPreviews != null)
            {
                for (int i = 0; i < m_DepTreeView.assetPreviews.Length; ++i)
                {
                    EditorGUILayout.LabelField(m_DepTreeView.assetPreviews[i], GUILayout.Height(128.0f));
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    void GetAllAssetsDependency(Object[] objs)
    {
        Undo.RecordObject(m_CurrentlyEdited, "Dependencies added");
        if(m_CurrentlyEdited.dependencies == null)
            m_CurrentlyEdited.dependencies = new string[0];

        for(int i = 0; i < objs.Length; ++i)
        {
            string[] str = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(objs[i]));

            for (int j = 0; j < str.Length; ++j)
            {
                if (!ArrayUtility.Contains(m_CurrentlyEdited.dependencies, str[j]))
                {
                    ArrayUtility.Add(ref m_CurrentlyEdited.dependencies, str[j]);
                }
            }
        }
        PopulateTreeview();
    }

    void PopulateTreeview()
    {
        if (m_CurrentlyEdited == null)
            return;

        m_DepTreeView.assetPackage = m_CurrentlyEdited;
        m_DepTreeView.Reload();
        m_DepTreeView.ExpandAll();

        string[] files = new string[0];
        for(int i = 0; i < m_CurrentlyEdited.dependencies.Length; ++i)
        {
            if(AssetDatabase.GetMainAssetTypeAtPath(m_CurrentlyEdited.dependencies[i]) == typeof(MonoScript))
            {
                ArrayUtility.Add(ref files, m_CurrentlyEdited.dependencies[i].Replace("Assets", Application.dataPath));
            }
        }

        if(files.Length != 0)
        {
            CheckCanCompile(files);
        }
    }

    void GetAllPackageList()
    {
        string[] assets = AssetDatabase.FindAssets("t:AssetPackage");
        m_AssetPackageList = new AssetPackage[assets.Length];

        for(int i = 0; i < assets.Length; ++i)
        {
            m_AssetPackageList[i] = AssetDatabase.LoadAssetAtPath<AssetPackage>(AssetDatabase.GUIDToAssetPath(assets[i]));
        }
    }

    void CheckCanCompile(string[] files)
    {
        CSharpCodeProvider provider = new CSharpCodeProvider();
        CompilerParameters parameters = new CompilerParameters();

        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.Location.Contains("Library"))
                continue;//we don't add the library specific to the project, as we want to test compilign for any project
            parameters.ReferencedAssemblies.Add(assembly.Location);
        }

        parameters.GenerateInMemory = true;
        parameters.GenerateExecutable = false;

        CompilerResults results = provider.CompileAssemblyFromFile(parameters, files);

        if (results.Errors.HasErrors)
        {
            StringBuilder sb = new StringBuilder();

            foreach (CompilerError error in results.Errors)
            {
                sb.AppendLine(string.Format("Error in {0} : {1}", error.FileName, error.ErrorText));
            }

            Debug.Log(sb);
        }
        else
        {
            Debug.Log("succesful compiled");
        }
    }
}


public class DependencyTreeView : TreeView
{
    public AssetPackage assetPackage = null;
    public GUIContent[] assetPreviews = null;


    //TODO replace that with proper id handling, reusing etc. For now simple increment will be enough
    protected int freeID = 0;

    protected override TreeViewItem BuildRoot()
    {
        TreeViewItem item = new TreeViewItem();
        item.depth = -1;
        if (assetPackage == null || assetPackage.dependencies == null || assetPackage.dependencies.Length == 0)
        {
            TreeViewItem itm = new TreeViewItem(freeID, 0, "Nothing");
            item.AddChild(itm);
            return item;
        }

        for(int i = 0; i < assetPackage.dependencies.Length; ++i)
        {
            RecursiveAdd(item, assetPackage.dependencies[i], assetPackage.dependencies[i]);
        }

        SetupDepthsFromParentsAndChildren(item);
        return item;
    }

    void RecursiveAdd(TreeViewItem root, string value, string fullPath)
    {
        int idx = value.IndexOf('/');

        if (idx > 0)
        {
            string node = value.Substring(0, idx);
            string childValue = value.Substring(idx+1);

            if (root.hasChildren)
            {
                for (int i = 0; i < root.children.Count; ++i)
                {
                    if (root.children[i].displayName == node)
                    {
                        RecursiveAdd(root.children[i], childValue, fullPath);
                        return;
                    }
                }
            }

            //we didn't find a children named that way, so create a new one
            TreeViewItem itm = new TreeViewItem(freeID);
            freeID += 1;
            itm.displayName = node;
            root.AddChild(itm);
            RecursiveAdd(itm, childValue, fullPath);
        }
        else
        {//this is a leaf node, just create a new one and add it to the root
            AssetTreeViewItem itm = new AssetTreeViewItem(freeID);
            freeID += 1;

            itm.displayName = value;
            itm.fullAssetPath = fullPath;

            Object obj = AssetDatabase.LoadAssetAtPath(fullPath, typeof(UnityEngine.Object));
            itm.icon = AssetPreview.GetMiniThumbnail(obj);

            root.AddChild(itm);
        }
    }

    protected override void SelectionChanged(IList<int> selectedIds)
    {
        base.SelectionChanged(selectedIds);

        assetPreviews = new GUIContent[0];

        IList<TreeViewItem> items = FindRows(selectedIds);

        for(int i = 0; i < items.Count; ++i)
        {
            if(!items[i].hasChildren)
            {
                AssetTreeViewItem itm = items[i] as AssetTreeViewItem;
                if(itm != null)
                {
                    GUIContent content = new GUIContent(AssetPreview.GetAssetPreview(AssetDatabase.LoadAssetAtPath(itm.fullAssetPath, typeof(Object))));
                    ArrayUtility.Add(ref assetPreviews, content);
                }
            }
        }
    }

    protected override void CommandEventHandling()
    {
        base.CommandEventHandling();

        if(Event.current.commandName.Contains("Delete"))
        {
            IList<TreeViewItem> items = FindRows(GetSelection());
            bool haveDelete = false;

            Undo.RecordObject(assetPackage, "Delete dependency");
            for (int i = 0; i < items.Count; ++i)
            {
                RecursiveDelete(items[i], ref haveDelete);
            }

            if (haveDelete)
            {
                Reload();
                ExpandAll();
            }
        }
    }

    void RecursiveDelete(TreeViewItem item, ref bool haveDelete)
    {
        if (!item.hasChildren)
        {
            AssetTreeViewItem itm = item as AssetTreeViewItem;
            if (itm != null)
            {
                if (!haveDelete)
                {
                    haveDelete = true;
                }

                ArrayUtility.Remove(ref assetPackage.dependencies, itm.fullAssetPath);
            }
        }
        else
        {
            for (int j = 0; j < item.children.Count; ++j)
            {
                RecursiveDelete(item.children[j], ref haveDelete);
            }
        }
    }

    public DependencyTreeView(TreeViewState state) : base(state)
    {
        freeID = 0;
    }
}

public class AssetTreeViewItem : TreeViewItem
{
    public string fullAssetPath = "";

    public AssetTreeViewItem(int id) : base(id) { }
}