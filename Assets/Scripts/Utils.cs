using UnityEngine;

public static class Utils 
{
    // => UI
    public static bool SetLocalPositionFromWorldPosition
            (this RectTransform rectTransform,
            Vector3 worldPosition,
            RectTransform rectTransformParent,
            RenderMode cameraRenderMode,
            Camera camera = null)
    {
        var screenPosition = camera.WorldToScreenPoint(worldPosition);

        Vector2 localPosition;
        bool isHit = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransformParent,
            screenPosition,
            cameraRenderMode == RenderMode.ScreenSpaceOverlay ? null : camera,
            out localPosition
        );

        if (!isHit) return false;
        rectTransform.localPosition = localPosition;
        return true;
    }
}
