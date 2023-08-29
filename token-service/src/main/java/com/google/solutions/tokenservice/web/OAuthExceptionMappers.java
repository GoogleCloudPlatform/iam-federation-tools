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

import org.jboss.resteasy.spi.UnhandledException;

import javax.ws.rs.NotAcceptableException;
import javax.ws.rs.NotAllowedException;
import javax.ws.rs.NotFoundException;
import javax.ws.rs.core.Response;
import javax.ws.rs.ext.ExceptionMapper;
import javax.ws.rs.ext.Provider;
import java.io.IOException;

public class OAuthExceptionMappers {
  public static final Class<?>[] ALL = new Class<?>[] {
    NullPointerExceptionMapper.class,
    IOExceptionMapper.class,
    UnhandledExceptionMapper.class,
    NotAllowedExceptionMapper.class,
    NotAcceptableExceptionMapper.class,
    NotFoundExceptionMapper.class
  };
  @Provider
  public static class NullPointerExceptionMapper implements ExceptionMapper<NullPointerException> {
    @Override
    public Response toResponse(NullPointerException exception) {
      return Response
        .status(Response.Status.BAD_REQUEST)
        .entity(new OAuthResource.TokenErrorResponse(OAuthResource.TokenErrorResponse.SERVER_ERROR, exception))
        .build();
    }
  }

  @Provider
  public static class IOExceptionMapper implements ExceptionMapper<IOException> {
    @Override
    public Response toResponse(IOException exception) {
      return Response
        .status(Response.Status.BAD_GATEWAY)
        .entity(new OAuthResource.TokenErrorResponse(OAuthResource.TokenErrorResponse.TEMPORARILY_UNAVAILABLE, exception))
        .build();
    }
  }

  @Provider
  public static class NotAllowedExceptionMapper implements ExceptionMapper<NotAllowedException> {
    @Override
    public Response toResponse(NotAllowedException exception) {
      return Response
        .status(Response.Status.METHOD_NOT_ALLOWED)
        .entity(new OAuthResource.TokenErrorResponse(OAuthResource.TokenErrorResponse.INVALID_REQUEST, exception))
        .build();
    }
  }

  @Provider
  public static class NotAcceptableExceptionMapper implements ExceptionMapper<NotAcceptableException> {
    @Override
    public Response toResponse(NotAcceptableException exception) {
      return Response
        .status(Response.Status.NOT_ACCEPTABLE)
        .entity(new OAuthResource.TokenErrorResponse(OAuthResource.TokenErrorResponse.INVALID_REQUEST, exception))
        .build();
    }
  }

  @Provider
  public static class NotFoundExceptionMapper implements ExceptionMapper<NotFoundException> {
    @Override
    public Response toResponse(NotFoundException exception) {
      return Response
        .status(Response.Status.NOT_FOUND)
        .entity(new OAuthResource.TokenErrorResponse(OAuthResource.TokenErrorResponse.INVALID_REQUEST, exception))
        .build();
    }
  }

  @Provider
  public static class UnhandledExceptionMapper implements ExceptionMapper<UnhandledException> {
    @Override
    public Response toResponse(UnhandledException exception) {
      return Response
        .status(Response.Status.INTERNAL_SERVER_ERROR)
        .entity(new OAuthResource.TokenErrorResponse(OAuthResource.TokenErrorResponse.SERVER_ERROR, exception))
        .build();
    }
  }
}
