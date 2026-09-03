# Redungeon Reborn
> **Disclaimer:** This is an unofficial fan project. All rights to *Redungeon* belong to Nitrome and Eneminds. See [License](#license) for details.

A mod of Redungeon (by Nitrome / Eneminds) that brings the old 2016 game back to modern phones. It works on new devices (arm64), and adds new content: character mods, a Daily Run mode, and more.

# What this mod does?
- **64-bit (arm64) Support** — The original game stopped receiving updates long ago. So it never recieved 64-bit compatibility was required. This build restores native execution on current Android devices.
- **Character Mods** — Brand new mechanics and balance adjustments for Gylbard, Bragg, and Vampire. Toggle them on or off under **Options -> Mods**.
- **Daily Run Mode** — A globally synchronized dungeon that generates a new layout every day at 00:00 UTC. Share your records with other players.
- **General Modifiers** — Includes extra gameplay tweaks (such as *Hardcore Webs*) for more challenge.

## How to install it?
> [!WARNING]  
> Installing the mod requires uninstalling the official game app first. **Your original save data will be erased unless you back it up manually first!**
> New mod updates can be installed **directly** over previous mod releases without losing your save data.
1. Go to the **Releases** section of this repository and download the latest `.apk`.
2. **Uninstall** the original *Redungeon* app from your device (the mod uses a custom signature, so Android will refuse to update over the original store build).
3. Install the downloaded `.apk` file and launch the game.


---

## Troubleshooting & Bug Reporting

### Why is my Daily Run seed different from everyone else's?
Daily seed generation depends on the mod version. If your seed code does not match the community's, ensure you have updated to the latest mod release.

### How do I report a bug?
Open an issue on this repository or create a post on [r/RedungeonGame](https://reddit.com/r/RedungeonGame). Please describe what happened, your device model, and how to reproduce the issue.

To capture diagnostic logs using ADB:
```shell
adb logcat -c
adb logcat | grep -iE "monodroid|AndroidRuntime|redungeon"
```
then run the game and do the bug.


## How to contribute?
Suggestions and feature requests are welcome! Share your ideas on r/RedungeonGame or open an issue on GitHub.
Want to make your own mod? Clone this repository and change what you need. Build it:
```shell
cd src
make install && make launch
```
then use the Makefile to get a signed apk, installed on your device and opened. Test a lot, so nothing breaks.


## How to support?
Being active in the subreddit, sharing your daily scores, and reporting bugs is the best way to support the project!
If you would like to leave a financial tip, you can send Monero (XMR) to the address below:
```text
88zCV1WTwSoAXq6pgqyeNUEDnf3jHuAdtDxmNobbXQRXFEXr7JsaGgV9Hd1FiTKfHwU7KUaLK6Gs7hiDLsrBJ2BT89YZPCf
```

## License
Redungeon was developed by Eneminds and published by Nitrome. I claim no ownership over the original game assets or intellectual property. This project is a non-commercial, fan-made modification. This repository and its releases will be removed immediately upon request by the copyright holders.