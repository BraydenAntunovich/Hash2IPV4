# Hash2IPV4
A C# (.NET 8.0) class to assist in reversal of hashes back to their original IPv4 addresses, using [hashcat](https://github.com/hashcat/hashcat).
Designed for cybersecurity professionals and forensic analysts, it automates the cracking process with a focus on ~~MD5~~ hashes (MD5, SHA2, etc.) known to encode IPv4 addresses.

## Features

- Extremely fast, utilising ``hashcat``'s Optimized Kernel.
- Automated hash cracking to IPv4.
- Leverages GPU and CPU via `hashcat`.
- Real-time progress monitoring of `hashcat`.
- Automatic termination of `hashcat` upon completion.
- Hits are cached on-disk via a `hashcat` potfile.
- Cross-platform support.

## Prerequisites

- .NET 8.0 SDK.
- `hashcat` installation. Extract `hashcat`'s [binaries](https://hashcat.net/hashcat/) to a folder, pass the folder path to MD5toIPV4's constructor.
