PondSharp
====

A tech demo of in-browser C# (.NET Standard) compilation and PixiJS WebGL.

**Who:** This project was made by [Kevin Gravier](https://github.com/mrkmg) and open sourced under the MIT License.

**What:**  A simulation to tech demo in browser C# .Net Standard compilation and execution using 
    [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor),
    [Bootstrap](https://getbootstrap.com/),
    [PixiJS](https://www.pixijs.com/), and
    [Monaco Editor](https://microsoft.github.io/monaco-editor/).

**Where:** [https://mrkmg.github.io/PondSharp/](https://mrkmg.github.io/PondSharp/)

**When:** © 2020 Kevin Gravier (MIT License).

**Why:** This was made mostly out of curiosity of the ability to compile and run c# in the browser with Blazor. This may or may not turn into something more.


------

### Building/Developing Prerequisites

You need:
- This project cloned
- NodeJS and NPM installed
- DotNet 6.0 SDK

## How to Run for Development

In a terminal, at the root of the solution run:
```bash
    # change directory to the Client project
    cd Client
    
    # restore dependencies
    dotnet restore
    
    # build the project. Will also run npm install, and webpack build
    dotnet build
    
    # run the project...
    
    # ...as Debug
    dotnet run -c Debug --launch-profile Pondsharp.Client.Debug
    # ...as Release
    dotnet run -c Release --launch-profile Pondsharp.Client.Release
```

Then open your browser to http://localhost:5500/

## How to build for Deployment

```bash
    cd Client
    dotnet restore
    dotnet publish -c Release
    cd ..
```

You can then deploy the result of ./build/wwwroot to any web host. *You must
host it on a server, as wasm does not run when opened via file://*

