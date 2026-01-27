//
// Copyright 2022 Google LLC
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//

using Google.Solutions.WWAuth.Interop;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Google.Solutions.WWAuth.Adapters
{
    /// <summary>
    /// Adapter for the Windows Shell and related APIs.
    /// </summary>
    public interface IShellAdapter
    {
        void RegisterFileExtension(
            string progId,
            string extension,
            string openCommand);

        DialogResult ShowSaveFileDialog(
            IWin32Window owner,
            string title,
            string filter,
            string defaultExtension,
            out string filePath);

        void StartConsoleCommand(string commandLine);

        void StartProcessAsUser(
            string fileName,
            string commandLine,
            NetworkCredential credential);

        void OpenFile(string path);

        DialogResult PromptForCredentials(
            IWin32Window owner,
            out NetworkCredential credential);
    }

    internal class ShellAdapter : IShellAdapter
    {
        /// <summary>
        /// Register a file extension and command.
        /// </summary>
        public void RegisterFileExtension(
            string progId,
            string extension,
            string openCommand)
        {
            Debug.Assert(!extension.StartsWith("."));
            Debug.Assert(openCommand.Contains(".exe"));
            Debug.Assert(openCommand.Contains("%1"));

            using (var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default))
            {
                using (var progIdKey = hkcu.CreateSubKey($@"SOFTWARE\Classes\{progId}"))
                {
                }

                using (var commandKey = hkcu.CreateSubKey($@"SOFTWARE\Classes\{progId}\shell\open\command"))
                {
                    commandKey.SetValue(null, openCommand);
                }

                using (var extensionKey = hkcu.CreateSubKey($@"SOFTWARE\Classes\.{extension}"))
                {
                    extensionKey.SetValue(null, progId);
                }
            }
        }

        public DialogResult ShowSaveFileDialog(
            IWin32Window owner,
            string title,
            string filter,
            string defaultExtension,
            out string filePath)
        {

            using (var dialog = new SaveFileDialog()
            {
                AddExtension = true,
                DefaultExt = defaultExtension,
                Filter = filter,
                OverwritePrompt = true,
                RestoreDirectory = true,
                ValidateNames = true,
                Title = title
            })
            {
                var result = dialog.ShowDialog(owner);
                filePath = result == DialogResult.OK
                    ? dialog.FileName
                    : null;

                return result;
            }
        }

        public void StartConsoleCommand(
            string commandLine)
        {
            using (Process.Start(new ProcessStartInfo()
            {
                FileName = $"{Environment.SystemDirectory}\\cmd.exe",
                Arguments = commandLine,
                UseShellExecute = false
            }))
            { }
        }

        public void StartProcessAsUser(
            string fileName,
            string commandLine,
            NetworkCredential credential)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(
                    "File not found: " + fileName);
            }

            using (Process.Start(new ProcessStartInfo()
            {
                FileName = fileName,
                Arguments = commandLine,
                UserName = credential.UserName,
                Password = credential.SecurePassword,
                Domain = string.IsNullOrEmpty(credential.Domain)
                    ? null
                    : credential.Domain, // Avoid passing an empty string
                UseShellExecute = false,
                LoadUserProfile = true
            }))
            { }
        }

        public void OpenFile(string path)
        {
            using (Process.Start(new ProcessStartInfo()
            {
                Verb = "open",
                FileName = path,
                UseShellExecute = true
            }))
            { }
        }

        public DialogResult PromptForCredentials(
            IWin32Window owner,
            out NetworkCredential credential)
        {
            credential = null;

            var uiInfo = new NativeMethods.CREDUI_INFO()
            {
                cbSize = Marshal.SizeOf<NativeMethods.CREDUI_INFO>(),
                hwndParent = owner.Handle,
                pszCaptionText = "Enter credentials",
                pszMessageText = "Enter Active Directory Credentials"
            };

            var usernameBuffer = new StringBuilder(256);
            var passwordBuffer = new StringBuilder(256);
            var save = false;

            var result = NativeMethods.CredUIPromptForCredentialsW(
                ref uiInfo,
                string.Empty,
                IntPtr.Zero,
                0,
                usernameBuffer,
                usernameBuffer.Capacity,
                passwordBuffer,
                passwordBuffer.Capacity,
                ref save,
                NativeMethods.CREDUI_FLAGS.GENERIC_CREDENTIALS |
                    NativeMethods.CREDUI_FLAGS.ALWAYS_SHOW_UI |
                    NativeMethods.CREDUI_FLAGS.DO_NOT_PERSIST |
                    NativeMethods.CREDUI_FLAGS.VALIDATE_USERNAME
                    );
            if (result == NativeMethods.ERROR_NOERROR)
            {
                var username = usernameBuffer.ToString();
                if (username.Contains("\\"))
                {
                    //
                    // NetBIOS notation.
                    //
                    var parts = username.Split('\\');
                    credential = new NetworkCredential(
                        parts[1],
                        passwordBuffer.ToString(),
                        parts[0]);
                }
                else
                {
                    //
                    // UPN notation.
                    //
                    credential = new NetworkCredential(
                        usernameBuffer.ToString(),
                        passwordBuffer.ToString());
                }

                return DialogResult.OK;
            }
            else if (result == NativeMethods.ERROR_CANCELLED)
            {
                return DialogResult.Cancel;
            }
            else
            {
                throw new Win32Exception(result);
            }
        }
    }
}
