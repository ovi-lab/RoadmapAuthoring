using System.Linq;
using TMPro;
using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    public class DebugMessages : Singleton<DebugMessages>
    {
        public int maxMessages = 5;
        public bool logToDebugLog = true;

        private TextMeshProUGUI debugText;

        private void Start()
        {
            if (debugText == null)
            {
                debugText = GetComponent<TextMeshProUGUI>();
            }
        }

        public void LogToDebugText(string message)
        {
            if (logToDebugLog)
            {
                Debug.Log($"[DEBUG_MSG] {message}");
            }

            if (debugText != null)
            {
                string[] lines = debugText.text.Split('\n');
                debugText.text = $"{message}\n" + string.Join("\n", lines.Take(maxMessages));
            }
        }

        public void ClearDebugText()
        {
            debugText.text = string.Empty;
        }
    }
}
