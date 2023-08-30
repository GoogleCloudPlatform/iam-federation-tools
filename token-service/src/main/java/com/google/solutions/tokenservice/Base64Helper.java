//
// Copyright 2023 Google LLC
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

package com.google.solutions.tokenservice;

import java.nio.charset.StandardCharsets;
import java.util.Base64;

public class Base64Helper {
    private Base64Helper() {}

    /**
     * Escape a string by base64-encoding its UTF-8 representation.
     */
    public static String escape(String string) {
        return Base64.getEncoder().encodeToString(string.getBytes(StandardCharsets.UTF_8));
    }

    /**
     * Unescape a base64-encoded UTF-8 string.
     */
    public static String unescape(String string) {
        return new String(Base64.getDecoder().decode(string), StandardCharsets.UTF_8);
    }
}
