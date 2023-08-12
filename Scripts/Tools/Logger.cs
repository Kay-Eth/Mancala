using Godot;
using Godot.Collections;
using System.Diagnostics;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

public static class Logger
{
    private static LogLevel _logLevel = LogLevel.Debug;

    private static int _logNumber = 0;

    const string LOG_FILE_DIR = "user://GameLogs";
    const string LOG_FILE_PATH = LOG_FILE_DIR + "/log_file_";
    const string LOG_FILE_EXT = ".log";
    const int NUMBER_OF_LOG_FILES = 10;

    private static string _logFilePath = "";

    private static bool Initialized {
        get
        {
            if (_initialized)
                return true;
            
            if (!Engine.EditorHint)
            {
                Directory dir = new Directory();
                if (!dir.DirExists(LOG_FILE_DIR))
                {
                    Error err = dir.MakeDirRecursive(LOG_FILE_DIR);
                    if (err != Godot.Error.Ok)
                    {
                        GD.PushError("Log file directory creation failed: " + err.ToString());
                        return false;
                    }
                }

                Dictionary date = OS.GetDatetime();

                string year = GD.Str(date["year"]);
                string month = (int)date["month"] > 9 ? GD.Str(date["month"]) : GD.Str(0, date["month"]);
                string day = (int)date["day"] > 9 ? GD.Str(date["day"]) : GD.Str(0, date["day"]);

                string hour = (int)date["hour"] > 9 ? GD.Str(date["hour"]) : GD.Str(0, date["hour"]);
                string minute = (int)date["minute"] > 9 ? GD.Str(date["minute"]) : GD.Str(0, date["minute"]);
                string second = (int)date["second"] > 9 ? GD.Str(date["second"]) : GD.Str(0, date["second"]);

                _logFilePath = LOG_FILE_PATH + year + "_" + month + "_" + day + "_" + hour + "_" + minute + "_" + second + LOG_FILE_EXT;

                File file = new File();
                var error = file.Open(_logFilePath, File.ModeFlags.Write);
                if (error == Godot.Error.Ok)
                {
                    file.StoreString("Date\tLogNumber\tLogLevel\t[Caller|Method]\tMessage\n");
                    file.Close();
                }
                else
                {
                    GD.PushError("Log file save failed: " + error.ToString());
                    return false;
                }

                _initialized = true;
                Info("Logger successfully initialized.");

                if (RemoveOldLogs())
                {
                    Info("Removed old log files.");
                }
            }
            else
            {
                _initialized = true;
            }

            return true;
        }
    }

    private static bool _initialized = Initialized;

    public static void Debug(object message, bool showPrefix = true)
    {
        Log(message, LogLevel.Debug, showPrefix);
    }

    public static void Info(object message, bool showPrefix = true)
    {
        Log(message, LogLevel.Info, showPrefix);
    }

    public static void Warning(object message, bool showPrefix = true)
    {
        Log(message, LogLevel.Warning, showPrefix);
    }

    public static void Error(object message, bool showPrefix = true)
    {
        Log(message, LogLevel.Error, showPrefix);
    }

    public static void Fatal(object message, int errorCode = 1, bool showPrefix = true)
    {
        Log(message, LogLevel.Fatal, showPrefix);
        System.Environment.Exit(errorCode);
    }

    private static void Log(object message, LogLevel logLevel = LogLevel.Debug, bool showPrefix = true)
    {
        #if DEBUG
            if (!_initialized)
            {
                GD.PushError("Logger is not initiated!");
                return;
            }

            StackFrame frame = new StackFrame(2);
            if (Engine.EditorHint)
                GD.Print($"[{logLevel}]\t[{frame.GetMethod().DeclaringType.Name}|{frame.GetMethod().Name}]\t{message}");
            else
            {
                LogMessage(message.ToString(), frame.GetMethod().Name, frame.GetMethod().DeclaringType.Name, showPrefix, logLevel);
            }
        #endif
    }

