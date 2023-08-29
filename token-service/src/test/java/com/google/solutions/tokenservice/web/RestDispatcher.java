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

import com.fasterxml.jackson.databind.ObjectMapper;
import com.google.gson.Gson;
import org.jboss.resteasy.core.SynchronousDispatcher;
import org.jboss.resteasy.core.SynchronousExecutionContext;
import org.jboss.resteasy.mock.MockDispatcherFactory;
import org.jboss.resteasy.mock.MockHttpRequest;
import org.jboss.resteasy.mock.MockHttpResponse;
import org.jboss.resteasy.plugins.providers.ServerFormUrlEncodedProvider;
import org.jboss.resteasy.spi.Dispatcher;

import javax.ws.rs.core.MediaType;
import java.io.IOException;
import java.net.URISyntaxException;
import java.net.URLEncoder;
import java.util.Map;

public class RestDispatcher<TResource> {
  private final Dispatcher dispatcher;

  public RestDispatcher(TResource resource) {
    dispatcher = MockDispatcherFactory.createDispatcher();
    dispatcher.getRegistry().addSingletonResource(resource);

    //
    // Register all exception mappers.
    //
    for (var mapper : OAuthExceptionMappers.ALL) {
      dispatcher.getProviderFactory().registerProvider(mapper);
    }

    //
    // Add support for serializing MultivaluedMaps.
    //
    dispatcher.getProviderFactory().registerProviderInstance(
      new ServerFormUrlEncodedProvider(true));
  }

  private <TResponse> Response<TResponse> invoke(
    MockHttpRequest request,
    Class<TResponse> responseType
  ) {
    var response = new MockHttpResponse();
    var synchronousExecutionContext = new SynchronousExecutionContext(
      (SynchronousDispatcher)dispatcher,
      request,
      response);

    request.setAsynchronousContext(synchronousExecutionContext);
    dispatcher.invoke(request, response);
    return new Response<>(response, responseType);
  }

  public <TResponse> Response<TResponse> get(
    String path,
    Class<TResponse> responseType
  ) throws URISyntaxException {
    return invoke(MockHttpRequest.get(path), responseType);
  }

  public <TResponse> Response<TResponse> post(
    String path,
    Class<TResponse> responseType
  ) throws URISyntaxException {
    return invoke(MockHttpRequest.post(path), responseType);
  }

  public <TRequest, TResponse> Response<TResponse> post(
    String path,
    TRequest request,
    Class<TResponse> responseType
  ) throws URISyntaxException {
    var mockRequest = MockHttpRequest.post(path)
      .accept(MediaType.APPLICATION_JSON)
      .contentType(MediaType.APPLICATION_JSON_TYPE)
      .content(new Gson().toJson(request).getBytes());

    return invoke(mockRequest, responseType);
  }

  public <TRequest, TResponse> Response<TResponse> postForm(
    String path,
    Map<String, String> parameters,
    Class<TResponse> responseType
  ) throws URISyntaxException {
    var body = parameters
      .entrySet()
      .stream()
      .map(p -> URLEncoder.encode(p.getKey()) + "=" + URLEncoder.encode(p.getValue()))
      .reduce((p1, p2) -> p1 + "&" + p2)
      .orElse("");

    var mockRequest = MockHttpRequest.post(path)
      .accept(MediaType.APPLICATION_JSON_TYPE)
      .contentType(MediaType.APPLICATION_FORM_URLENCODED)
      .content(body.getBytes());

    return invoke(mockRequest, responseType);
  }

  public static class Response<T> {
    private final MockHttpResponse mockResponse;
    private final Class<T> responseType;

    public Response(MockHttpResponse mockResponse, Class<T> responseType) {
      this.mockResponse = mockResponse;
      this.responseType = responseType;
    }

    public int getStatus() {
      return this.mockResponse.getStatus();
    }

    public T getBody() throws IOException {
      //
      // NB. GSON ignores JsonProperty annotations on records. Use
      // Jackson instead.
      //
      return new ObjectMapper()
        .reader()
        .readValue(this.mockResponse.getContentAsString(), responseType);
    }
  }
}
