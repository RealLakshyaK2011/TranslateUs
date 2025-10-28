namespace LibreTranslate.Net
{
    /// <summary>
    /// The model for the translation api response
    /// </summary>
    public class TranslationResponse
    {
        /// <summary>
        /// The translated text
        /// </summary>
        public string translatedText { get; set; }
        public DetectedLanguage detectedLanguage { get; set; }
    }

    public class DetectedLanguage
    {
        public float confidence { get; set; }
        public string language { get; set; }
    }
}
