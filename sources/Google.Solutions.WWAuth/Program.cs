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

using Google.Apis.Logging;
using Google.Solutions.WWAuth.Adapters;
using Google.Solutions.WWAuth.Data;
using Google.Solutions.WWAuth.Interop;
using Google.Solutions.WWAuth.View;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

[assembly: InternalsVisibleTo("Google.Solutions.WWAuth.Test")]

namespace Google.Solutions.WWAuth
{
    static class Program
    {
        public static readonly string ExecutablePath
            = Assembly.GetExecutingAssembly().Location;

        private static readonly IShellAdapter shellAdapter = new ShellAdapter();

        private static void WindowsMain()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                var options = AttendedCommandLineOptions
                    .Parse(Environment.CommandLine)
                    .Validate();

                if (options.Verify != null)
                {
                    //
                    // Verify the given configuration.
                    //
                    using (var dialog = new VerifyConfigurationDialog(
                        CredentialConfigurationFile
                            .FromFile(options.Verify)
                            .Configuration))
                    {
                        dialog.ShowDialog(null);
                    }
                }
                else
                {
                    //
                    // Register a file extension.
                    //
                    shellAdapter.RegisterFileExtension(
                        typeof(Program).Namespace,
                        CredentialConfigurationFile.FileExtension,
                        new AttendedCommandLineOptions()
                        {
                            Executable = ExecutablePath,
                            Edit = "%1"
                        }.ToString());

                    //
                    // Edit or create new file.
                    //
                    // NB. For now, we always create a workload identity
                    // configuration. 
                    //
                    var file = string.IsNullOrEmpty(options.Edit)
                        ? CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile()
                        : CredentialConfigurationFile.FromFile(options.Edit);

                    using (var dialog = new EditConfigurationDialog(file))
                    {
                        dialog.ShowDialog(null);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorDialog.ShowError(null, "Invalid credential configuration", e);
                Environment.Exit(1);
            }
        }

        private static void ConsoleMain()
        {
            //
            // Fetch token for current user and output a
            // pluggable auth-compliant result.
            //
            var writer = ConsoleHandle.Out.Writer;
            try
            {
                var options = UnattendedCommandLineOptions
                    .Parse(Environment.CommandLine)
                    .Validate();

                //
                // Create the right token adapter.
                //
                var tokenAdapter = AdapterFactory.CreateTokenAdapter(
                    options,
                    new NullLogger());

                //
                // Acquire a token and write the result to the console
                // (where the calling application will pick it up).
                //
                var token = tokenAdapter
                    .AcquireTokenAsync(
                        Adapters.TokenAcquisitionOptions.None,
                        CancellationToken.None)
                    .Result;

                new PluggableAuthResult(token).WriteTo(writer);
            }
            catch (Exception e)
            {
                //
                // Pluggable auth expects errors to be reported
                // as a result on STDOUT, not on STDERR.
                //
                new PluggableAuthResult(e).WriteTo(writer);
            }
            finally
            {
                writer.Flush();
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            //
            // Enable TLS 1.2.
            //
            System.Net.ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12 |
                SecurityProtocolType.Tls11;

            //
            // N.B. Pluggable authentication providers must write
            // to STDOUT. On Windows, that (typically) demands that
            // we create a CONSOLE-subsystem EXE, because GUI-subsystem
            // processes don't have an attached console, and no STDOUT.
            //
            // But we want the same application to also serve as a
            // GUI app (for configuration and testing). That demands 
            // that we create a GUI-subsystem EXE, otherwise, we'll
            // get an (ugly, empty) console window.
            //
            // To have our cake and eat it to, let Windows treat this
            // EXE as a GUI-subsystem process. At launch, check if we've 
            // been launched with a redirected STDOUT handle. If so, then
            // assume we're in "pluggable authentication mode" and behave
            // like a console app.
            //
            // If STDOUT hasn't been redirected, then behave like a 
            // regular GUI app.
            //
            // To force the app to behave like a console app, just
            // redirect output to `more` or a file:
            //
            //   wwauth.exe <...> | more
            // 
            // N.B. The Visual Studio debugger always launches apps
            // with STDOUT redirected, hence the (otherwise redundant)
            // check for /protocol.
            //

            if (ConsoleHandle.Out.IsRedirected &&
                args.Any(a => a.Equals("/protocol", StringComparison.OrdinalIgnoreCase)))
            {
                //
                // Run in unattended mode, acting like
                // pluggable-auth console application.
                //
                ConsoleMain();
            }
            else
            {
                //
                // Act like a Windows application
                //
                WindowsMain();
            }
        }
    }
}
