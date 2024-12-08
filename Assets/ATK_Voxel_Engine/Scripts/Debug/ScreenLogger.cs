using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System;

namespace ATKVoxelEngine
{
    public class ScreenLogger
    {
        const int MAX_LOGS = 15;
        const float LOG_DURATION = 15f;

        static VisualElement _logContainer;
        static Stack<ScreenLog> _logPool = new Stack<ScreenLog>();
        static Queue<ScreenLog> _logQueue = new Queue<ScreenLog>();
        static bool isInitialized = false;

        public ScreenLogger(UIDocument doc)
        {
            AddLabels(doc);
            isInitialized = true;
        }

        public static void AddToStack(ScreenLog log)
        {
            _logPool.Push(log);
            _logQueue.Dequeue();
            if (_logQueue.Count == 0)
                _logContainer.style.opacity = 0;
        }

        void AddLabels(UIDocument doc)
        {
            _logContainer = doc.rootVisualElement.Q<VisualElement>("LogContainer");
            Label[] labels = new Label[MAX_LOGS];
            for (int i = 0; i < MAX_LOGS; i++)
            {
                labels[i] = new Label();
                _logPool.Push(new ScreenLog(labels[i]));
                _logContainer.Add(labels[i]);
            }
        }

        static ScreenLog GetFreeLog()
        {
            if (!_logPool.TryPop(out ScreenLog freeLog))
                freeLog = _logQueue.Dequeue();

            freeLog.Label.SendToBack();

            // display the log container
            _logContainer.style.opacity = 1;

            return freeLog;
        }

        static string GetStyle(LogType logType)
        {
            switch (logType)
            {
                case LogType.Error: return "ErrorLog";
                case LogType.Assert: return "AssertLog";
                case LogType.Warning: return "WarningLog";
                case LogType.Exception: return "ExceptionLog";
                default: return "NormalLog";
            }
        }

        #region Loggers
        public static void Log(LogType logType, object message)
        {
            if (!isInitialized) return;
            ScreenLog freeLog = GetFreeLog();
            freeLog.Label.ClearClassList();
            freeLog.Label.AddToClassList(GetStyle(logType));
            freeLog.Log(message.ToString(), LOG_DURATION);
            _logQueue.Enqueue(freeLog);
        }

        public static void Log(LogType logType, object message, UnityEngine.Object context)
        {
            if (!isInitialized) return;
            ScreenLog freeLog = GetFreeLog();
            freeLog.Label.ClearClassList();
            freeLog.Label.AddToClassList(GetStyle(logType));
            freeLog.Log($"Object({context.name}): {message.ToString()}", LOG_DURATION);
            _logQueue.Enqueue(freeLog);
        }

        public static void Log(LogType logType, string tag, object message)
        {
            if (!isInitialized) return;
            ScreenLog freeLog = GetFreeLog();
            freeLog.Label.ClearClassList();
            freeLog.Label.AddToClassList(GetStyle(logType));
            freeLog.Log($"Tag({tag}): {message.ToString()}", LOG_DURATION);
            _logQueue.Enqueue(freeLog);
        }

        public static void Log(LogType logType, string tag, object message, UnityEngine.Object context)
        {
            if (!isInitialized) return;
            ScreenLog freeLog = GetFreeLog();
            freeLog.Label.ClearClassList();
            freeLog.Label.AddToClassList(GetStyle(logType));
            freeLog.Log($"Tag({tag}),Object({context.name}): {message.ToString()}", LOG_DURATION);
            _logQueue.Enqueue(freeLog);
        }

        public static void Log(object message)
        {
            if (!isInitialized) return;
            ScreenLog freeLog = GetFreeLog();
            freeLog.Label.ClearClassList();
            freeLog.Label.AddToClassList(GetStyle(LogType.Log));
            freeLog.Log($"{message.ToString()}", LOG_DURATION);
            _logQueue.Enqueue(freeLog);
        }

        public static void Log(string tag, object message)
        {
            if (!isInitialized) return;
            ScreenLog freeLog = GetFreeLog();
            freeLog.Label.ClearClassList();
            freeLog.Label.AddToClassList(GetStyle(LogType.Log));
            freeLog.Log($"Tag({tag}): {message.ToString()}", LOG_DURATION);
            _logQueue.Enqueue(freeLog);
        }

        public static void Log(string tag, object message, UnityEngine.Object context)
        {
            if (!isInitialized) return;
            ScreenLog freeLog = GetFreeLog();
            freeLog.Label.ClearClassList();
            freeLog.Label.AddToClassList(GetStyle(LogType.Log));
            freeLog.Log($"Tag({tag}),Object({context.name}): {message.ToString()}", LOG_DURATION);
            _logQueue.Enqueue(freeLog);
        }

        public static void LogWarning(string tag, object message)
        {
            if (!isInitialized) return;
            ScreenLog freeLog = GetFreeLog();
            freeLog.Label.ClearClassList();
            freeLog.Label.AddToClassList(GetStyle(LogType.Warning));
            freeLog.Log($"Tag({tag}): {message.ToString()}", LOG_DURATION);
            _logQueue.Enqueue(freeLog);
        }

        public static void LogWarning(string tag, object message, UnityEngine.Object context)
        {
            if (!isInitialized) return;
            ScreenLog freeLog = GetFreeLog();
            freeLog.Label.ClearClassList();
            freeLog.Label.AddToClassList(GetStyle(LogType.Warning));
            freeLog.Log($"Tag({tag}),Object({context.name}): {message.ToString()}", LOG_DURATION);
            _logQueue.Enqueue(freeLog);
        }

        public static void LogError(string tag, object message)
        {
            if (!isInitialized) return;
            ScreenLog freeLog = GetFreeLog();
            freeLog.Label.ClearClassList();
            freeLog.Label.AddToClassList(GetStyle(LogType.Error));
            freeLog.Log($"Tag({tag}): {message.ToString()}", LOG_DURATION);
            _logQueue.Enqueue(freeLog);
        }

        public static void LogError(string tag, object message, UnityEngine.Object context)
        {
            if (!isInitialized) return;
            ScreenLog freeLog = GetFreeLog();
            freeLog.Label.ClearClassList();
            freeLog.Label.AddToClassList(GetStyle(LogType.Error));
            freeLog.Log($"Tag({tag}),Object({context.name}): {message.ToString()}", LOG_DURATION);
            _logQueue.Enqueue(freeLog);
        }

        public static void LogException(Exception exception)
        {
            if (!isInitialized) return;
            ScreenLog freeLog = GetFreeLog();
            freeLog.Label.ClearClassList();
            freeLog.Label.AddToClassList(GetStyle(LogType.Exception));
            freeLog.Log($"Exception({exception.Source}|{exception.Data}): {exception.Message}", LOG_DURATION);
            _logQueue.Enqueue(freeLog);
        }

        public static void LogException(Exception exception, UnityEngine.Object context)
        {
            if (!isInitialized) return;
            ScreenLog freeLog = GetFreeLog();
            freeLog.Label.ClearClassList();
            freeLog.Label.AddToClassList(GetStyle(LogType.Exception));
            freeLog.Log($"Exception({exception.Source}|{exception.Data}),Object({context.name}): {exception.Message}", LOG_DURATION);
            _logQueue.Enqueue(freeLog);
        }
        #endregion
    }
}