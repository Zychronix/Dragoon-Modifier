﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Dragoon_Modifier {
    public class Emu {
        const uint versionAddr = 0x9E19;
        const uint versionStringLen = 11;
        const string AoBCheck = "50 53 2D 58 20 45 58 45";
        const string duckstationCheck = "53 6F 6E 79 20 43 6F 6D 70 75 74 65 72 20 45 6E 74 65 72 74 61 69 6E 6D 65 6E 74 20 49 6E 63";

        static readonly int[] duckstationOffsets = new int[] { 0x110, 0x118 };
        static readonly Dictionary<string, Region> versions = new Dictionary<string, Region> {
            { "SCUS_944.91", Region.NTA },
            { "SCUS_945.84", Region.NTA },
            { "SCUS_945.85", Region.NTA },
            { "SCUS_945.86", Region.NTA },

            { "SCPS_101.19", Region.JPN },
            { "SCPS_101.20", Region.JPN },
            { "SCPS_101.21", Region.JPN },
            { "SCPS_101.22", Region.JPN },

            { "SCES_030.43", Region.ENG },
            { "SCES_130.43", Region.ENG },
            { "SCES_230.43", Region.ENG },
            { "SCES_330.43", Region.ENG },

            { "SCES_030.44", Region.FRN },
            { "SCES_130.44", Region.FRN },
            { "SCES_230.44", Region.FRN },
            { "SCES_330.44", Region.FRN },

            { "SCES_030.45", Region.GER },
            { "SCES_130.45", Region.GER },
            { "SCES_230.45", Region.GER },
            { "SCES_330.45", Region.GER },

            { "SCES_030.46", Region.ITL },
            { "SCES_130.46", Region.ITL },
            { "SCES_230.46", Region.ITL },
            { "SCES_330.46", Region.ITL },

            { "SCES_030.47", Region.SPN },
            { "SCES_130.47", Region.SPN },
            { "SCES_230.47", Region.SPN },
            { "SCES_330.47", Region.SPN },

        };
        
        IntPtr _processHandle;
        long _emulatorOffset = 0;
        Region _region;
        Dictionary<string, int> _regionalAddresses;

        //public MemoryController.MemoryController MemoryController { get; private set; }
        //public Battle.Battle BattleController { get; private set; }

        public Emu(string emulatorName) {
            _emulatorOffset = Constants.OFFSET; // TODO load previous offset

            if (emulatorName.ToLower().Contains(".exe")) {
                emulatorName = emulatorName.Replace("exe", "");
            }

            Process proc = FindEmulatorProcess(emulatorName);

            _processHandle = ProcessMemory.GetProcessHandle(proc);

            if (!Emulators(proc, emulatorName)) {
                throw new EmulatorAttachException();
            }

            _regionalAddresses = LoadRegionalAddresses(_region);

        }

        #region Byte

        public byte ReadByte(long address) {
            byte[] buffer = new byte[1];
            ProcessMemory.ReadProcessMemory(_processHandle, address + _emulatorOffset, buffer, 1, out long lpNumberOfBytesRead);
            return buffer[0];
        }

        public byte ReadByte(string address, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                byte[] buffer = new byte[1];
                ProcessMemory.ReadProcessMemory(_processHandle, key + _emulatorOffset + offset, buffer, 1, out long lpNumberOfBytesRead);
                return buffer[0];
            }
            Constants.WriteError($"Incorrect address key {address}.");
            return 0;
            
        }

        public void WriteByte(long address, byte value) {
            var val = BitConverter.GetBytes(value);
            ProcessMemory.WriteProcessMemory(_processHandle, address + _emulatorOffset, val, 1, out int lpNumberOfBytesWritten);
        }

        public void WriteByte(string address, byte value, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                var val = BitConverter.GetBytes(value);
                ProcessMemory.WriteProcessMemory(_processHandle, key + _emulatorOffset + offset, val, 1, out int lpNumberOfBytesWritten);
                return;
            }
            Constants.WriteError($"Incorrect address key {address}.");
        }

        #endregion

        #region SByte
        public sbyte ReadSByte(long address) {
            byte[] buffer = new byte[1];
            ProcessMemory.ReadProcessMemory(_processHandle, address + _emulatorOffset, buffer, 1, out long lpNumberOfBytesRead);
            return (sbyte) buffer[0];
        }

        public sbyte ReadSByte(string address, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                byte[] buffer = new byte[1];
                ProcessMemory.ReadProcessMemory(_processHandle, key + _emulatorOffset + offset, buffer, 1, out long lpNumberOfBytesRead);
                return (sbyte) buffer[0];
            }
            Constants.WriteError($"Incorrect address key {address}.");
            return 0;
        }

        public void WriteSByte(long address, sbyte value) {
            var val = BitConverter.GetBytes(value);
            ProcessMemory.WriteProcessMemory(_processHandle, address + _emulatorOffset, val, 1, out int lpNumberOfBytesWritten);
        }

        public void WriteSByte(string address, sbyte value, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                var val = BitConverter.GetBytes(value);
                ProcessMemory.WriteProcessMemory(_processHandle, key + _emulatorOffset + offset, val, 1, out int lpNumberOfBytesWritten);
                return;
            }
            Constants.WriteError($"Incorrect address key {address}.");
        }

        #endregion

        #region Short

        public short ReadShort(long address) {
            byte[] buffer = new byte[2];
            ProcessMemory.ReadProcessMemory(_processHandle, address + _emulatorOffset, buffer, 2, out long lpNumberOfBytesRead);
            return BitConverter.ToInt16(buffer, 0);
        }
        public short ReadShort(string address, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                byte[] buffer = new byte[2];
                ProcessMemory.ReadProcessMemory(_processHandle, key + _emulatorOffset + offset, buffer, 2, out long lpNumberOfBytesRead);
                return BitConverter.ToInt16(buffer, 0);
            }
            Constants.WriteError($"Incorrect address key {address}.");
            return 0;
        }

        public void WriteShort(long address, short value) {
            var val = BitConverter.GetBytes(value);
            ProcessMemory.WriteProcessMemory(_processHandle, address + _emulatorOffset, val, 2, out int lpNumberOfBytesWritten);
        }
        public void WriteShort(string address, short value, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                var val = BitConverter.GetBytes(value);
                ProcessMemory.WriteProcessMemory(_processHandle, key + _emulatorOffset + offset, val, 2, out int lpNumberOfBytesWritten);
                return;
            }
            Constants.WriteError($"Incorrect address key {address}.");
        }

        #endregion

        #region UShort

        public ushort ReadUShort(long address) {
            byte[] buffer = new byte[2];
            ProcessMemory.ReadProcessMemory(_processHandle, address + _emulatorOffset, buffer, 2, out long lpNumberOfBytesRead);
            return BitConverter.ToUInt16(buffer, 0);
        }

        public ushort ReadUShort(string address, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                byte[] buffer = new byte[2];
                ProcessMemory.ReadProcessMemory(_processHandle, key + _emulatorOffset + offset, buffer, 2, out long lpNumberOfBytesRead);
                return BitConverter.ToUInt16(buffer, 0);
            }
            Constants.WriteError($"Incorrect address key {address}.");
            return 0;
        }

        public void WriteUShort(long address, ushort value) {
            var val = BitConverter.GetBytes(value);
            ProcessMemory.WriteProcessMemory(_processHandle, address + _emulatorOffset, val, 2, out int lpNumberOfBytesWritten);
        }

        public void WriteUShort(string address, ushort value, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                var val = BitConverter.GetBytes(value);
                ProcessMemory.WriteProcessMemory(_processHandle, key + _emulatorOffset + offset, val, 2, out int lpNumberOfBytesWritten);
                return;
            }
            Constants.WriteError($"Incorrect address key {address}.");
        }

        #endregion

        #region UInt24

        public UInt32 ReadUInt24(long address) {
            byte[] buffer = new byte[3];
            ProcessMemory.ReadProcessMemory(_processHandle, address + _emulatorOffset, buffer, 3, out long lpNumberOfBytesRead);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public UInt32 ReadUInt24(string address, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                byte[] buffer = new byte[3];
                ProcessMemory.ReadProcessMemory(_processHandle, key + _emulatorOffset + offset, buffer, 3, out long lpNumberOfBytesRead);
                return BitConverter.ToUInt32(buffer, 0);
            }
            Constants.WriteError($"Incorrect address key {address}.");
            return 0;
        }

        public void WriteUInt24(long address, UInt32 value) {
            var val = BitConverter.GetBytes(value);
            val = val.Take(val.Count() - 1).ToArray();
            ProcessMemory.WriteProcessMemory(_processHandle, address + _emulatorOffset, val, 3, out int lpNumberOfBytesWritten);
        }

        public void WriteUInt24(string address, UInt32 value, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                var val = BitConverter.GetBytes(value);
                val = val.Take(val.Count() - 1).ToArray();
                ProcessMemory.WriteProcessMemory(_processHandle, key + _emulatorOffset + offset, val, 3, out int lpNumberOfBytesWritten);
                return;
            }
            Constants.WriteError($"Incorrect address key {address}.");
        }

        #endregion

        #region Int

        public Int32 ReadInt(long address) {
            byte[] buffer = new byte[4];
            ProcessMemory.ReadProcessMemory(_processHandle, address + _emulatorOffset, buffer, 4, out long lpNumberOfBytesRead);
            return BitConverter.ToInt32(buffer, 0);
        }

        public Int32 ReadInt(string address, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                byte[] buffer = new byte[4];
                ProcessMemory.ReadProcessMemory(_processHandle, key + _emulatorOffset + offset, buffer, 4, out long lpNumberOfBytesRead);
                return BitConverter.ToInt32(buffer, 0);
            }
            Constants.WriteError($"Incorrect address key {address}.");
            return 0;
        }

        public void WriteInt(long address, Int32 value) {
            var val = BitConverter.GetBytes(value);
            ProcessMemory.WriteProcessMemory(_processHandle, address + _emulatorOffset, val, 4, out int lpNumberOfBytesWritten);
        }

        public void WriteInt(string address, Int32 value, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                var val = BitConverter.GetBytes(value);
                ProcessMemory.WriteProcessMemory(_processHandle, key + _emulatorOffset + offset, val, 4, out int lpNumberOfBytesWritten);
                return;
            }
            Constants.WriteError($"Incorrect address key {address}.");
        }

        #endregion

        #region UInt

        public UInt32 ReadUInt(long address) {
            byte[] buffer = new byte[4];
            ProcessMemory.ReadProcessMemory(_processHandle, address + _emulatorOffset, buffer, 4, out long lpNumberOfBytesRead);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public UInt32 ReadUInt(string address, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                byte[] buffer = new byte[4];
                ProcessMemory.ReadProcessMemory(_processHandle, key + _emulatorOffset + offset, buffer, 4, out long lpNumberOfBytesRead);
                return BitConverter.ToUInt32(buffer, 0);
            }
            Constants.WriteError($"Incorrect address key {address}.");
            return 0;
        }

        public void WriteUInt(long address, UInt32 value) {
            var val = BitConverter.GetBytes(value);
            ProcessMemory.WriteProcessMemory(_processHandle, address + _emulatorOffset, val, 4, out int lpNumberOfBytesWritten);
        }

        public void WriteUInt(string address, UInt32 value, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                var val = BitConverter.GetBytes(value);
                ProcessMemory.WriteProcessMemory(_processHandle, key + _emulatorOffset + offset, val, 4, out int lpNumberOfBytesWritten);
                return;
            }
            Constants.WriteError($"Incorrect address key {address}.");
        }

        #endregion

        #region Long

        public long ReadLong(long address) {
            byte[] buffer = new byte[8];
            ProcessMemory.ReadProcessMemory(_processHandle, address + _emulatorOffset, buffer, 8, out long lpNumberOfBytesRead);
            return BitConverter.ToInt64(buffer, 0);
        }

        public long ReadLong(string address, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                byte[] buffer = new byte[8];
                ProcessMemory.ReadProcessMemory(_processHandle, key + _emulatorOffset + offset, buffer, 8, out long lpNumberOfBytesRead);
                return BitConverter.ToInt64(buffer, 0);
            }
            Constants.WriteError($"Incorrect address key {address}.");
            return 0;
        }

        public void WriteLong(long address, long value) {
            var val = BitConverter.GetBytes(value);
            ProcessMemory.WriteProcessMemory(_processHandle, address + _emulatorOffset, val, 8, out int lpNumberOfBytesWritten);
        }

        public void WriteLong(string address, long value, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                var val = BitConverter.GetBytes(value);
                ProcessMemory.WriteProcessMemory(_processHandle, key + _emulatorOffset + offset, val, 8, out int lpNumberOfBytesWritten);
                return;
            }
            Constants.WriteError($"Incorrect address key {address}.");
        }

        #endregion

        #region ULong

        public ulong ReadULong(long address) {
            byte[] buffer = new byte[8];
            ProcessMemory.ReadProcessMemory(_processHandle, address + _emulatorOffset, buffer, 8, out long lpNumberOfBytesRead);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public ulong ReadULong(string address, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                byte[] buffer = new byte[8];
                ProcessMemory.ReadProcessMemory(_processHandle, key + _emulatorOffset + offset, buffer, 8, out long lpNumberOfBytesRead);
                return BitConverter.ToUInt64(buffer, 0);
            }
            Constants.WriteError($"Incorrect address key {address}.");
            return 0;
        }

        public void WriteULong(long address, ulong value) {
            var val = BitConverter.GetBytes(value);
            ProcessMemory.WriteProcessMemory(_processHandle, address + _emulatorOffset, val, 8, out int lpNumberOfBytesWritten);
        }

        public void WriteULong(string address, ulong value, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                var val = BitConverter.GetBytes(value);
                ProcessMemory.WriteProcessMemory(_processHandle, key + _emulatorOffset + offset, val, 8, out int lpNumberOfBytesWritten);
            }
            Constants.WriteError($"Incorrect address key {address}.");
        }

        #endregion

        #region AoB

        public byte[] ReadAoB(long startAddr, long endAddr) {
            long len = endAddr - startAddr;
            byte[] buffer = new byte[len];
            ProcessMemory.ReadProcessMemory(_processHandle, startAddr + _emulatorOffset, buffer, len, out long lpNumberOfBytesRead);
            return buffer;
        }

        public byte[] ReadAoB(string address, long length, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                byte[] buffer = new byte[length];
                ProcessMemory.ReadProcessMemory(_processHandle, key + _emulatorOffset + offset, buffer, length, out long lpNumberOfBytesRead);
                return buffer;
            }
            Constants.WriteError($"Incorrect address key {address}.");
            return new byte[0];
        }

        public void WriteAoB(long address, string values) {
            string[] strArr = values.Split(' ');
            byte[] arr = new byte[strArr.Length];
            for (int i = 0; i < strArr.Length; i++) {
                arr[i] = Convert.ToByte(strArr[i], 16);
            }
            ProcessMemory.WriteProcessMemory(_processHandle, address + _emulatorOffset, arr, arr.Length, out int lpNumberOfBytesWritten);
        }

        public void WriteAoB(string address, string values, int offset = 0) {
            if (_regionalAddresses.TryGetValue(address, out var key)) {
                string[] strArr = values.Split(' ');
                byte[] arr = new byte[strArr.Length];
                for (int i = 0; i < strArr.Length; i++) {
                    arr[i] = Convert.ToByte(strArr[i], 16);
                }
                ProcessMemory.WriteProcessMemory(_processHandle, key + _emulatorOffset + offset, arr, arr.Length, out int lpNumberOfBytesWritten);
                Constants.WriteError($"Incorrect address key {address}.");
                return;
            }
        }

        #endregion

        public List<long> ScanAoB(long start, long end, string pattern, bool useOffset = true, bool addOffset = false) {
            long offset = 0;
            if (!useOffset) {
                offset -= _emulatorOffset;
            }

            List<long> results = KMP.Search(pattern, ReadAoB(start + offset, end + offset), true);

            for (int i = 0; i < results.Count; i++) {
                results[i] += start;
                if (addOffset) {
                    results[i] += _emulatorOffset;
                }
            }

            return results;
        }

        // Write text

        // Read name

        // GetCharacterByNumber

        // GetCharacterbyChar

        bool Verify(long offset) {
            var start = versionAddr + offset;
            var end = start + versionStringLen;
            string version = Encoding.Default.GetString(ReadAoB(start - _emulatorOffset, end - _emulatorOffset));
            if (versions.TryGetValue(version, out var key)) {
                _region = key;
                Constants.WriteOutput($"Detected region: {key}");
                return true;
            }
            return false;
        }

        bool Emulators(Process proc, string emulatorName) {
            if (Verify(_emulatorOffset)) {
                Constants.WriteOutput("Previous offset successful.");
                return true;
            } else {
                if (emulatorName.ToLower() == "retroarch") {
                    return RetroArch(proc);
                } else if (emulatorName.ToLower().Contains("duckstation")) {
                    return DuckStation(proc);
                } else if (emulatorName.Contains("ePSXe")) {
                    return ePSXe(proc);
                }
                return false;
            }
        }

        bool ePSXe(Process proc) {
            _emulatorOffset = 0;
            var start = (long) proc.MainModule.BaseAddress;
            var end = start + proc.MainModule.ModuleMemorySize;
            Constants.WriteOutput("Starting Scan: " + Convert.ToString(start, 16).ToUpper() + " - " + Convert.ToString(end, 16).ToUpper());
            var results = KMP.Search(AoBCheck, ReadAoB(start, end), true);
            foreach (var result in results) {
                var tempOffset = start + result - 0xB070;
                if (Verify(tempOffset)) {
                    _emulatorOffset = tempOffset;
                    Constants.KEY.SetValue("Offset", _emulatorOffset);
                    Constants.WriteOutput("Base scan successful.");
                    return true;
                }
            }
            return false;
        }

        bool RetroArch(Process proc) {
            try {
                _emulatorOffset = 0;
                var start = (long) proc.MainModule.BaseAddress;
                var end = start + 0x1000008;
                for (int i = 0; i < 17; i++) {
                    Constants.WriteOutput("Start RetroArch Scan (" + i + "/16): " + Convert.ToString(start, 16).ToUpper() + " - " + Convert.ToString(end, 16).ToUpper());
                    var results = KMP.Search(AoBCheck, ReadAoB(start, end), true);
                    foreach (var result in results) {
                        var tempOffset = start + result - 0xB070;
                        if (Verify(tempOffset)) {
                            _emulatorOffset = tempOffset;
                            Constants.KEY.SetValue("Offset", _emulatorOffset);
                            Constants.WriteOutput("Base scan successful.");
                            return true;
                        }
                    }
                    start += 0x10000000;
                    end += 0x10000000;
                }
                return false;

            } catch (Exception e) {
                Constants.WriteOutput("RetroArch scan failed.");
                return false;
            }
        }

        bool DuckStation(Process proc) {
            _emulatorOffset = 0;
            var start = (long) proc.MainModule.BaseAddress;
            var end = start + proc.MainModule.ModuleMemorySize;
            var results = KMP.Search(duckstationCheck, ReadAoB(start, end), true);
            foreach (var result in results) {
                foreach (var offset in duckstationOffsets) {
                    var pointer = ReadLong(result + start - offset);
                    if (Verify(pointer)) {
                        _emulatorOffset = pointer;
                        Constants.KEY.SetValue("Offset", _emulatorOffset);
                        Constants.WriteOutput("Base scan successful.");
                        return true;
                    }
                }
            }
            return false;
        }

        static Process FindEmulatorProcess(string emulatorName) {
            Process[] processes = Process.GetProcesses();

            foreach (Process proc in processes) {
                if (proc.ProcessName.Equals(emulatorName, StringComparison.CurrentCultureIgnoreCase) || proc.ProcessName.Contains(emulatorName.ToLower())) { // Find (name).exe in the process list (use task manager to find the name)
                    return proc;
                }
            }
            throw new EmulatorNotFoundException();
        }

        static Dictionary<string, int> LoadRegionalAddresses(Region region) {
            var addresses = new Dictionary<string, int>();
            using (StreamReader reader = File.OpenText("Scripts\\Addresses.csv")) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    string[] values = line.Split(',');
                    if (addresses.ContainsKey(values[0])) {
                        Constants.WriteDebug("Same key warning: " + values[0]);
                    } else {
                        addresses.Add(values[0], Convert.ToInt32(values[(int) region + 1], 16));
                    }
                }
            }

            foreach (string file in Directory.GetFiles("Scripts\\Addresses", "*.csv", SearchOption.AllDirectories)) {
                using (StreamReader reader = File.OpenText(file)) {
                    string line;
                    while ((line = reader.ReadLine()) != null) {
                        string[] values = line.Split(',');
                        if (addresses.ContainsKey(values[0])) {
                            Constants.WriteDebug("Same key warning: " + values[0]);
                        } else {
                            addresses.Add(values[0], Convert.ToInt32(values[(int) region + 1], 16));
                        }
                    }
                }
            }



            return addresses;
        }
    }
}