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

using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Google.Solutions.WWAuth.Util
{
    /// <summary>
    /// Extension methods for enums.
    /// </summary>
    internal static class EnumExtensions
    {
        public static string GetDescription<TEnum>(this TEnum enumerationValue)
            where TEnum : struct
        {
            Debug.Assert(enumerationValue.GetType().IsEnum);

            return typeof(TEnum)
                .GetMember(enumerationValue.ToString())
                .EnsureNotNull()
                .SelectMany(m => m.GetCustomAttributes(typeof(DescriptionAttribute), false))
                .OfType<DescriptionAttribute>()
                .Select(attr => attr.Description)
                .FirstOrDefault() ?? enumerationValue.ToString();
        }

        public static TEnum? FromDescription<TEnum>(string description)
            where TEnum : struct
        {
            var field = typeof(TEnum)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .FirstOrDefault(f => f.GetCustomAttribute(typeof(DescriptionAttribute))
                    is DescriptionAttribute d &&
                    d.Description == description);

            if (field != null)
            {
                return (TEnum)field.GetValue(null);
            }
            else
            {
                return null;
            }
        }
    }
}
