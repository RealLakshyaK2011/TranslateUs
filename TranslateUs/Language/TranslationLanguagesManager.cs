using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LibreTranslate.Net;

namespace TranslateUs.Language;

public class TranslationLanguagesManager
{
    private TcpListener configEndpoint;
    private LanguageCode MyLanguage = LanguageCode.English;
    private ArrayList TargetLanguages = new ArrayList();

    private readonly object tarLock = new object();
    private readonly object myLock = new object();

    public LanguageCode[] GetTargetLanguages()
    {
        lock(tarLock)
        {
            LanguageCode[] targets = new LanguageCode[TargetLanguages.Count];
            int i = 0;
            foreach (LanguageCode tar in TargetLanguages)
            {
                targets[i] = tar;
                i++;
            }

            return targets;
        }
    }

    public LanguageCode GetMyLanguage()
    {
        lock(myLock)
        {
            return MyLanguage;
        }
    }

    public TranslationLanguagesManager()
    {
        configEndpoint = new TcpListener(IPAddress.Loopback, 5001);
        configEndpoint.Start();
        

        new Thread(() =>
        {
            while (true)
            {
                try
                {
                    TranslateUsPlugin.LogLine("Accepting...");
                    TcpClient cl = configEndpoint.AcceptTcpClient();
                    TranslateUsPlugin.LogLine("Acceted!");

                    NetworkStream stream = cl.GetStream();
                    int mode = stream.ReadByte();
                    int add = 0;
                    string msg = "Success!";
                    bool err = false;
                    string lang = "";
                    LanguageCode code = null;

                    if (mode == 0) // change my language
                    {
                        lang = Encoding.ASCII.GetString([((byte)stream.ReadByte()), ((byte)stream.ReadByte())]);
                    }
                    else //add or remove tar language
                    {
                        add = stream.ReadByte();
                        lang = Encoding.ASCII.GetString([((byte)stream.ReadByte()), ((byte)stream.ReadByte())]);
                    }

                    try
                    {
                        code = LanguageCode.FromString(lang);
                    }
                    catch (Exception e)
                    {
                        err = true;
                        msg = "Invalid/Unsupported language provided";
                    }

                    if (err)
                    {
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        cl.Close();
                        continue;
                    }

                    if (mode == 0)
                    {
                        lock (myLock)
                        {
                            MyLanguage = code;
                        }
                    }
                    else
                    {
                        lock (tarLock)
                        {
                            if (add == 0) // remove
                            {
                                if (TargetLanguages.Contains(code))
                                {
                                    TargetLanguages.Remove(code);
                                }
                                else
                                {
                                    msg = "Target languages doesn't contain the provided language!";
                                }
                            }
                            else
                            {
                                if (TargetLanguages.Contains(code))
                                {
                                    msg = "Target languages already contains the provided language!";
                                }
                                else
                                {
                                    TargetLanguages.Add(code);
                                }
                            }
                        }
                    }


                    stream.Write(Encoding.ASCII.GetBytes(msg + "\n"), 0, msg.Length + 1);
                    cl.Close();
                }
                catch(Exception e)
                {
                    TranslateUsPlugin.LogLine("Exception: " + e.Message);
                    TranslateUsPlugin.LogLine("StackTrace: \n" + e.StackTrace);
                }
            }
            
        }).Start();
    }
}