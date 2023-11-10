using UnityEngine;
using UnityEditor;

namespace ubc.ok.ovilab.roadmap.editor
{
    [CustomEditor(typeof(PlaceablesManager), true)]
    public class PlaceablesManagerEditor :UnityEditor.Editor
    {
        private PlaceablesManager t;
        private string currentSelectedSpawnObject;

        void OnEnable()
        {
            t = target as PlaceablesManager;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUI.enabled = Application.isPlaying;

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Active config name:", $"{RoadmapSettings.instance.activeConfig.name} ({RoadmapSettings.instance.activeConfig.identifier})");

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Spawn object");
            if (GUILayout.Button(currentSelectedSpawnObject == null ? "Select" : $"Selected: {currentSelectedSpawnObject}"))
            {
                GenericMenu menu = new GenericMenu();
                foreach(var identifier in RoadmapSettings.instance.activeConfig.PlacableIdentifierList())
                {
                    menu.AddItem(new GUIContent(identifier), false, SpawnClicked, identifier);
                }

                menu.ShowAsContext();
            }

            GUI.enabled = Application.isPlaying && currentSelectedSpawnObject != null;
            if (GUILayout.Button("Spawn"))
            {
                if (RoadmapApplicationConfig.activeApplicationConfig != RoadmapSettings.instance.activeConfig)
                {
                    RoadmapApplicationConfig.activeApplicationConfig = RoadmapSettings.instance.activeConfig;
                }
                t.SpawnObject(currentSelectedSpawnObject);
            }
            GUI.enabled = Application.isPlaying;

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Clear data"))
            {
                t.ClearData();
            }

            if (GUILayout.Button("Toggle modifyable"))
            {
                t.Modifyable = !t.Modifyable;
            }

            GUI.enabled = Application.isPlaying;
        }

        private void SpawnClicked(object obj)
        {
            currentSelectedSpawnObject = (string)obj;
        }
    }
}
