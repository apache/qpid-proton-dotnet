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

FROM alpine:3.17.3
WORKDIR /root

# Install dependencies from vanilla system packages
RUN apk add bash icu-libs krb5-libs libgcc libintl libssl1.1 libstdc++ zlib git doxygen

# Install a maven release  -------------------------------------------
# Inspired from https://github.com/apache/accumulo-docker/blob/master/Dockerfile#L53
ENV MAVEN_VERSION 3.9.1
ENV APACHE_DIST_URLS \
  https://www.apache.org/dyn/closer.cgi?action=download&filename= \
  # if the version is outdated (or we're grabbing the .asc file), we might have to pull from the dist/archive :/
  https://www-us.apache.org/dist/ \
  https://www.apache.org/dist/ \
  https://archive.apache.org/dist/
RUN set -eux; \
  download() { \
    local f="$1"; shift; \
    local distFile="$1"; shift; \
    local success=; \
    local distUrl=; \
    for distUrl in $APACHE_DIST_URLS; do \
      if wget -nv -O "$f" "$distUrl$distFile"; then \
        success=1; \
        break; \
      fi; \
    done; \
    [ -n "$success" ]; \
  }; \
  download "maven.tar.gz" "maven/maven-3/$MAVEN_VERSION/binaries/apache-maven-$MAVEN_VERSION-bin.tar.gz"; \
  tar xzf "maven.tar.gz" -C /tmp/; \
  mv /tmp/apache-maven-$MAVEN_VERSION /opt/maven; \
  rm "maven.tar.gz"
ENV PATH="/opt/maven/bin:${PATH}"

# Install .NET SDK
RUN cd /opt ; \
    wget https://dot.net/v1/dotnet-install.sh ; \
    bash ./dotnet-install.sh --channel "6.0" --install-dir "/opt/dotnet" ;

ENV PATH $PATH:/opt/dotnet

CMD ["/bin/bash", "-i"]
