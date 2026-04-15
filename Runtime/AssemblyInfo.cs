using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]
[assembly: Preserve]

// Required so Tests/Runtime/ can access `internal static class PackageVersion`
// and other internal helpers via reflection + direct access.
[assembly: InternalsVisibleTo("BizSim.Google.Play.Review.Tests")]
[assembly: InternalsVisibleTo("BizSim.Google.Play.Review.EditorTests")]

// Editor assembly needs internal access too — the ReviewConfiguration window calls
// BizSimLogger.InvalidateCache() (declared public per CROSS-INVARIANTS §12.3 visibility note,
// but other internal hooks may follow the same path).
[assembly: InternalsVisibleTo("BizSim.Google.Play.Review.Editor")]
