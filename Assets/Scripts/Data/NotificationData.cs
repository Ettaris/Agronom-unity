using UnityEngine;

public struct NotificationData
{
    public string Message;
    public Sprite Icon;
    public Color Color;
    public float Duration; // время отображения

    public NotificationData(string message, Sprite icon = null, Color? color = null, float duration = 2.5f)
    {
        Message = message;
        Icon = icon;
        Color = color ?? Color.white;
        Duration = duration;
    }
}