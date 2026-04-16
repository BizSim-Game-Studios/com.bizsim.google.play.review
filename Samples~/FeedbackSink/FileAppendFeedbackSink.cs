using System;
using System.IO;
using UnityEngine;

namespace BizSim.Google.Play.Review.Samples
{
    /// <summary>
    /// Sample <see cref="IFeedbackSink"/> that appends feedback entries as JSON lines to
    /// <c>Application.persistentDataPath/bizsim-feedback.jsonl</c>.
    /// <para>
    /// Each line is a self-contained JSON object with <c>timestamp</c> (ISO 8601) and
    /// <c>text</c> fields. The file grows monotonically; consider clearing it on app
    /// update or periodically uploading and truncating.
    /// </para>
    /// </summary>
    public sealed class FileAppendFeedbackSink : IFeedbackSink
    {
        private const string FILE_NAME = "bizsim-feedback.jsonl";
        private readonly string _filePath;

        public FileAppendFeedbackSink()
        {
            _filePath = Path.Combine(Application.persistentDataPath, FILE_NAME);
        }

        public FileAppendFeedbackSink(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        /// <summary>The full path to the feedback file.</summary>
        public string FilePath => _filePath;

        public void SubmitFeedback(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            try
            {
                var timestamp = DateTime.UtcNow.ToString("O");
                // Manual JSON construction to avoid JsonUtility overhead for a two-field object.
                // Escape backslashes and double quotes in the text to produce valid JSON.
                var escaped = text
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t");
                var line = $"{{\"timestamp\":\"{timestamp}\",\"text\":\"{escaped}\"}}";

                File.AppendAllText(_filePath, line + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // IFeedbackSink contract: implementations must not throw.
                // Log and swallow — the controller also wraps in try/catch, but defense in depth.
                Debug.LogWarning($"[BizSim.Review] FileAppendFeedbackSink.SubmitFeedback failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears all stored feedback. Call on app update or after uploading to a server.
        /// </summary>
        public void Clear()
        {
            try
            {
                if (File.Exists(_filePath))
                    File.Delete(_filePath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BizSim.Review] FileAppendFeedbackSink.Clear failed: {ex.Message}");
            }
        }
    }
}
