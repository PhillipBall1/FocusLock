﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FocusLock.Service
{
    public static class ProcessTreeHelper
    {
        // Struct for process info returned by Windows API
        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public int th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public int th32ModuleID;
            public uint cntThreads;
            public int th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        }

        private const uint TH32CS_SNAPPROCESS = 0x00000002;

        // Windows API functions for process snapshot and enumeration
        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll")]
        private static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll")]
        private static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        // Gets the full list of processes that are descendants of the given parent process ID.
        public static List<Process> GetProcessTree(int parentId)
        {
            var result = new List<Process>();
            var queue = new Queue<int>();
            var visited = new HashSet<int>();

            queue.Enqueue(parentId);

            var pidToParent = new Dictionary<int, int>();

            IntPtr snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
            if (snapshot == IntPtr.Zero)
                return result;

            var entry = new PROCESSENTRY32 { dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32)) };
            if (!Process32First(snapshot, ref entry))
                return result;

            // Build map of process ID -> parent process ID
            do
            {
                pidToParent[entry.th32ProcessID] = entry.th32ParentProcessID;
            }
            while (Process32Next(snapshot, ref entry));

            // BFS traversal to get all descendants
            while (queue.Count > 0)
            {
                int pid = queue.Dequeue();

                if (visited.Contains(pid))
                    continue;

                visited.Add(pid);

                try
                {
                    var proc = Process.GetProcessById(pid);
                    result.Add(proc);
                }
                catch
                {
                    // Process may have exited, ignore exceptions
                }

                // Enqueue child processes
                foreach (var kv in pidToParent)
                {
                    if (kv.Value == pid && !visited.Contains(kv.Key))
                        queue.Enqueue(kv.Key);
                }
            }
            return result;
        }

        // Gets a dictionary mapping process IDs to their parent process IDs.
        public static Dictionary<int, int> GetParentPidMap()
        {
            var map = new Dictionary<int, int>();

            var snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
            if (snapshot == IntPtr.Zero) return map;

            try
            {
                var procEntry = new PROCESSENTRY32();
                procEntry.dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32));

                if (Process32First(snapshot, ref procEntry))
                {
                    do
                    {
                        map[(int)procEntry.th32ProcessID] = (int)procEntry.th32ParentProcessID;
                    } while (Process32Next(snapshot, ref procEntry));
                }
            }
            finally
            {
                CloseHandle(snapshot);
            }

            return map;
        }

        // Returns the root process by walking up the parent chain,
        // stopping if it hits the explorer process or a max depth to avoid cycles.
        public static Process GetRootProcess(Process process)
        {
            var parentMap = GetParentPidMap();

            const int MaxDepth = 10;
            int depth = 0;

            try
            {
                while (parentMap.TryGetValue(process.Id, out int parentId) && parentId > 4 && depth < MaxDepth)
                {
                    try
                    {
                        Process parent = Process.GetProcessById(parentId);

                        if (parent == null || parent.ProcessName == "explorer") break;

                        process = parent;
                        depth++;
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            catch
            {
                // Ignore exceptions
            }
            return process;
        }
    }
}
