<p align="center">
  <img width="200" alt="NosCore" src="icon.png"/>
</p>

<h1 align="center">NosCore.Networking</h1>

<p align="center">
  <a href="https://www.nuget.org/packages/NosCore.Networking/"><img alt="NuGet" src="https://img.shields.io/nuget/v/NosCore.Networking.svg?style=flat-square"/></a>
  <a href="https://github.com/NosCoreIO/NosCore.Networking/actions/workflows/dotnet.yml"><img alt="Build" src="https://github.com/NosCoreIO/NosCore.Networking/actions/workflows/dotnet.yml/badge.svg?branch=master"/></a>
  <a href="https://discord.gg/Eu3ETSw"><img alt="Discord" src="https://img.shields.io/badge/Discord-join-5865F2?style=flat-square&logo=discord&logoColor=white"/></a>
  <a href="LICENSE"><img alt="MIT License" src="https://img.shields.io/badge/license-MIT-blue?style=flat-square"/></a>
</p>

The socket side of the NosCore servers, on [SuperSocket](https://github.com/kerryjiang/SuperSocket): the login and
world encoders and decoders for the client's packet encryption, frame splitting, per-session state, session groups for
broadcasting, and a spam filter.

## Install

```bash
dotnet add package NosCore.Networking
```

## Contents

| Path | What it holds |
|---|---|
| [Encoding](src/NosCore.Networking/Encoding) | `LoginEncoder`/`LoginDecoder`, `WorldEncoder`/`WorldDecoder`, `FrameDelimiter`, request filters |
| [NetworkClient.cs](src/NosCore.Networking/NetworkClient.cs) | One connected client: its session, region and outgoing packets |
| [SessionGroup](src/NosCore.Networking/SessionGroup) | Groups of sessions a packet can be broadcast to |
| [NetworkManager.cs](src/NosCore.Networking/NetworkManager.cs) | Starts the listener with the pipeline for a login or world server |

## Building

```bash
dotnet build
dotnet test
```

## Contributing

Questions, ideas and pull requests are welcome; most of the discussion happens on [Discord](https://discord.gg/Eu3ETSw).
Any editor that handles .NET 11 works: `dotnet build` and `dotnet test` run on Windows, Linux and macOS.
In Visual Studio, [Roslynator](https://github.com/dotnet/roslynator) and
[ReSharper](https://www.jetbrains.com/resharper/) help keep to the code style.

## Supporting NosCore

NosCore is a spare-time open source project. Sponsoring buys time spent on the emulator and the packages around it,
and gives the **Supporter** role on Discord.

<p>
  <a href="https://github.com/sponsors/0Lucifer0"><img alt="Sponsor on GitHub" src="https://img.shields.io/badge/GitHub-sponsor-EA4AAA?style=for-the-badge&logo=githubsponsors&logoColor=white"/></a>
  <a href="https://ko-fi.com/A3562BQV"><img alt="Support on Ko-fi" src="https://img.shields.io/badge/Ko--fi-support-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white"/></a>
  <a href="https://www.patreon.com/bePatron?u=6503887"><img alt="Become a patron" src="https://img.shields.io/badge/Patreon-become_a_patron-F96854?style=for-the-badge&logo=patreon&logoColor=white"/></a>
</p>

Thanks to [Navicat](https://www.navicat.com) for supporting the project.

<a href="https://www.navicat.com"><img height="60" alt="Navicat" src="https://user-images.githubusercontent.com/35202750/207230064-dcf23adc-9e96-4481-9a53-cd212f5bd60e.png"/></a>

## Legal

NosCore is an independent, unofficial project for educational use. It is not affiliated with, authorized, maintained,
sponsored or endorsed by Gameforge or any of its affiliates; NosTale is a trademark of Gameforge. Using it may be
against the terms of service of NosTale servers.

Released under the [MIT License](LICENSE).
