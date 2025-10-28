using System.Collections.Generic;
using HarmonyLib;
using TranslateUs.Chat;

namespace TranslateUs.Patch;

public class ChatBubblePatch
{

    private static Dictionary<long, ChatBubbleMetadeta> chatBubbleMetadeta = new Dictionary<long, ChatBubbleMetadeta>();


    [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetText))]
    static class Patch1
    {
        static void Prefix(ChatBubble __instance, ref string chatText)
        {
            ChatBubbleMetadeta cbm = GetOrCreateMetadeta(__instance);
            if (!cbm.Debounce)
            {
                cbm.Text = chatText;
                cbm.IsTranslated = false;
                cbm.Version++;
            }
        }
    }

    [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.Reset))]
    static class Patch2
    {
        static void Prefix(ChatBubble __instance)
        {
            ChatBubbleMetadeta cbm = GetOrCreateMetadeta(__instance);
            cbm.Text = "";
            cbm.IsTranslated = false;
        }
    }

    public static ChatBubbleMetadeta GetOrCreateMetadeta(ChatBubble chatBubble)
    {
        long ptr = chatBubble.Pointer.ToInt64();
        if (chatBubbleMetadeta.ContainsKey(ptr))
        {
            return chatBubbleMetadeta[ptr];
        }

        ChatBubbleMetadeta cbm = new ChatBubbleMetadeta();
        chatBubbleMetadeta[ptr] = cbm;
        return cbm;
    }

    public static void RawSetText(ChatBubble chatBubble, string Text)
    {
        ChatBubbleMetadeta cbm = GetOrCreateMetadeta(chatBubble);
        cbm.Debounce = true;

        chatBubble.SetText(Text);

        cbm.Debounce = false;
    }
    
    public static void ResetTextToNormal(ChatBubble chatBubble)
    {
        RawSetText(chatBubble, GetOrCreateMetadeta(chatBubble).Text);
    }
}