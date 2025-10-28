using System;
using System.Collections;
using System.Text;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using LibreTranslate.Net;
using Sentry.Unity.NativeUtils;
using TranslateUs.Chat;
using TranslateUs.Helper;
using TranslateUs.Language;
using UnityEngine;

namespace TranslateUs.Patch;

public class ChatControllerPatch
{

    private static ArrayList actionList = new ArrayList();
    private static bool rawSendFreeChat = false;
    private static bool cancelSendChat = false;
    private static float savedTimeSinceLastMessage = 0f;

    private static readonly object lockObj = new object();

    public static void RawSendFreeChat(ChatController chatController)
    {
        rawSendFreeChat = true;
        chatController.SendChat();
    }

    public static void AddAction(Action action)
    {
        lock (lockObj)
        {
            actionList.Add(action);
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    static class Patch1
    {
        static bool Prefix(ChatController __instance)
        {
            TranslateUsPlugin.LogLine("Called!");
            if (cancelSendChat)
            {
                TranslateUsPlugin.LogLine("Cancelled!");
                return false; 
            }

            if (rawSendFreeChat)
            {
                savedTimeSinceLastMessage = __instance.timeSinceLastMessage;
                __instance.timeSinceLastMessage = ChatController.MAX_CHAT_SEND_RATE + 1.0f;
                return true;
            }

            //Let natural warning behaivour occur
            if (__instance.timeSinceLastMessage < (ChatController.MAX_CHAT_SEND_RATE - 0.001f))
            {
                return true;
            }

            //Sending logic here!
            string SendingText = __instance.freeChatField.Text;
            TranslateUsPlugin plugin = TranslateUsPlugin.GetInstance();
            LanguageCode[] targetLanguages = plugin.GetTranslationLanguagesManager().GetTargetLanguages();
            if (targetLanguages.Length == 0) return true;

            plugin.GetChatTranslator().GetTranslationsAndPerform(SendingText, targetLanguages,
            (translations) =>
            {
                AddAction(() =>
                {
                    //string savedMessage = __instance.freeChatField.Text;

                    //Send original
                    StringBuilder builder = new StringBuilder();
                    builder.Append(SendingText);



                    foreach (TranslationData td in translations)
                    {
                        if (td.TranslatedText != null && !td.TranslatedText.Trim().Equals(""))
                        {
                            builder.Append("\n" + td.TargetLanguage.ToString() + ": " + td.TranslatedText);
                        }

                    }

                    //__instance.freeChatField.textArea.SetText(builder.ToString());
                    // RawSendFreeChat(__instance);
                    cancelSendChat = true;
                    TranslateUsPlugin.LogLine("sending rpc");
                    PlayerControl.LocalPlayer.RpcSendChat(builder.ToString());
                    TranslateUsPlugin.LogLine("sending rpc end");
                    cancelSendChat = false;


                    //__instance.freeChatField.textArea.SetText(savedMessage);

                    __instance.timeSinceLastMessage = 0.0f;
                });
            });

            __instance.freeChatField.textArea.SetText("");

            return false;
        }

        static void Postfix(ChatController __instance)
        {
            if (rawSendFreeChat)
            {
                __instance.timeSinceLastMessage = savedTimeSinceLastMessage;
                rawSendFreeChat = false;
            }
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
    static class Patch2
    {
        static void Prefix(ChatController __instance)
        {

            Il2CppSystem.Collections.Generic.List<PoolableBehavior> activeChatBubbles = __instance.chatBubblePool.activeChildren;

            foreach (PoolableBehavior pb in activeChatBubbles)
            {
                ChatBubble chatBubble = pb.TryCast<ChatBubble>();
                if (chatBubble != null && chatBubble.playerInfo != null)
                {
                    PlayerControl plr = PlayerControlHelper.GetPlayerControlById(chatBubble.playerInfo.PlayerId);
                    if (plr == null) return;
                    if (!plr.AmOwner)
                    {
                        //Translation logic here
                        ChatBubbleMetadeta cbm = ChatBubblePatch.GetOrCreateMetadeta(chatBubble);
                        if (!cbm.IsTranslated && !cbm.Text.Trim().Equals(""))
                        {
                            //Change this to the actual translated text
                            int oldVer = cbm.Version;
                            TranslateUsPlugin plugin = TranslateUsPlugin.GetInstance();

                            plugin.GetChatTranslator()
                            .GetTranslationsAndPerform(cbm.Text, [plugin.GetTranslationLanguagesManager().GetMyLanguage()], (translation) =>
                            {
                                AddAction(() =>
                                {
                                    if (chatBubble == null) return;
                                    if (oldVer == cbm.Version)
                                    {
                                        LanguageCode dl = translation[0].DetectedLanguage;
                                        string tt = translation[0].TranslatedText;
                                        if (tt == null)
                                        {
                                            cbm.IsTranslated = false;
                                        }

                                        string txtToSet = cbm.Text + "\n\nTranslation (Detected language: " + (dl == null ? "unknown" : dl.ToString()) + "):\n" + (tt == null ? "Error while translating!" : tt);
                                        TranslateUsPlugin.LogLine("Translated:\n" + txtToSet);
                                        ChatBubblePatch.RawSetText(chatBubble, txtToSet);
                                        chatBubble.AlignChildren();
                                        __instance.AlignAllBubbles();
                                    }
                                });
                                
                            });

                            cbm.IsTranslated = true;
                        }
                    }
                }

            }
            
            lock(lockObj)
            {
                foreach (object o in actionList)
                {
                    Action action = (Action)o;
                    action();
                }
                __instance.freeChatField.textArea.allowAllCharacters = true;
                actionList = new ArrayList();
            }
        }
    }
}