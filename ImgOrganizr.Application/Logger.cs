using System;
using System.IO;
using ImgOrganizr.Application;

public class Logger
{
    public event Action<LogMessage> MessageLogged = delegate { };

    public void Log(LogMessage logMessage)
    {
        // Raise event
        MessageLogged(logMessage);
    }
}
