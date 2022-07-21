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
using Google.Solutions.WWAuth.Util;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Google.Solutions.WWAuth.View
{
    [SkipCodeCoverage]
    internal class ErrorDialog
    {
        public static void ShowError(
            IWin32Window parent,
            string caption,
            Exception e)
        {
            Debug.Assert(!(parent is Control control) || !control.InvokeRequired);

            e = e.Unwrap();

            var details = new StringBuilder();

            for (var innerException = e.InnerException;
                    innerException != null; innerException =
                    innerException.InnerException)
            {
                details.Append(e.InnerException.GetType().Name);
                details.Append(":\n");
                details.Append(innerException.Message);
                details.Append("\n");
            }

            var config = new NativeMethods.TASKDIALOGCONFIG()
            {
                cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.TASKDIALOGCONFIG)),
                hwndParent = parent?.Handle ?? IntPtr.Zero,
                dwFlags = 0,
                dwCommonButtons = NativeMethods.TASKDIALOG_COMMON_BUTTON_FLAGS.TDCBF_OK_BUTTON,
                pszWindowTitle = "An error occured",
                MainIcon = NativeMethods.TD_ERROR_ICON,
                pszMainInstruction = caption,
                pszContent = e.Message,
                pszExpandedInformation = details.ToString()
            };

            NativeMethods.TaskDialogIndirect(
                ref config,
                out _,
                out _,
                out _);
        }
    }
}
