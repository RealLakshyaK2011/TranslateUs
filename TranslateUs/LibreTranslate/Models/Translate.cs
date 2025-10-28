namespace LibreTranslate.Net
{
    /// <summary>
    /// The model to send to the libre translate api
    /// </summary>
    public class Translate
    {
        /// <summary>
        /// The text to be translated
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// The source of the current language text
        /// </summary>
        public LanguageCode Source { get; set; }
        /// <summary>
        /// The target of the language we want to convert text
        /// </summary>
        public LanguageCode Target { get; set; }
        /// <summary>
        /// The libre translate api key
        /// </summary>
        public string ApiKey { get; set; }
    }
}
