# Apache Qpid proton-dotnet Client

Qpid Proton DotNet is a high-performance, lightweight AMQP Client that provides an imperative API which can be used in the widest range of messaging applications.

## Adding the client to your .NET application

Using the `dotnet` CLI you can add a reference to the Qpid proton-dotnet client to your application which will also download release binaries from the Nuget gallery. The following command should be run (with the appropriate version updated) in the location where you project file is saved.

    dotnet add package Apache.Qpid.Proton.Client --version 1.0.0-M9

Following this command your 'csproj' file should be updated to contain a reference to to the proton-dotnet client library and should look similar to the following example:

    <ItemGroup>
      <PackageReference Include="Apache.Qpid.Proton.Client" Version="1.0.0-M9" />
    </ItemGroup>

Users can manually add this reference as well and use the `dotnet restore` command to fetch the artifacts from the Nuget gallery.

## Client Documentation

The full client documentation is located in the Qpid proton-dotnet client [here](docs/README.md).

