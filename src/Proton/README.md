# Apache Qpid Proton DotNet

Qpid proton-dotnet is a high-performance, lightweight AMQP protocol engine. It can be used to create AMQP 1.0 nessaging clients and servers or in toolingthat requires a fast AMQP 1.0 protocol codec.

## Adding the engine to your .NET application

Using the `dotnet` CLI you can add a reference to the Qpid proton-dotnet engine to your application which will also download release binaries from the Nuget gallery. The following command
should be run (with the appropriate version updated) in the location where you project file is saved.

    dotnet add package Apache.Qpid.Proton --version 1.0.0-M9

Following this command your 'csproj' file should be updated to contain a reference to to the proton-dotnet protocol engine library and should look similar to the following example:

    <ItemGroup>
      <PackageReference Include="Apache.Qpid.Proton" Version="1.0.0-M9" />
    </ItemGroup>

Users can manually add this reference as well and use the `dotnet restore` command to fetch the artifacts from the Nuget gallery.

Please see http://qpid.apache.org/proton for more information.


