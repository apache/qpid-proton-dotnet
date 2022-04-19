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

       (dotnet build --configuration Release Proton.sln)
       (LANG=en_US.UTF-8 dotnet test --configuration Release --no-build Proton.sln)

    ;;

    rat)
      mvn apache-rat:check -pl :qpid-proton-dotnet
      ;;

    dist)
      # build source tarball for upload to apache dist staging
      mkdir -p dist

      SRC_DIR=qpid-proton-dotnet-src-$VERSION
      BIN_DIR=qpid-proton-dotnet-bin-$VERSION

      rm -rf "dist/${SRC_DIR}"
      rm -rf "dist/${BIN_DIR}"

      if [ -d .git ]; then
        mkdir -p "dist/${SRC_DIR}"
        mkdir -p "dist/${BIN_DIR}"
        git archive HEAD | tar -x -C "dist/${SRC_DIR}"
      else
        echo "Not a GIT repo .. cannot continue"
        exit 255
      fi

      # runs RAT on artifacts
      mvn -N apache-rat:check

      (cd dist; tar czf "../dist/${SRC_DIR}.tar.gz" "${SRC_DIR}")

      # pack NuGet packages
      dotnet pack --configuration Release Proton.sln

      # Move binary artifacts to dist lib folder
      mkdir -p "dist/${BIN_DIR}/lib"

      cp -R src/Proton/bin/Release/* dist/${BIN_DIR}/lib/
      cp -R src/Proton.Client/bin/Release/* dist/${BIN_DIR}/lib/

      # add the binary LICENSE and NOTICE to the tarball
      cp README.md LICENSE NOTICE dist/${BIN_DIR}

      (cd dist; tar czf "../dist/${BIN_DIR}.tar.gz" "${BIN_DIR}")

      ;;

    clean)
      rm -rf src/{Proton,Proton.Client,Proton.TestPeer}/{obj,bin}
      rm -rf test/{Proton.Tests,Proton.Client.Tests,Proton.TestPeer.Tests}/{obj,bin}
      rm -rf dist
      rm -rf target
      ;;

    *)
      usage
      ;;
  esac
done
