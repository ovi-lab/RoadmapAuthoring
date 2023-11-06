using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ubc.ok.ovilab.roadmap.editor
{
    [CustomEditor(typeof(RemoteDataSynchronization), true)]
    public class RemoteDataSynchronizationEditor :UnityEditor.Editor
    {
        private RemoteDataSynchronization t;
        private string branchToChange;
        private bool showCreateBranch;
        private bool showChangebranch;
        private bool showMergeBranch;

        void OnEnable()
        {
            t = target as RemoteDataSynchronization;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUI.enabled = Application.isPlaying;

            EditorGUILayout.Separator();

            if (GUILayout.Button("Sync With Remote"))
            {
                t.SyncWithRemote();
            }

            if (GUILayout.Button("Push"))
            {
                t.OverwriteRemote();
            }

            if (GUILayout.Button("Pull"))
            {
                t.OverwriteLocal();
            }

            if (GUILayout.Button("Update branches list"))
            {
                t.UpdateBranchesList();
            }

            EditorGUILayout.Separator();

            if (GUI.enabled)
            {
                EditorGUILayout.LabelField($"Current branch: {PlaceablesManager.Instance.BranchName}");
            }

            showChangebranch = EditorGUILayout.Foldout(showChangebranch, "Change branch");
            if (showChangebranch)
            {
                showCreateBranch = false;
                showMergeBranch = false;
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(branchToChange))
                {
                    GenericMenu menu = new GenericMenu();
                    List<string> branches = RemoteDataSynchronization.Instance.GetBranches();
                    if (branches != null)
                    {
                        foreach (var branch in branches)
                        {
                            menu.AddItem(new GUIContent(branch), false, BranchClicked, branch);
                        }
                    }

                    menu.ShowAsContext();
                }

                ButtonWithCheck_branchToChange("Change", $"Changing to branch {branchToChange}",
                                               () => RemoteDataSynchronization.Instance.ChangeToRemoteBranch(branchToChange));
                EditorGUILayout.EndHorizontal();
            }

            showCreateBranch = EditorGUILayout.Foldout(showCreateBranch, "Create Branch");
            if (showCreateBranch)
            {
                showChangebranch = false;
                showMergeBranch = false;
                EditorGUILayout.BeginHorizontal();

                branchToChange = EditorGUILayout.TextField("Branch name", branchToChange);
                ButtonWithCheck_branchToChange("Create", $"Crearting and changing to new branch {branchToChange}",
                                               () => PlaceablesManager.Instance.SetBranchName(branchToChange));
                EditorGUILayout.EndHorizontal();
            }

            showMergeBranch = EditorGUILayout.Foldout(showMergeBranch, "Merge Branch");
            if (showMergeBranch)
            {
                showChangebranch = false;
                showCreateBranch = false;
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(branchToChange))
                {
                    GenericMenu menu = new GenericMenu();
                    List<string> branches = RemoteDataSynchronization.Instance.GetBranches();
                    if (branches != null)
                    {
                        foreach (var branch in branches)
                        {
                            menu.AddItem(new GUIContent(branch), false, BranchClicked, branch);
                        }
                    }

                    menu.ShowAsContext();
                }

                ButtonWithCheck_branchToChange("Merge", $"Merging with branch {branchToChange}",
                                               () => RemoteDataSynchronization.Instance.MergeWithRemoteBranch(branchToChange));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Separator();

            if (GUILayout.Button("Delete last push"))
            {
                t.RemoveLastRemoteStorageData();
            }

            GUI.enabled = Application.isPlaying;
        }

        private void BranchClicked(object obj)
        {
            branchToChange = (string)obj;
        }

        private void ButtonWithCheck_branchToChange(string buttonString, string message, System.Action callbackOnYes)
        {
            if (GUILayout.Button(buttonString))
            {
                if (!string.IsNullOrEmpty(branchToChange))
                {
                    if (EditorUtility.DisplayDialog(message, "Are you Sure?", "yes", "no"))
                    {
                        callbackOnYes?.Invoke();
                    }
                }
                else
                {
                    Debug.LogError($"Something went wrong, the branchName is empty");
                }
                branchToChange = "";
            }
        }
    }
}
