using UnityEngine;
using UnityEditor;

namespace ubc.ok.ovilab.roadmap.editor
{
    [CustomEditor(typeof(RemoteDataSynchronization), true)]
    public class RemoteDataSynchronizationEditor :UnityEditor.Editor
    {
        private RemoteDataSynchronization t;
        private string currentSelectedSpawnObject;

        void OnEnable()
        {
            t = target as RemoteDataSynchronization;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUI.enabled = Application.isPlaying;

            if (GUI.enabled)
            {
                EditorGUILayout.LabelField($"Current branch: {PlaceablesManager.Instance.BranchName}");
            }

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

            if (GUILayout.Button("Delete last push"))
            {
                t.RemoveLastRemoteStorageData();
            }

            GUI.enabled = Application.isPlaying;
        }

        private void SpawnClicked(object obj)
        {
            currentSelectedSpawnObject = (string)obj;
        }
    }
}
