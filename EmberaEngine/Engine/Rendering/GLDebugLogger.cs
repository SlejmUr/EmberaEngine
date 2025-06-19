using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.InteropServices;

namespace EmberaEngine.Engine.Utilities
{
    public static class GLDebugLogger
    {
        private static DebugProc debugDelegate = DebugCallback;

        public static void Enable()
        {
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GL.DebugMessageCallback(debugDelegate, IntPtr.Zero);

            // Optional: filter out noisy or low-severity messages
            GL.DebugMessageControl(DebugSourceControl.DontCare,
                                   DebugTypeControl.DontCare,
                                   DebugSeverityControl.DebugSeverityNotification,
                                   0, Array.Empty<int>(), false);

            Console.WriteLine("[OpenGL] Debug Logger Enabled.");
        }

        private static void DebugCallback(DebugSource source,
                                          DebugType type,
                                          int id,
                                          DebugSeverity severity,
                                          int length,
                                          IntPtr messagePtr,
                                          IntPtr userParam)
        {
            string message = Marshal.PtrToStringAnsi(messagePtr, length);

            string log = $"[OpenGL Debug] Source: {source}, Type: {type}, ID: {id}, Severity: {severity}\n→ {message}";

            switch (severity)
            {
                case DebugSeverity.DebugSeverityHigh:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(log);
                    break;

                case DebugSeverity.DebugSeverityMedium:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(log);
                    break;

                case DebugSeverity.DebugSeverityLow:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(log);
                    break;

                case DebugSeverity.DebugSeverityNotification:
                    Console.ResetColor();
                    Console.WriteLine($"[OpenGL Notice] {message}");
                    break;

                default:
                    Console.ResetColor();
                    Console.WriteLine(log);
                    break;
            }

            Console.ResetColor();
        }
    }
}
