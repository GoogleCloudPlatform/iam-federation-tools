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

using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Runtime.InteropServices;

namespace Google.Solutions.WWAuth.Interop
{
    /// <summary>
    /// Helper class for finding out if console handles have
    /// been redirected or not.
    /// </summary>
    internal class ConsoleHandle
    {
        public bool IsRedirected { get; private set; }
        public SafeHandle Handle { get; private set; }
        public TextWriter Writer { get; private set; }

        public static ConsoleHandle Out { get; }

        private ConsoleHandle()
        {
        }

        static ConsoleHandle()
        {
            var stdout = NativeMethods.GetStdHandle(
                NativeMethods.StandardHandle.Output);

            var fileType = NativeMethods.GetFileType(stdout);
            if (fileType == NativeMethods.FileType.Disk ||
                fileType == NativeMethods.FileType.Pipe)
            {
                //
                // STDOUT has been redirected.
                //
                NativeMethods.AttachConsole(NativeMethods.ATTACH_PARENT_PROCESS);

                var safeHandle = new SafeFileHandle(stdout, false);
                Out = new ConsoleHandle()
                {
                    IsRedirected = true,
                    Handle = safeHandle,
                    Writer = new StreamWriter(new FileStream(safeHandle, FileAccess.Write))
                };

            }
            else
            {
                //
                // STDOUT hasn't been redirected, so there's no console
                // to write to.
                //
                Out = new ConsoleHandle()
                {
                    IsRedirected = false,
                };
            }
        }
    }
}
