# The various SSL stores and certificates were created with the following commands:
# Requires use of JDK 8+ keytool command.


# Clean up existing files
# -----------------------
rm -f *.crt *.csr *.pfx

# Create a key and self-signed certificate for the CA, to sign certificate requests and use for trust:
# ----------------------------------------------------------------------------------------------------
keytool -storetype pkcs12 -keystore ca.pfx -storepass password -keypass password -alias ca -genkey -keyalg "RSA" -keysize 2048 -dname "O=My Trusted Inc.,CN=my-ca.org" -validity 9999 -ext bc:c=ca:true
keytool -storetype pkcs12 -keystore ca.pfx -storepass password -alias ca -exportcert -rfc > ca.crt

# Create a key pair for the server, and sign it with the CA:
# ----------------------------------------------------------
keytool -storetype pkcs12 -keystore server.pfx -storepass password -keypass password -alias server -genkey -keyalg "RSA" -keysize 2048 -dname "O=Server,CN=localhost" -validity 9999 -ext bc=ca:false -ext eku=sA

keytool -storetype pkcs12 -keystore server.pfx -storepass password -alias server -certreq -file server.csr
keytool -storetype pkcs12 -keystore ca.pfx -storepass password -alias ca -gencert -rfc -infile server.csr -outfile server.crt -validity 9999 -ext bc=ca:false -ext eku=sA

keytool -storetype pkcs12 -keystore server.pfx -storepass password -keypass password -importcert -alias ca -file ca.crt -noprompt
keytool -storetype pkcs12 -keystore server.pfx -storepass password -keypass password -importcert -alias server -file server.crt

# Create a key pair for the client, and sign it with the CA:
# ----------------------------------------------------------
keytool -storetype pkcs12 -keystore client.pfx -storepass password -keypass password -alias client -genkey -keyalg "RSA" -keysize 2048 -dname "O=Client,CN=client" -validity 9999 -ext bc=ca:false -ext eku=cA

keytool -storetype pkcs12 -keystore client.pfx -storepass password -alias client -certreq -file client.csr
keytool -storetype pkcs12 -keystore ca.pfx -storepass password -alias ca -gencert -rfc -infile client.csr -outfile client.crt -validity 9999 -ext bc=ca:false -ext eku=cA

keytool -storetype pkcs12 -keystore client.pfx -storepass password -keypass password -importcert -alias ca -file ca.crt -noprompt
keytool -storetype pkcs12 -keystore client.pfx -storepass password -keypass password -importcert -alias client -file client.crt

# Clean up intermediate files
# -----------------------
rm -f *.crt *.csr
