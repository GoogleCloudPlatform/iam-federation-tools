#
# Copyright 2022 Google LLC
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

#------------------------------------------------------------------------------
# Basic configuration
#------------------------------------------------------------------------------

PRODUCT_VERSION = 1.1
CONFIGURATION = Release
PLATFORM=Any CPU
TARGET_FRAMEWORK = net462

!if ( "$(KOKORO_BUILD_NUMBER)" != "" ) 
BUILD_NUMBER = $(KOKORO_BUILD_NUMBER)
!else
BUILD_NUMBER = 1
!endif

BINARY_VERSION = $(PRODUCT_VERSION).$(BUILD_NUMBER)

ASSEMBLY_DIR = Google.Solutions.WWAuth\bin\$(CONFIGURATION)\$(TARGET_FRAMEWORK)
PACKED_ASSEMBLY = $(ASSEMBLY_DIR)\wwauth.exe
MAIN_ASSEMBLY = $(ASSEMBLY_DIR)\Google.Solutions.WWAuth.exe

NUNIT_ASSEMBLIES = Google.Solutions.WWAuth.Test\bin\$(CONFIGURATION)\$(TARGET_FRAMEWORK)\Google.Solutions.WWAuth.Test.dll
NUNIT_COVERAGE_FILTER = +[Google.Solutions.WWAuth]* 
    
NUGET_PACKAGES = $(USERPROFILE)\.nuget\packages

#------------------------------------------------------------------------------
# Output targets
#------------------------------------------------------------------------------

default: test $(PACKED_ASSEMBLY)

$(MAIN_ASSEMBLY):
	@echo "========================================================"
	@echo "=== Building solution                                ==="
	@echo "========================================================"

	msbuild /t:Rebuild "/p:Configuration=$(CONFIGURATION);Platform=$(PLATFORM);Version=$(BINARY_VERSION)"
	
$(PACKED_ASSEMBLY): $(MAIN_ASSEMBLY)
	@echo "========================================================"
	@echo "=== Packing binary                                   ==="
	@echo "========================================================"
    ILRepack.exe \
        /ver:$(BINARY_VERSION).0 \
        /out:$(PACKED_ASSEMBLY) \
        /lib:$(ASSEMBLY_DIR) \
        /wildcards \
        $(MAIN_ASSEMBLY) \
        $(ASSEMBLY_DIR)\*.dll
restore:
	@echo "========================================================"
	@echo "=== Restoring nuget packages                         ==="
	@echo "========================================================"

	nuget restore

build: $(PACKED_ASSEMBLY)

# NB. The OpenCover version must match the version of the CLR installed
test: $(MAIN_ASSEMBLY)
	@echo "========================================================"
	@echo "=== Testing solution                                 ==="
	@echo "========================================================"

	OpenCover.Console.exe \
		-register:user \
		-returntargetcode \
		-target:nunit3-console.exe \
		"-targetargs:$(NUNIT_ASSEMBLIES) --result=sponge_log.xml;transform=kokoro\nunit-to-sponge.xsl --agents=1" \
		-filter:"$(NUNIT_COVERAGE_FILTER)" \
		"-excludebyattribute:*.SkipCodeCoverage*;*CompilerGenerated*" \
		-output:opencovertests.xml

	$(NUGET_PACKAGES)\ReportGenerator\$(REPORTGENERATOR)\tools\net47\ReportGenerator.exe \
		-reports:opencovertests.xml \
		-targetdir:coveragereport \
		-reporttypes:HTML
        
clean:
    rd /S /Q $(ASSEMBLY_DIR)