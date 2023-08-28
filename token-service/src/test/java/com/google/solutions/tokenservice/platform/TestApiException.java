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

package com.google.solutions.tokenservice.platform;

import com.google.api.client.googleapis.json.GoogleJsonError;
import com.google.api.client.googleapis.json.GoogleJsonResponseException;
import com.google.api.client.http.HttpHeaders;
import com.google.api.client.http.HttpResponseException;
import org.junit.jupiter.api.Test;

import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;

public class TestApiException {
  // -------------------------------------------------------------------------
  // issueToken.
  // -------------------------------------------------------------------------

  @Test
  public void whenJsonResponseHasDetails_thenFromReturnsException() {
    var error = new GoogleJsonError();
    error.setMessage("detail-message");

    error.setDetails(List.of(new GoogleJsonError.Details()));
    var e = ApiException.from(new GoogleJsonResponseException(
      new HttpResponseException.Builder(400, "", new HttpHeaders())
        .setMessage("message"),
      error));

    assertEquals("detail-message", e.getMessage());
  }

  @Test
  public void whenJsonResponseHasNoDetails_thenFromReturnsException() {
    var e = ApiException.from(new GoogleJsonResponseException(
      new HttpResponseException.Builder(400, "", new HttpHeaders())
        .setMessage("message"),
      null));

    assertEquals("message", e.getMessage());
  }
}
