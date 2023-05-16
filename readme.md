# DragonFruit Kaplan
[![DragonFruit Discord](https://img.shields.io/discord/482528405292843018?label=Discord&style=popout)](https://discord.gg/VA26u5Z)

A portable MSIX/APPX removal utility.

## Overview
This is a production-ready tool that can be used to remove APPX/MSIX-based programs from Windows 10/11, including the ability to remove ones that can't be removed by normal means (i.e. Family Safety, Cortana, Camera and Xbox Game Bar).

The "core values" of the project are related to simplicity and ease-of-use:

- Single .exe file
- No additional dependencies are required
- No logs/files are written by the program to the system being used
- Can remove either for the current user (default), or from the entire machine (known as "provisioning packages")

**When using this app you should make sure you understand what you're uninstalling.** If you remove packages and you experience issues, we are unable to provide support.

To minimise potential damage, the app will only list packages installed for the current user by default, but can be extended to remove packages from the machine using the selector in the top left. System packages that cannot be removed are not shown in the list.

### Releases
Releases can be downloaded from the "Releases" tab on the right-hand side of the GitHub repo.

## Usage
1. Download the latest version from the Releases tab
2. Run DragonFruit.Kaplan.exe, allowing it to run as an admin
3. Search for the packages you wish to remove with the searchbar, and select them by clicking on them
4. Press the remove button in the bottom right corner and wait for the removal to finish
    - MSIX/APPX seem to have concurrency restrictions, so if the computer is performing an update some packages may seem to take forever to remove. Don't worry, and if it's taken too long you can restart the program and the apps will be gone.
5. Close the app and send it to the recycle bin (or store it somewhere if you'd like to keep it for future use)

## Development
Kaplan is built as a .NET desktop app, using Avalonia UI and their ReactiveUI MVVM library. You will need to use a Windows desktop and run the IDE as an administrator to allow the app to boot properly
**Debug builds do not uninstall packages**, but fakes the uninstallation process with a delay. Switching to release builds will remove this.

## Contributing
Contributions are welcome in multiple forms. The main ways you can contribute are using the app and reporting bugs, or providing changes through pull requests.

## License
Kaplan is licensed under the Apache License. Refer to [the licence file](license.md) for more information. If you are unsure about any of the license terms, drop us a line at inbox@dragonfruit.network or join our [Discord](https://discord.gg/VA26u5Z) server and ask there.