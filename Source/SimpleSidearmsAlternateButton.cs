﻿using System;
using System.Linq;
using System.Reflection;

using HarmonyLib;
using RimWorld;
using Verse;

namespace GD.SSAB
{
    [StaticConstructorOnStartup]
    public static class SSAB
    {
        public static string Author => "GonDragon";
        public static string Name => Assembly.GetName().Name;
        public static string Id => Author + "." + Name;

        public static string Version => Assembly.GetName().Version.ToString();

        private static Assembly Assembly
        {
            get
            {
                return Assembly.GetAssembly(typeof(SSAB));
            }
        }

        public static readonly Harmony Harmony;
        static SSAB()
        {
            Harmony = new Harmony(Id);
            Harmony.PatchAll();
            SSAB.Log("Mod ready");
        }

        public static void Log(string message) => Verse.Log.Message(PrefixMessage(message));
        public static void Warning(string message) => Verse.Log.Warning(PrefixMessage(message));
        public static void Error(string message) => Verse.Log.Error(PrefixMessage(message));
        public static void ErrorOnce(string message, string key) => Verse.Log.ErrorOnce(PrefixMessage(message), key.GetHashCode());
        public static void Message(string message) => Messages.Message(message, MessageTypeDefOf.TaskCompletion, false);
        private static string PrefixMessage(string message) => $"[{Name} v{Version}] {message}";
    }
}