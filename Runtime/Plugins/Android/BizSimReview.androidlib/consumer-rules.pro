# Play Core In-App Review (defensive — Unity minify can strip reflected classes
# before R8 processes the library's own consumer rules)
-keep class com.google.android.play.core.review.** { *; }
-keep class com.google.android.play.core.review.model.** { *; }
-keep class com.google.android.play.core.review.testing.** { *; }
-keep class com.google.android.gms.tasks.** { *; }

# BizSim bridge class — called from Unity C# via AndroidJavaClass/Object
-keep class com.bizsim.google.play.review.ReviewBridge { *; }
-keep class com.bizsim.google.play.review.ReviewBridge$* { *; }