    public static LogLevel GetLogLevel()
    {
        return _logLevel;
    }

    public static void SetLogLevel(LogLevel level)
    {
        _logLevel = level;
    }

    private static void LogMessage(string message, string methodName, string callerName, bool showPrefix = true, LogLevel logLevel = LogLevel.Debug)
    {
        if (_logLevel > logLevel)
            return;

        string result = "";

        if (showPrefix)
        {
            Godot.Collections.Dictionary date = OS.GetDatetime();

            string year = GD.Str(date["year"]);
            string month = (int)date["month"] > 9 ? GD.Str(date["month"]) : GD.Str(0, date["month"]);
            string day = (int)date["day"] > 9 ? GD.Str(date["day"]) : GD.Str(0, date["day"]);

            string hour = (int)date["hour"] > 9 ? GD.Str(date["hour"]) : GD.Str(0, date["hour"]);
            string minute = (int)date["minute"] > 9 ? GD.Str(date["minute"]) : GD.Str(0, date["minute"]);
            string second = (int)date["second"] > 9 ? GD.Str(date["second"]) : GD.Str(0, date["second"]);

            result += GD.Str("[", year, "-", month, "-", day, " ", hour, ":", minute, ":", second, "]\t");

            result += GD.Str("[", _logNumber++ ,"]\t");

            switch (logLevel)
            {
                case LogLevel.Debug:
                    result += "[DEBUG]\t";
                    break;
                case LogLevel.Info:
                    result += "[INFO]\t";
                    break;
                case LogLevel.Warning:
                    result += "[WARN]\t";
                    break;
                case LogLevel.Error:
                    result += "[ERROR]\t";
                    break;
                case LogLevel.Fatal:
                    result += "[FATAL]\t";
                    break;
            }

            result += GD.Str("[", callerName, "|", methodName, "]\t");
        }

        message = message.Replace("\\r\\n", "\n");
        message = message.Replace("\\n", "\n");
        message = message.Replace("\\t", "\t");
        
        result += message;

        GD.Print(result);

        if (logLevel > LogLevel.Info)
        {
            string stack = new System.Diagnostics.StackTrace().ToString();
            stack = stack.Remove(0, 4);
            stack = stack.Remove(0, stack.Find("at") + 2);
            stack = stack.Remove(0, stack.Find("  at"));

            GD.Print(stack);
        }

        File file = new File();
        var error = file.Open(_logFilePath, File.ModeFlags.ReadWrite);
        if (error == Godot.Error.Ok)
        {
            file.SeekEnd();
            file.StoreString(result + "\n");
            file.Close();
        }
        else
        {
            GD.PushError($"Log file ({_logFilePath}) save failed: " + error.ToString());
        }
    }

    public static bool RemoveOldLogs()
    {
        if (!_initialized)
        {
            GD.PushError("Logger is not initiated!");
            return false;
        }

        Directory dir = new Directory();
        Error error = dir.Open(LOG_FILE_DIR);
        if (error != Godot.Error.Ok)
        {
            Error($"Cannot open log files directory: {error.ToString()}");
            return false;
        }

        error = dir.ListDirBegin(true, true);
        if (error != Godot.Error.Ok)
        {
            Error($"Cannot list log files: {error.ToString()}");
            return false;
        }

        System.Collections.Generic.List<string> logFiles = new System.Collections.Generic.List<string>();
        string logFile;
        while (!(logFile = dir.GetNext()).Empty())
        {
            if (logFile.EndsWith(LOG_FILE_EXT))
                logFiles.Add(logFile);
        }

        dir.ListDirEnd();

        logFiles.Sort();

        while (logFiles.Count > NUMBER_OF_LOG_FILES)
        {
            error = dir.Remove(logFiles[0]);
            if (error != Godot.Error.Ok)
            {
                Error($"Cannot delete log file \"{logFiles[0]}\": {error.ToString()}");
                return false;
            }
            logFiles.RemoveAt(0);
        }

        return true;
    }
}
