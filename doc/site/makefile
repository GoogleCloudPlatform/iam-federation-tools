#
# Copyright 2026 Google LLC
#
# Licensed to the Apache Software Foundation (ASF) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The ASF licenses this file
# to you under the Apache License, Version 2.0 (the
# "License"); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
# 
#   http://www.apache.org/licenses/LICENSE-2.0
# 
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.
#

VENV = $(CURDIR)/.venv
PYTHON = $(VENV)/bin/python3

.PHONY: serve install

serve: $(VENV)/bin/mkdocs
	@echo "========================================================"
	@echo "=== Serving site                                     ==="
	@echo "========================================================"
	cd $(CURDIR)/sources && "$(PYTHON)" -m mkdocs serve --livereload

install:
	@echo "========================================================"
	@echo "=== Install mkdocs                                   ==="
	@echo "========================================================"
	test -d "$(VENV)" || python3 -m venv "$(VENV)"
	"$(PYTHON)" -m pip install --upgrade --force-reinstall git+https://github.com/jpassing/mkdocs-material-smallscreen.git

$(VENV)/bin/mkdocs:
	$(MAKE) install
