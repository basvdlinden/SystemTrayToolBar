using System.Reflection;

namespace SystemTrayToolBar;

internal static class NotifyIconExtensionMethods
{
    internal static void ShowContextMenu(this NotifyIcon notifyIcon)
    {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        mi.Invoke(notifyIcon, null);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}
