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

import com.fasterxml.jackson.annotation.JsonProperty;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.google.common.base.Preconditions;

import javax.enterprise.context.RequestScoped;
import java.io.IOException;
import java.util.HashMap;
import java.util.Map;
import java.util.function.Consumer;

/**
 * Adapter class for writing structured logs.
 */
@RequestScoped
public class LogAdapter {
  private final Appendable output;

  private String traceId;

  public LogAdapter(Appendable output) {
    Preconditions.checkNotNull(output);
    this.output = output;
  }

  public LogAdapter() {
    this(System.out);
  }

  /**
   * Set Trace ID for current request.
   */
  public void setTraceId(String traceId) {
    this.traceId = traceId;
  }

  public LogEntry newInfoEntry(String eventId, String message) {
    return new LogEntry("INFO", eventId, message, this.traceId);
  }

  public LogEntry newWarningEntry(String eventId, String message) {
    return new LogEntry("WARNING", eventId, message, this.traceId);
  }

  public LogEntry newErrorEntry(String eventId, String message) {
    return new LogEntry("ERROR", eventId, message, this.traceId);
  }

  public LogEntry newErrorEntry(String eventId, String message, Exception e) {
    return new LogEntry(
      "ERROR",
      eventId,
      String.format("%s: %s", message, e.getMessage()),
      this.traceId);
  }

  //---------------------------------------------------------------------
  // Inner classes.
  //---------------------------------------------------------------------

  /**
   * Entry that, when serialized to JSON, can be parsed and interpreted by Cloud Logging.
   */
  public class LogEntry {
    @JsonProperty("severity")
    private final String severity;

    @JsonProperty("message")
    private final String message;

    @JsonProperty("logging.googleapis.com/labels")
    private final Map<String, String> labels;

    @JsonProperty("logging.googleapis.com/trace")
    private final String traceId;

    private LogEntry(
      String severity,
      String eventId,
      String message,
      String traceId
    ) {
      this.severity = severity;
      this.message = message;
      this.traceId = traceId;

      this.labels = new HashMap<>();
      this.labels.put("event", eventId);
    }

    public LogEntry addLabel(String label, String value) {
      assert !this.labels.containsKey(label);

      this.labels.put(label, value);
      return this;
    }

    public LogEntry addLabels(Consumer<LogEntry> func) {
      func.accept(this);
      return this;
    }

    /**
     * Emit the log entry to the log.
     */
    public void write() {
      try {
        //
        // Write to STDOUT, AppEngine picks it up from there.
        //
        output.append(new ObjectMapper().writeValueAsString(this)).append("\n");
      }
      catch (IOException e) {
        try {
          output.append(String.format("Failed to log: %s\n", message));
        }
        catch (IOException ignored) {
        }
      }
    }
  }
}
