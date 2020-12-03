﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotBase.Enums
{
    public enum eSettings
    {
        /// <summary>
        /// How often could a form navigate to another (within one user action/call/message)
        /// </summary>
        NavigationMaximum = 1,


        /// <summary>
        /// Loggs all messages and sent them to the event handler
        /// </summary>
        LogAllMessages = 2,


        /// <summary>
        /// Skips all messages during running (good for big delay updates)
        /// </summary>
        SkipAllMessages = 3,


        /// <summary>
        /// Does stick to the console event handler and saves all sessions on exit.
        /// </summary>
        SaveSessionsOnConsoleExit = 4



    }
}
