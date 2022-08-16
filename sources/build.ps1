#
# Copyright 2019 Google LLC
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

$ErrorActionPreference = "stop"

# Use TLS 1.2 for all downloads.
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

${env:__BUILD_ENV_INITIALIZED} = "1"

#------------------------------------------------------------------------------
# Find MSBuild and add to PATH
#------------------------------------------------------------------------------

$Msbuild = (Resolve-Path ([IO.Path]::Combine(${Env:ProgramFiles(x86)}, 
	'Microsoft Visual Studio', '*', '*', 'MSBuild', '*' , 'bin', 'msbuild.exe'))).Path | Select-Object -Last 1
if ($Msbuild)
{
	$MsbuildDir = (Split-Path $Msbuild -Parent)
	$env:Path += ";$MsbuildDir"
}
else
{
	Write-Host "Could not find msbuild" -ForegroundColor Red
	exit 1
}

#------------------------------------------------------------------------------
# Find nmake and add to PATH
#------------------------------------------------------------------------------

$Nmake = (Resolve-Path ([IO.Path]::Combine(${Env:ProgramFiles(x86)}, 
	'Microsoft Visual Studio', '*', '*', 'VC', 'Tools', 'MSVC', '*' , 'bin', 'Hostx86', '*' , 'nmake.exe'))).Path | Select-Object -Last 1
if ($Nmake)
{
	$NMakeDir = (Split-Path $NMake -Parent)
	$env:Path += ";$NMakeDir"
}
else
{
	Write-Host "Could not find nmake" -ForegroundColor Red
	exit 1
}

#------------------------------------------------------------------------------
# Run nmake.
#------------------------------------------------------------------------------

& $Nmake $args

if ($LastExitCode -ne 0)
{
    exit $LastExitCode
}
