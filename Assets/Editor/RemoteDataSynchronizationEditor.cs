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
                if (EditorUtility.DisplayDialog("Syncing branch", $"Sync branch `{PlaceablesManager.Instance.BranchName}` with remote. \nAre you Sure?", "yes", "no"))
                {
                    t.SyncWithRemote();
                }
            }

            if (GUILayout.Button("Push"))
            {
                if (EditorUtility.DisplayDialog("Pushing", $"Pushing changes of branch `{PlaceablesManager.Instance.BranchName}` to remote. \nAre you Sure?", "yes", "no"))
                {
                    t.OverwriteRemote();
                }
            }

            if (GUILayout.Button("Pull"))
            {
                if (EditorUtility.DisplayDialog("Pulling", $"Pulling changes of branch `{PlaceablesManager.Instance.BranchName}` from remote. \nAre you Sure?", "yes", "no"))
                {
                    t.OverwriteLocal();
                }
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

                string message;
                if (t.GetBranches().Contains(PlaceablesManager.Instance.BranchName))
                {
                    message = $"Active branch `{PlaceablesManager.Instance.BranchName}` not seen in remote. Try updating branch list or pushing. If this branch is not in the remote, all data for this branch will be lost.";
                }
                else
                {
                    message = $"Changing to branch {branchToChange} from {PlaceablesManager.Instance.BranchName}";
                }
                ButtonWithCheck_branchToChange("Change", message,
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
                    if (EditorUtility.DisplayDialog(message, $"{message}. \nAre you Sure?", "yes", "no"))
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
