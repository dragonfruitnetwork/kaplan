
# DragonFruit Kaplan
[![DragonFruit Discord](https://img.shields.io/discord/482528405292843018?label=Discord&style=popout)](https://discord.gg/VA26u5Z)

A portable MSIX/APPX removal utility.

## Overview
This is a production-ready tool that can be used to remove APPX/MSIX-based programs from Windows 10/11, including the ability to remove "stubs" (i.e. ESPN, TikTok and Instagram) with two clicks, and remove packages for either the current user or the entire machine.

The "core values" of the project are related to simplicity and ease-of-use:

- Single .exe file
- No additional dependencies are required
- No logs/files are written by the program to the system being used

When using this app you should make sure you understand what packages you're uninstalling. Removing some packages may cause issues with other apps (for example, removing "Your Phone" will cause the settings app crash when opening the Phone section). To minimise potential damage, the app will only list packages installed for the current user by default, but can be extended to remove packages from the machine using the selector in the top left.

Releases can be downloaded from the "Releases" tab on the right-hand side of the GitHub repo.

## Usage

1. Download the latest version from the Releases tab
2. Run DragonFruit.Kaplan.exe, allowing it to run as an admin
3. Search for the packages you wish to remove with the searchbar, and select them by clicking on them (or if you wish to remove the ~~ads~~"stubs", use the select stubs button in the bottom right hand cornder)
4. Press the remove button in the bottom right corner and wait for the removal to finish
    - MSIX/APPX seem to have concurrency restrictions, so if the computer is performing an update some packages may seem to take forever to remove. Don't worry, and if it's taken too long you can restart the program and the apps will be gone.
5. Close the app and send it to the recycle bin (or store it somewhere if you'd like to keep it for future reference)

## Development
Kaplan is built as a .NET desktop app, using Avalonia UI and their ReactiveUI MVVM library. You will need to use a Windows desktop and run the IDE as an administrator to allow the app to boot properly

### Downloading source code
Clone the repo using git:

```
git clone https://github.com/dragonfruitnetwork/sakura DragonFruit.Sakura
cd DragonFruit.Sakura
```

Double-clicking the `.sln` or a `.csproj` file inside the folder should open your IDE with all the files listed in a sidebar. VS Code users may need to open the project as a folder instead before being able to view the structure.

**Debug builds do not uninstall packages**, but fake the uninstallation process with a delay. Switching to release builds will remove this issue.

## Contributing

Contributions are welcome in multiple forms. The main ways you can contribute are using the app and reporting bugs, or providing changes through pull requests. Feedback is welcome - it helps the project advance and enables more people to get involved.

## License
Kaplan is licensed under the Apache License. Refer to [the licence file](license.md) for more information. If you are unsure about any of the license terms, drop us a line at inbox@dragonfruit.network or join our [Discord](https://discord.gg/VA26u5Z) server and ask there.