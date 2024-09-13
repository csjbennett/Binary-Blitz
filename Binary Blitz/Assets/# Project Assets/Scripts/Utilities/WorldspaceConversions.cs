using UnityEngine;

/// <summary>
/// A class for easier shorthand of commonly used camera transform conversions
/// </summary>

namespace HomebrewUtilities.WorldspaceConversions
{
    public static class WorldspaceConversions
    {
        public static Vector2 ScreenToWorld(Vector2 screenPosition, Camera camera)
        {
            return camera.ScreenToWorldPoint((Vector3)screenPosition + (Vector3.forward * 10f));
        }
        public static Vector2 WorldToScreen(Vector2 worldPosition, Camera camera)
        {
            return camera.WorldToScreenPoint(worldPosition);
        }
    }
}
