using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ubc.ok.ovilab.roadmap.editor
{
    [CustomEditor(typeof(RoadmapApplicationConfig), true)]
    [CanEditMultipleObjects]
    public class RoadmapApplicationConfigEditor : Editor
    {
        RoadmapApplicationConfig config;
        bool duplicatesInIdentifiers;
        bool duplicatesInPrefabs;

        private void OnEnable()
        {
            config = target as RoadmapApplicationConfig;
        }

        public override void OnInspectorGUI()
        {
            if (config.groupID != RoadmapSettings.instance.groupID)
            {
                config.groupID = RoadmapSettings.instance.groupID;
                Save();
            }

            EditorGUILayout.LabelField($"<color=#dddddd>Group ID:    <b>{config.groupID}</b></color>", new GUIStyle() {richText = true});
            if (string.IsNullOrEmpty(RoadmapSettings.instance.groupID))
            {
                EditorGUILayout.HelpBox("Set your Group ID", MessageType.Error);
            }

            if (GUILayout.Button(new GUIContent("Set Group ID", "Make sure all members of the team use the same ID")))
            {
                GroupIDPopup window = (GroupIDPopup)EditorWindow.GetWindow(typeof(GroupIDPopup));
                window.ShowPopup();
            }
            EditorGUILayout.Space();

            base.OnInspectorGUI();
            serializedObject.Update();

            if (config.stateChanged)
            {
                duplicatesInPrefabs = config.VerifyAssetDuplicates();
                if (duplicatesInPrefabs)
                {
                    EditorGUILayout.HelpBox($"There are duplicate entries in identifiers", MessageType.Warning);
                }

                duplicatesInIdentifiers = config.VerifyIdentifierDuplicates();
                if (duplicatesInIdentifiers)
                {
                    EditorGUILayout.HelpBox($"There are duplicate entries in prefabs", MessageType.Warning);
                }
                config.stateChanged = false;
            }

            EditorGUILayout.BeginHorizontal();

            if (duplicatesInIdentifiers)
            {
                if (GUILayout.Button(new GUIContent("Remove duplicate identifiers",
                                                    "Keeps only one of the entries with the same name")))
                {
                    config.RemoveDuplicateNames();
                    config.stateChanged = true;
                    Save();
                }
            }

            if (duplicatesInPrefabs)
            {
                if (GUILayout.Button(new GUIContent("Remove duplicate prefabs",
                                                    "Keeps only one of the entries with the same prefab")))
                {
                    config.RemoveDuplicatePrefabs();
                    config.stateChanged = true;
                    Save();
                }
            }
            EditorGUILayout.EndHorizontal();

            if(GUILayout.Button(new GUIContent("Add prefabs from a folder",
                                               "Automatically add files with extension `.prefab` or `.fbx` to the `Placables` list.")))
            {
                FileSelectionPopup window = (FileSelectionPopup)EditorWindow.GetWindow(typeof(FileSelectionPopup));

                window.SetOkCallback((files) => {
                    foreach (string file in files)
                    {
                        config.AddPrefab(Path.GetFileNameWithoutExtension(file),
                                         AssetDatabase.LoadAssetAtPath<GameObject>(file));
                    }
                    Save();
                    config.stateChanged = true;
                });
                window.ShowPopup();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void Save()
        {
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// Window to add files from a directory
    /// </summary>
    class FileSelectionPopup : EditorWindow
    {
        private string path;
        private List<string> files;
        private System.Action<string[]> okCallback;
        private Vector2 scrollPos;
        private Object folder;
        private Object prevFolder;
        private bool checkPath = false,
            addPrefabs = true,
            addFbx = true;

        /// <summary>
        /// Set the callback for the ok buttons onClick.
        /// </summary>
        public void SetOkCallback(System.Action<string[]> okCallback)
        {
            this.okCallback = okCallback;
        }

        /// <summary>
        /// Validate if the given folder.
        /// </summary>
        bool CheckFolder()
        {
            return folder != null && AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(folder));
        }

        // Unity method
        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            folder = EditorGUILayout.ObjectField("Location: ", folder, typeof(DefaultAsset), false);
            if (GUILayout.Button("Select folder", GUILayout.Width(100)))
            {
                path = EditorUtility.OpenFolderPanel("Load prefabs from direcotry", "", "");
                path = Path.GetRelativePath(Path.GetDirectoryName(Application.dataPath), path);
                checkPath = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            addPrefabs = EditorGUILayout.Toggle("Add .prefabs", addPrefabs);
            addFbx = EditorGUILayout.Toggle("Add .fbx", addFbx);

            if (EditorGUI.EndChangeCheck())
            {
                checkPath = true;
            }

            if (!addPrefabs && !addFbx)
            {
                EditorGUILayout.HelpBox("Both Add prefab and Add FBX cannot be unchecked.", MessageType.Error);
                checkPath = false;
                files.Clear();
            }

            if (checkPath && !string.IsNullOrEmpty(path))
            {
                folder = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                if (!CheckFolder())
                {
                    EditorGUILayout.HelpBox("Folder invalid. Consider drag and drop.", MessageType.Error);
                }
            }

            if (CheckFolder())
            {
                if (checkPath || folder != prevFolder)
                {
                    files = new List<string>();
                    List<string> filters = new List<string>();

                    if (addFbx)
                    {
                        filters.Add("t:model");
                    }

                    if (addPrefabs)
                    {
                        filters.Add("t:prefab");
                    }

                    // FIXME: For some reason the use of the "or" operator is not working here.
                    // Hence, getting files for each filter and combining them.
                    foreach(string filter in filters)
                    {
                        files.AddRange(AssetDatabase.FindAssets(filter, new string[] { AssetDatabase.GetAssetPath(folder) })
                                       .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                                       .ToList());
                    }

                    prevFolder = folder;
                }

                checkPath = false;
                path = null;

                GUILayout.Label("Add all prefabs in directory:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("From location: " + AssetDatabase.GetAssetPath(folder));
                EditorGUILayout.LabelField("Count: " + files.Count);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                EditorGUILayout.BeginVertical();
                EditorGUILayout.HelpBox(string.Join("\n", files.Select(x => "- " + x)), MessageType.None);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();

                if(GUILayout.Button("OK"))
                {
                    this.okCallback?.Invoke(files.ToArray());
                    this.Close();
                }

                if(GUILayout.Button("Cancel"))
                {
                    this.Close();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Drag and drop a directory from the Project window or use 'Select folder'.");
            }
        }
    }

    /// <summary>
    /// Window to get the group ID.
    /// </summary>
    class GroupIDPopup: EditorWindow
    {
        private string groupID;

        void OnEnable()
        {
            groupID = RoadmapSettings.instance.groupID;
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Make sure all members of the team use the same ID");
            groupID = EditorGUILayout.TextField(text:groupID, label:"Group ID: ");

            GUI.enabled = groupID != RoadmapSettings.instance.groupID;
            if (GUILayout.Button("Set Group ID"))
            {
                RoadmapSettings.instance.groupID = groupID;
                RoadmapSettings.instance.Save();
                this.Close();
            }
            GUI.enabled = true;
        }
    }
}
