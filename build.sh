#!/bin/bash

#
#  Licensed to the Apache Software Foundation (ASF) under one or more
#  contributor license agreements.  See the NOTICE file distributed with
#  this work for additional information regarding copyright ownership.
#  The ASF licenses this file to You under the Apache License, Version 2.0
#  (the "License"); you may not use this file except in compliance with
#  the License.  You may obtain a copy of the License at
#
#  http://www.apache.org/licenses/LICENSE-2.0
#
#  Unless required by applicable law or agreed to in writing, software
#  distributed under the License is distributed on an "AS IS" BASIS,
#  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
#  See the License for the specific language governing permissions and
#  limitations under the License.
#
# ===========================================================================
# Bash functions that can be used in this script or exported by using
# source build.sh

# Stop here if sourcing for functions
[[ "$0" == *"bash" ]] && return 0

# ===========================================================================

# This might not have been sourced if the entrypoint is not bash
[[ -f "$HOME/.cargo/env" ]] && . "$HOME/.cargo/env"

set -xe
cd "${0%/*}"

VERSION=$(<VERSION.txt)

usage() {
  echo "Usage: $0 {test|dist|sign|clean|veryclean|docker|rat|githooks}"
  exit 1
}

(( $# == 0 )) && usage

while (( "$#" ))
do
  target="$1"
  shift

  case "$target" in

    test)

       (dotnet build)
       (dotnet test)

    ;;

    rat)
      mvn apache-rat:check -pl :qpid-proton-dotnet
      ;;

    *)
      usage
      ;;
  esac
done
