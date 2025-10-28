using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using LibreTranslate.Net;
using Newtonsoft.Json;
using UnityEngine;

namespace TranslateUs.Language;

public class ChatTranslator
{

    public LibreTranslate.Net.LibreTranslate translator;
    public HttpClient HttpClient = new HttpClient{BaseAddress = new Uri("http://127.0.0.1:5000") };

    public void GetTranslationsAndPerform(string TextToTranslate, LanguageCode[] targetLanguages, TranslationPostAction.PostTranslateAction translationPostAction, LanguageCode src = null)
    {
        TranslateUsPlugin.LogLine("Got a translation batch with total targets: " + targetLanguages.Length);
        Task<TranslationResponse>[] translationTasks = new Task<TranslationResponse>[targetLanguages.Length];
        TranslationData[] datas = new TranslationData[targetLanguages.Length];

        for (int i = 0; i < targetLanguages.Length; i++)
        {
            TranslationData data = new TranslationData();
            data.TextToTranslate = TextToTranslate;
            data.TargetLanguage = targetLanguages[i];

            LanguageCode srcL = LanguageCode.AutoDetect;
            if(src != null)
            {
                srcL = src;
            }
            translationTasks[i] = translator.TranslateAsync(new Translate()
            {
                Text = data.TextToTranslate,
                Source = srcL,
                Target = data.TargetLanguage
            });

            TranslateUsPlugin.LogLine("Created async request for text: " + TextToTranslate + ", and language: " + data.TargetLanguage.ToString());

            
            datas[i] = data;
            
        }

        Helper(translationTasks, datas, translationPostAction);
    }
    
    private async Task Helper(Task<TranslationResponse>[] tasks, TranslationData[] datas, TranslationPostAction.PostTranslateAction postTranslateAction)
    {
        for (int i = 0; i < tasks.Length; i++)
        {
            TranslateUsPlugin.LogLine("Awaiting!");
            try
            {
                TranslationResponse res = await tasks[i];
                var translatedText = res.translatedText;
                datas[i].TranslatedText = translatedText;
                if(res.detectedLanguage != null)
                {
                    var detectedLanguage = LanguageCode.FromString(res.detectedLanguage.language);
                    datas[i].DetectedLanguage = detectedLanguage;
                }
            }
            catch(Exception e)
            {
                TranslateUsPlugin.LogLine("Caught an exception! Message: " + e.Message);
                TranslateUsPlugin.LogLine("StackTrace:\n" + e.StackTrace);
            }
            
            TranslateUsPlugin.LogLine("Await end!");
        }

        TranslateUsPlugin.LogLine("Performing PostTranslateAction");
        postTranslateAction(datas);
    }

    public ChatTranslator()
    {
        translator = new LibreTranslate.Net.LibreTranslate("http://127.0.0.1:5000");
    }

}