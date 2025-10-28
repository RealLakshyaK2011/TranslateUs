using LibreTranslate.Net;

namespace TranslateUs.Language;

public class TranslationData
{
    public string TextToTranslate;
    public string TranslatedText;
    public LanguageCode TargetLanguage;
    public LanguageCode DetectedLanguage;
}