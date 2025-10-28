namespace TranslateUs.Chat;

public class ChatBubbleMetadeta
{
    public string Text { get; set; } = "";
    public bool IsTranslated { get; set; } = false;
    public bool Debounce { get; set; } = false;

    public int Version;
}