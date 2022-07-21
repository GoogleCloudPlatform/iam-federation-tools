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

using Google.Apis.Util;
using Google.Solutions.WWAuth.Adapters;
using Google.Solutions.WWAuth.Data;
using Google.Solutions.WWAuth.Interop;
using System.ComponentModel;
using System.IO;
using System.Security.AccessControl;
using System.Windows.Forms;

namespace Google.Solutions.WWAuth.View
{
    internal class EditConfigurationViewModel : ViewModelBase
    {
        private readonly CredentialConfigurationFile file;
        private readonly IShellAdapter shellAdapter;

        private string windowTitle = string.Empty;

        public EditConfigurationViewModel(
            CredentialConfigurationFile file,
            IShellAdapter shellAdapter)
        {
            this.file = file.ThrowIfNull(nameof(file));
            this.shellAdapter = shellAdapter.ThrowIfNull(nameof(shellAdapter));

            this.WindowTitle = file.FilePath != null
                ? new FileInfo(file.FilePath).Name
                : "New configuration";
        }

        //---------------------------------------------------------------------
        // Observable properties.
        //---------------------------------------------------------------------

        public string WindowTitle
        {
            get => this.windowTitle;
            private set
            {
                this.windowTitle = value;
                RaisePropertyChange();
            }
        }

        //---------------------------------------------------------------------
        // Actions.
        //---------------------------------------------------------------------

        public DialogResult VerifyConfiguration(
            IWin32Window owner)
        {
            using (var dialog = new VerifyConfigurationDialog(
                this.file.Configuration))
            {
                return dialog.ShowDialog(owner);
            }
        }

        public DialogResult VerifyConfigurationAsUser(
            IWin32Window owner)
        {
            //
            // Launch a copy of this process as a different user.
            //
            // By running the entire program as that user, we not
            // only test if authentication works, but also ensure
            // that the user is allowed to access any required
            // certificates, files, etc.
            //
            // Create a temporary copy since the last changes might not
            // have been applied yet.
            //
            var tempFile = this.file.Clone();
            tempFile.SaveAs(Path.GetTempFileName());

            var result = shellAdapter.PromptForCredentials(
                owner,
                out var credential);
            if (result == DialogResult.OK)
            {
                //
                // Grant the user access to the file.
                //
                // N.B. Passing the file contents on the command line
                // would save us from changing file permissions, but
                // CreateProcessWithLogon has a 1K command line limit,
                // which is too short for that purpose.
                //
                var access = File.GetAccessControl(tempFile.FilePath);
                access.AddAccessRule(new FileSystemAccessRule(
                    credential.UserName,
                    FileSystemRights.Read,
                    AccessControlType.Allow));
                File.SetAccessControl(tempFile.FilePath, access);

                //
                // Launch a new process as the selected user.
                //
                try
                {
                    shellAdapter.StartProcessAsUser(
                        Program.ExecutablePath,
                        new AttendedCommandLineOptions()
                        {
                            Executable = string.Empty,
                            Verify = tempFile.FilePath
                        }.ToString(),
                        credential);
                }
                catch (Win32Exception e) when (e.NativeErrorCode == NativeMethods.ERROR_DIRECTORY)
                {
                    throw new IOException(
                        $"The user {credential.UserName} does not have access to the " +
                        $"program file {Program.ExecutablePath}.\n\n" +
                        "Modify the file permissions or move the program file to a " +
                        "different folder to ensure that the user can access and run " +
                        "this program.");
                }

                return DialogResult.OK;
            }
            else
            {
                return result;
            }
        }

        public void LaunchGcloud()
        {
            //
            // Create a temporary copy since the last changes might not
            // have been applied yet.
            //
            var tempFile = Path.GetTempFileName();
            this.file.Clone().SaveAs(tempFile);

            this.shellAdapter.StartConsoleCommand(
                $"cmd /K gcloud auth login --cred-file \"{tempFile}\"");
        }

        public void LaunchCommandLineEnvironment()
        {

            //
            // Create a temporary copy since the last changes might not
            // have been applied yet.
            //
            var tempFile = Path.GetTempFileName();
            this.file.Clone().SaveAs(tempFile);

            this.shellAdapter.StartConsoleCommand(
                $"cmd /K \"set \"GOOGLE_EXTERNAL_ACCOUNT_ALLOW_EXECUTABLES=1\" & " +
                    $"set \"GOOGLE_APPLICATION_CREDENTIALS={tempFile}\"\" & " +
                    $"echo Environment updated to use latest configuration as Application Default Credentials.");
        }

        public void LaunchExecutableCommand()
        {
            this.shellAdapter.StartConsoleCommand($"cmd /K {this.file.Configuration.Options} | more");
        }
    }
}
