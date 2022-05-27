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

# set -xe
cd "${0%/*}"

VERSION=$(<VERSION.txt)

usage() {
  echo "Usage: $0 {test|docs|dist|sign|clean|docker-test|podman-test|rat}"
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

    docs)
      # builds the docs for publication

      if [[ -z "${DOCS_OUTPUT_DIR}" ]]; then
         mkdir -p dist
         DOC_DIR=dist/qpid-proton-dotnet-docs-$VERSION
         rm -rf ${DOC_DIR}
      else
         DOC_DIR=${DOCS_OUTPUT_DIR}
      fi

      # build documentation
      (env DOXYGEN_OUTPUT_PATH=${DOC_DIR} doxygen Proton.dox)
      cp README.md LICENSE NOTICE ${DOC_DIR}
      echo "Wrote documentation to directory: $DOC_DIR"
      ;;

    dist)
      # build source tarball for upload to apache dist staging
      mkdir -p dist

      SRC_DIR=qpid-proton-dotnet-src-$VERSION
      BIN_DIR=qpid-proton-dotnet-bin-$VERSION
      DOC_DIR=qpid-proton-dotnet-docs-$VERSION

      rm -rf "dist/${SRC_DIR}"
      rm -rf "dist/${BIN_DIR}"
      rm -rf "dist/${DOC_DIR}"
      rm -rf "dist/${VERSION}"

      if [ -d .git ]; then
        mkdir -p "dist/${SRC_DIR}"
        mkdir -p "dist/${BIN_DIR}"
        mkdir -p "dist/${DOC_DIR}"
        mkdir -p "dist/${VERSION}"
        git archive HEAD | tar -x -C "dist/${SRC_DIR}"
      else
        echo "Not a GIT repo .. cannot continue"
        exit 255
      fi

      # runs RAT on artifacts
      mvn -N apache-rat:check

      (cd dist; tar czf "../dist/${VERSION}/${SRC_DIR}.tar.gz" "${SRC_DIR}")

      # pack NuGet packages
      dotnet pack --configuration Release Proton.sln

      # Move binary artifacts to dist lib folder
      mkdir -p "dist/${BIN_DIR}/lib"

      cp -R src/Proton/bin/Release/* dist/${BIN_DIR}/lib/
      cp -R src/Proton.Client/bin/Release/* dist/${BIN_DIR}/lib/

      # add the binary LICENSE and NOTICE to the tarball
      cp README.md LICENSE NOTICE dist/${BIN_DIR}

      (cd dist; tar czf "../dist/${VERSION}/${BIN_DIR}.tar.gz" "${BIN_DIR}")

      # build documentation release archive
      (env DOXYGEN_OUTPUT_PATH="dist/${DOC_DIR}" doxygen Proton.dox)
      cp README.md LICENSE NOTICE dist/${DOC_DIR}
      (cd dist; tar czf "../dist/${VERSION}/${DOC_DIR}.tar.gz" "${DOC_DIR}")

      ;;

    sign)
      set +x

      echo -n "Enter password: "
      stty -echo
      read -r password
      stty echo

      for f in $(find dist//${VERSION} -type f \
        \! -name '*.sha512' \! -name '*.sha256' \
        \! -name '*.asc' \! -name '*.txt' \
        -name '*.tar.gz');
      do
        (cd "${f%/*}" && shasum -a 512 "${f##*/}") > "$f.sha512"
        gpg --passphrase "$password" --armor --output "$f.asc" --detach-sig "$f"
      done

      set -x
      ;;

    clean)
      dotnet clean
      rm -rf src/{Proton,Proton.Client,Proton.TestPeer}/{obj,bin}
      rm -rf test/{Proton.Tests,Proton.Client.Tests,Proton.TestPeer.Tests}/{obj,bin}
      rm -rf dist
      rm -rf target
      ;;

    docker-test)
      tar -cf- docker/Dockerfile |
        docker build -t proton-test -f docker/Dockerfile -
      docker run --rm -v "${PWD}:/proton${DOCKER_MOUNT_FLAG}" -w "/proton" --env "JAVA=${JAVA:-11}" proton-test /proton/docker/run-tests.sh
      ;;

    podman-test)
      tar -cf- docker/Dockerfile |
        podman build -t proton-test -f docker/Dockerfile -
      podman run --rm -v "${PWD}:/proton${DOCKER_MOUNT_FLAG}:Z,U" -w "/proton" --env "JAVA=${JAVA:-11}" proton-test /proton/docker/run-tests.sh
      ;;

    *)
      usage
      ;;
  esac
done
