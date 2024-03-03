﻿namespace Common.Stun
{
    public static class Logger
    {
        public static void Log(string message)
        {
            Console.WriteLine($"[thread_id: {Thread.CurrentThread.ManagedThreadId}] {message}");
        }
    }
}
