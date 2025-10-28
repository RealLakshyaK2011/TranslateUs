using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using GameCore;
using HarmonyLib;
using Reactor;
using System;
using System.Diagnostics;

using System.Net.Sockets;
using TranslateUs.Language;
using UnityEngine;
using UnityEngine.UI;

namespace TranslateUs;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
public partial class TranslateUsPlugin : BasePlugin
{

    private static readonly bool DEBUG = false;
    private static TranslateUsPlugin instance;
    private ChatTranslator translator;
    private TranslationLanguagesManager translationLanguagesManager;
    public Harmony Harmony { get; } = new(Id);
    TcpClient cl = null;

    public override void Load()
    {
        instance = this;
        if (DEBUG)
        {
            cl = new TcpClient("127.0.0.1", 20110);
        }
        ProcessStartInfo inf = new ProcessStartInfo()
        {
            FileName = "java",
            Arguments = "TranslationLanguagesManager.java",
            UseShellExecute = true
        };

        ProcessStartInfo inf2 = new ProcessStartInfo()
        {
            FileName = "libretranslate", CreateNoWindow = true, UseShellExecute = false
        };

        Process libretranslate = Process.Start(inf2);
        Process LanguageManager = Process.Start(inf);

        LanguageManager.EnableRaisingEvents = true;
        EventHandler callback = null;
        callback = (o, b) =>
        {
            LanguageManager = Process.Start(inf);
            LanguageManager.EnableRaisingEvents = true;
            LanguageManager.Exited += callback;
        };

        LanguageManager.Exited += callback;

        translator = new ChatTranslator();
        translationLanguagesManager = new TranslationLanguagesManager();
        Harmony.PatchAll();

        LogLine("Plugin Loaded!");
    }

    public static void LogLine(string line)
    {
        if (DEBUG)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(line + "\n");
            GetInstance().cl.GetStream().WriteAsync(data, 0, data.Length);
        }
        System.Console.WriteLine(line);
    }

    public static TranslateUsPlugin GetInstance()
    {
        return instance;
    }

    public ChatTranslator GetChatTranslator()
    {
        return translator;
    }

    public TranslationLanguagesManager GetTranslationLanguagesManager()
    {
        return translationLanguagesManager;
    }
}
