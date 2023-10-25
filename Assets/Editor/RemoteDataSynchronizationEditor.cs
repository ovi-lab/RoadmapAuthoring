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
        private bool askCreateBranch;
        private bool askChangebranch;
        private bool showChangebranch;
        private bool askMergeBranch;
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

            if (GUILayout.Button("Overwrite Remote"))
            {
                t.OverwriteRemote();
            }

            if (GUILayout.Button("Overwrite Local"))
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
                if (askChangebranch)
                {
                    EditorGUILayout.LabelField("Are you Sure?");
                    if (GUILayout.Button($"Yes"))
                    {
                        askChangebranch = false;
                        showChangebranch = false;
                        if (!string.IsNullOrEmpty(branchToChange))
                        {
                            RemoteDataSynchronization.Instance.ChangeToRemoteBranch(branchToChange);
                        }
                        else
                        {
                            Debug.LogError($"Something went wrong, the branchName is empty");
                        }
                        branchToChange = "";
                    }
                    if (GUILayout.Button($"No"))
                    {
                        askChangebranch = false;
                        showChangebranch = false;
                        branchToChange = "";
                    }
                }
                else
                {
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

                    if (GUILayout.Button("Change"))
                    {
                        askChangebranch = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                askChangebranch = false;
            }

            showCreateBranch = EditorGUILayout.Foldout(showCreateBranch, "Create Branch");
            if (showCreateBranch)
            {
                showChangebranch = false;
                showMergeBranch = false;
                EditorGUILayout.BeginHorizontal();
                if (askCreateBranch)
                {
                    EditorGUILayout.LabelField("Are you Sure?");
                    if (GUILayout.Button($"Yes"))
                    {
                        askCreateBranch = false;
                        showCreateBranch = false;
                        if (!string.IsNullOrEmpty(branchToChange))
                        {
                            PlaceablesManager.Instance.SetBranchName(branchToChange);
                        }
                        else
                        {
                            Debug.LogError($"Something went wrong, the branchName is empty");
                        }
                        branchToChange = "";
                    }
                    if (GUILayout.Button($"No"))
                    {
                        askCreateBranch = false;
                        showCreateBranch = false;
                        branchToChange = "";
                    }
                }
                else
                {
                    branchToChange = EditorGUILayout.TextField("Branch name", branchToChange);

                    if (GUILayout.Button("Create"))
                    {
                        askCreateBranch = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                askCreateBranch = false;
            }

            showMergeBranch = EditorGUILayout.Foldout(showMergeBranch, "Merge Branch");
            if (showMergeBranch)
            {
                showChangebranch = false;
                showCreateBranch = false;
                EditorGUILayout.BeginHorizontal();
                if (askMergeBranch)
                {
                    EditorGUILayout.LabelField("Are you Sure?");
                    if (GUILayout.Button($"Yes"))
                    {
                        askMergeBranch = false;
                        showMergeBranch = false;
                        if (!string.IsNullOrEmpty(branchToChange))
                        {
                            RemoteDataSynchronization.Instance.MergeWithRemoteBranch(branchToChange);
                        }
                        else
                        {
                            Debug.LogError($"Something went wrong, the branchName is empty");
                        }
                        branchToChange = "";
                    }
                    if (GUILayout.Button($"No"))
                    {
                        askMergeBranch = false;
                        showMergeBranch = false;
                        branchToChange = "";
                    }
                }
                else
                {
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

                    if (GUILayout.Button("Merge"))
                    {
                        askMergeBranch = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                askMergeBranch = false;
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
    }
}
