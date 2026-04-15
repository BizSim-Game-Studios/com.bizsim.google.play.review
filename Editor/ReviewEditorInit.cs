using UnityEditor;
using BizSim.Google.Play.Editor.Core;

namespace BizSim.Google.Play.Review.Editor
{
    /// <summary>
    /// Auto-registers the <c>BIZSIM_REVIEW_INSTALLED</c> scripting define at editor load, so
    /// consumer shared code can use <c>#if BIZSIM_REVIEW_INSTALLED</c> guards without manual
    /// Player Settings edits. Runs once per editor session via <see cref="InitializeOnLoadAttribute"/>.
    /// </summary>
    [InitializeOnLoad]
    internal static class ReviewEditorInit
    {
        static ReviewEditorInit()
        {
            BizSimDefineManager.AddDefine("BIZSIM_REVIEW_INSTALLED",
                BizSimDefineManager.GetRelevantPlatforms());
        }
    }
}
