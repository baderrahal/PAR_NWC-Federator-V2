namespace Federator.Core.Rerun
{
    /// <summary>
    /// One child window of a message box, as the add-in reads it off Windows: its class, its control id and its text.
    /// Plain values, so the rule of which box is answered, AppendBox, is proved with no window at all.
    /// </summary>
    public sealed class BoxPart
    {
        public BoxPart(string windowClass, int controlId, string text)
        {
            WindowClass = windowClass ?? string.Empty;
            ControlId = controlId;
            Text = text ?? string.Empty;
        }

        public string WindowClass { get; private set; }

        public int ControlId { get; private set; }

        public string Text { get; private set; }
    }
}
