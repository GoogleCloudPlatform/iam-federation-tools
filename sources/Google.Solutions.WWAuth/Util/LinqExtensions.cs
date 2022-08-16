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

using System.Collections.Generic;
using System.Linq;

namespace Google.Solutions.WWAuth.Util
{
    /// <summary>
    /// Extension methods for enumerables.
    /// </summary>
    internal static class LinqExtensions
    {
        public static IEnumerable<T> EnsureNotNull<T>(this IEnumerable<T> e)
        {
            return e ?? Enumerable.Empty<T>();
        }

        public static V TryGet<K, V>(
            this IDictionary<K, V> dict,
            K key)
            where V : class
        {
            if (dict.TryGetValue(key, out V value))
            {
                return value;
            }
            else
            {
                return default(V);
            }
        }

        public static IEnumerable<string> SplitQuotedString(
            this string s,
            params char[] separator)
        {
            var splits = new List<string>();
            bool inQuotedSegment = false;

            var segments = s.Split('\'', '"');
            for (int i = 0; i < segments.Length; i++)
            {
                segments[i] = segments[i].Trim(separator);

                if (segments[i].Length == 0 &&
                    (i == 0 || i == segments.Length - 1))
                {
                    //
                    // String starts/ends with a quote - skip
                    // that segment.
                    //
                }
                else if (inQuotedSegment)
                {
                    //
                    // Treat as one.
                    //
                    splits.Add(segments[i]);
                }
                else
                {
                    //
                    // Split as normal.
                    //
                    splits.AddRange(segments[i].Split(separator));
                }

                inQuotedSegment = !inQuotedSegment;
            }

            return splits;
        }
    }
}
