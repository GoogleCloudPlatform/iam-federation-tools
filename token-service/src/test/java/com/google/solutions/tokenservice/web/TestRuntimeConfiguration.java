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

package com.google.solutions.tokenservice.web;

import com.google.solutions.tokenservice.oauth.mtls.XlbMtlsClientCredentialsFlow;
import org.junit.jupiter.api.Test;

import java.util.Map;
import java.util.Set;

import static org.junit.jupiter.api.Assertions.assertEquals;

public class TestRuntimeConfiguration {

  // -------------------------------------------------------------------------
  // AuthenticationFlows.
  // -------------------------------------------------------------------------

  @Test
  public void whenNotSet_ThenAuthenticationFlowsReturnsDefault() {
    var configuration = new RuntimeConfiguration(Map.of());

    var flows = configuration.authenticationFlows();
    assertEquals(Set.of(), flows);
  }

  @Test
  public void whenSet_ThenAuthenticationFlowsReturnsCollection() {
    var settings = Map.of("AUTH_FLOWS", " flow1,,  flow2 ,");
    var configuration = new RuntimeConfiguration(settings);

    var flows = configuration.authenticationFlows();
    assertEquals(2, flows.size());
    assertEquals(Set.of("flow1", "flow2"), flows);
  }
}
