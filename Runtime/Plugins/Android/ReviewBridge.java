package com.bizsim.google.play.review;

import android.app.Activity;
import android.content.Context;
import android.os.Handler;
import android.os.Looper;

import com.google.android.play.core.review.ReviewException;
import com.google.android.play.core.review.ReviewInfo;
import com.google.android.play.core.review.ReviewManager;
import com.google.android.play.core.review.ReviewManagerFactory;
import com.google.android.play.core.review.model.ReviewErrorCode;
import com.google.android.play.core.review.testing.FakeReviewManager;

import com.google.android.gms.tasks.Task;

public final class ReviewBridge {
    private static final String TAG = "BizSimReview";

    public interface IReviewCallback {
        void onFlowCompleted();
        void onError(int errorCode, String message);
    }

    private final ReviewManager manager;
    private final Activity activity;
    private final Handler mainHandler;
    private IReviewCallback callback;

    private ReviewBridge(Activity activity, boolean useFake) {
        this.activity = activity;
        this.mainHandler = new Handler(Looper.getMainLooper());
        this.manager = useFake
            ? new FakeReviewManager(activity.getApplicationContext())
            : ReviewManagerFactory.create(activity.getApplicationContext());
    }

    public static ReviewBridge create(Activity activity) { return new ReviewBridge(activity, false); }
    public static ReviewBridge createFake(Activity activity) { return new ReviewBridge(activity, true); }

    public void setCallback(IReviewCallback callback) { this.callback = callback; }

    public void requestReviewFlow() {
        mainHandler.post(() -> {
            try {
                Task<ReviewInfo> request = manager.requestReviewFlow();
                request.addOnCompleteListener(task -> {
                    if (task.isSuccessful()) {
                        launchInternal(task.getResult());
                    } else {
                        int code = (task.getException() instanceof ReviewException)
                            ? ((ReviewException) task.getException()).getErrorCode()
                            : ReviewErrorCode.INTERNAL_ERROR;
                        emitError(code, task.getException() != null ? task.getException().getMessage() : "request failed");
                    }
                });
            } catch (Throwable t) {
                emitError(ReviewErrorCode.INTERNAL_ERROR, t.getMessage());
            }
        });
    }

    private void launchInternal(ReviewInfo info) {
        try {
            Task<Void> flow = manager.launchReviewFlow(activity, info);
            flow.addOnCompleteListener(t -> emitCompleted());
        } catch (Throwable t) {
            emitError(ReviewErrorCode.INTERNAL_ERROR, t.getMessage());
        }
    }

    private void emitCompleted() {
        if (callback != null) callback.onFlowCompleted();
    }

    private void emitError(int code, String message) {
        if (callback != null) callback.onError(code, message == null ? "" : message);
    }
}
