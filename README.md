# Simplified SAFE Stack

A lightweight alternative template to the full-fledged official [SAFE Template](https://github.com/SAFE-Stack/SAFE-template). Lowers the entry barrier by choosing the simplest possible opinionated defaults:
 - Nuget for package management
 - [FAKE](https://fake.build/) build script as a console project (see ./build)
 - [Saturn](https://github.com/SaturnFramework/Saturn) as server web framework
 - [Fable.Remoting](https://github.com/Zaid-Ajaj/Fable.Remoting) for client-server communications
 - [Feliz](https://github.com/Zaid-Ajaj/Feliz) as the React DSL on the front-end
 - [Expecto](https://github.com/haf/expecto) for server unit-tests project
 - [Fable.Mocha](https://github.com/Zaid-Ajaj/Fable.Mocha) for client unit-tests project (runs in Node.js when on CI servers or live during development)
 - [Serilog](https://serilog.net) for logging server-side stuff
 - Scalable architecture by modelling logical server-side components following Fable.Remoting protocols
 - F# Analyzers support
 - Simple application variable configuration (see below sections)

### Getting Started

To start using this template, simply clone this repository or use it as template via Github UI and you are good to go. 

You need to have installed: 
  - [.NET Core SDK 3.1+](https://dotnet.microsoft.com/download)
  - [Node.js 12.0+](https://nodejs.org/en)

You can use the editor of your choice to work with the repository. [VS Code](https://code.visualstudio.com) is recommended with the [Ionide](http://ionide.io) extension for F# development but the template will also work just fine with [Visual Studio](https://visualstudio.microsoft.com/vs/), [Rider](https://www.jetbrains.com/rider) or [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac) if you prefer to work with any of those.


### Running The Application

To work with and develop the application, you need to both the server and the client project running at the same time. The server application is in the `server` directory and the client is in `client` directory. To run them both, simply open two shell tabs, each for either applications then:
```bash
  Shell tab c:\project\simplified-safe   Shell tab c:\project\simplified-safe
 -------------------------------------- --------------------------------------
  > cd server                            > cd client
  > dotnet restore                       > npm install
  > dotnet run                           > npm start
```
As shown here below

![img](docs/running-the-application.gif)

The server web application starts listening for requests at `http://localhost:5000` where as the client application will be hosted at `http://localhost:8080` during developments. All web requests made from the front-end are automatically proxied to the backend at `http://localhost:5000`. In production, there will no proxy because the front-end application will be served from the backend itself.

> That is unless you are hosting the backend serverless and would like to host the front-end project separately.

### Available Build Targets

You can easily run the build targets as follows:
 - `./build.sh {Target}` on Linux, Mac or simulated bash on Windows
 - `build {Target}` on Windows
 - Hitting F5 where `Build.fsproj` is the startup project in Visual Studio or Rider

There are a bunch of built-in targets that you can run:
 - `Server` builds the server in Release mode
 - `Client` builds the client in production mode
 - `Clean` cleans up cached assets from all the projects
 - `ServerTests` runs the server unit-tests project
 - `ClientTests` runs the client unit-tests project by compiling the project first and running via Mocha in node.js
 - `LiveClientTests` runs a standalone web application at `http://localhost:8085` that shows test results from the unit tests and recompiles whenever the tests change.
 - `HeadlessBrowserTests` builds the test project as web application and spins up a headless browser to run the tests and report results
 - `Pack` builds and packs both server and client into the `{solutionRoot}/dist` directory after running unit tests of both projects. You can run the result application using `dotnet Server.dll` in the `dist` directory.
 - `PackNoTests` builds and packs both server and client projects into `{solutionRoot}/dist` without running tests.
 - `InstallAnalyzers` installs [F# code analyzers](https://github.com/ionide/FSharp.Analyzers.SDK). You can configure which analyzers to install from the build target itself.

### Configuring application variables: Server

The server web application picks up the environment variables by default from the host machine and makes them available from an injected `IConfiguration` interface. However, it adds a nice feature on top which allows to add more application-specific local variables by adding a JSON file called `config.json` inside your `server` directory:
```json
{
  "DATABASE_CONNECTIONSTRING": "ConnectionString",
  "APP_NAME": "SimplifiedSafe",
  "VERSION": "0.1.0-alpha"
}
```
Just including the file will allow the variables to be picked up automatically and will also be made available through the `IConfiguration` interface.

### Configuring application variables: Client

Even the client can use build variables. Using the `Config.variable : string -> string` function, you can have access to the environment variables that were used when the application was compiled. Webpack will pick them up automatically by default. To use local variables other than the environment variables, you add a file called `.env` into the `client` directory. This file is a dotenv variables file and has the following format:
```
KEY1=VALUE1
KEY2=VALUE2
WELCOME_MESSAGE=Welcome to full-stack F#
```
Then from your Fable application, you can use the variables like this:
```fs
Config.variable "WELCOME_MESSAGE" // returns "Welcome to full-stack F#"
```
Since this file can contain variables that might contain sensitive data. It is git-ignored by default.

### Injecting ASP.NET Core Services

Since we are using Fable.Remoting in the template, make sure to check out the [Functional Dependency Injection](https://zaid-ajaj.github.io/Fable.Remoting/src/dependency-injection.html) article from the documentation of Fable.Remoting that goes through the required steps of injecting services into the functions of Fable.Remoting APIs

### F# Analyzers support

When developing the application using Ionide and VS Code, you can make use of F# analyzers that are built to detect certain types of specific pieces of code. By default the template doesn't include any analyzers but it is easy to add and install them using the `InstallAnalyzers` build target defined in `build/Program.fs` as follows:
```fs
Target.create "InstallAnalyzers" <| fun _ ->
    let analyzersPath = path [ solutionRoot; "analyzers" ]
    Analyzers.install analyzersPath [
        // Add analyzer entries to download
        // example { Name = "NpgsqlFSharpAnalyzer"; Version = "3.2.0" }
    ]
```
To install for example the [NpgsqlFSharpAnalyzer](https://github.com/Zaid-Ajaj/Npgsql.FSharp.Analyzer) package, simply uncomment the entry to make the code look like this:
```fs
Target.create "InstallAnalyzers" <| fun _ ->
    let analyzersPath = path [ solutionRoot; "analyzers" ]
    Analyzers.install analyzersPath [
        // Add analyzer entries to download
        { Name = "NpgsqlFSharpAnalyzer"; Version = "3.2.0" }
    ]
```
Then run the build target `InstallAnalyzers` again where it will delete the contents of `analyzers` directory and re-install all configured analyzers from scratch. Restart VS Code to allow Ionide to reload the installed analyzers. If you already have analyzers installed and adding new ones, you might need to do that from the terminal outside of VS Code because Ionide will lock the files in the `analyzers` path preventing the target from deleting the old analyzers.

### IIS Support

The bundled application you get by running the `Pack` build target can be used directly as an application inside of IIS. Publishing on IIS requires that you make a separate Application Pool per .NET Core application with selected .NET CLR Version = `No Managed Code`. Then creating a new IIS Application which into the newly created Application Pool and setting the Physical Path of that Application to be the `dist` directory.