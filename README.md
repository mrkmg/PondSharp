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

How to Run
----------

You need:
- This project cloned.
- NodeJS (I use v16, but should work with many versions of Node)
- .net6 SDK

Install node dependencies

    cd Client
    npm install

Run app

    cd Client
    dotnet run

Publish

    cd Client
    dotnet publish -c Release -o ../Build

After the publish command is finished, you can host the project in Build/wwwroot

