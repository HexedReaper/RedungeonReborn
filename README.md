<div align="center">

# Redungeon Reborn

![Android](https://img.shields.io/badge/Platform-Android_arm64-3DDC84?style=flat-square&logo=android&logoColor=white)
![Build](https://img.shields.io/badge/Build-v1.0.0-informational?style=flat-square)
![License](https://img.shields.io/badge/License-Non--Commercial-lightgrey?style=flat-square)
![Monero](https://img.shields.io/badge/Donate-Monero-FF6600?style=flat-square&logo=monero&logoColor=white)

An unofficial arm64 modernization and modding project for *Redungeon*.

</div>

> **Disclaimer:** This is an unofficial fan project. All rights to *Redungeon* belong to Nitrome and Eneminds. See [License](#license) for details.

A mod of *Redungeon* (by Nitrome / Eneminds) that brings the 2016 game back to life on modern phones. It adds native **arm64** support for new devices and introduces new content, including character mods, a Daily Run mode, and more.

## Features

| Feature | Description |
| :--- | :--- |
| **Architecture** | Native **arm64** support for modern Android devices. |
| **Modifications** | New mechanics for Gylbard, Bragg, and Vampire (toggleable under *Options → Mods*). |
| **Daily Run** | Globally synchronized seed synced at 00:00 UTC. |
| **Modifiers** | Custom gameplay tweaks like *Hardcore Webs*. |

## Installation

> [!WARNING]
> Installing the mod requires uninstalling the official game app first. **Your original save data will be erased unless you back it up manually first!**
> New mod updates can be installed **directly** over previous mod releases without losing your save data.

1. Go to the **Releases** section of this repository and download the latest `.apk`.
2. **Uninstall** the original *Redungeon* app from your device (the mod uses a custom signature, so Android will refuse to update over the original store build).
3. Install the downloaded `.apk` file and launch the game.

## Backup Guide

<details>
<summary><b>Option 1: ADB (No Root)</b></summary>

1. Enable **Developer Options** in your phone settings (process varies by device; search online for specific instructions).
2. Go to **Developer Options** > **Enable USB Debugging**.
3. Connect your phone to the computer. When prompted with "Allow this device to use USB debugging?", tick **"Always allow"** and tap **"Yes"**.
4. Open a terminal and verify the connection:
   ```bash
   adb devices
   ```
5. Create a `.ab` backup of the game:
   ```bash
   adb backup -f redungeon_save.ab -noapk com.nitrome.redungeon
   ```
6. To restore the backup later:
   ```bash
   adb restore redungeon_save.ab
   ```

</details>

<details>
<summary><b>Option 2: Root Method</b></summary>

1. Use [AppManager](https://github.com/MuntashirAkon/AppManager) with root access granted.
2. Navigate to: **AppManager** > `⋮` (three dots) > **Settings** > **Backup/restore**.
3. Enable **Back up apps with Android Keystore**.
4. Return to the main menu, find **Redungeon**, tap `⋮` (three dots), and select **Backup/Restore**.

</details>

---

## Troubleshooting & Bug Reporting

### Why is my Daily Run seed different from everyone else's?
Daily seed generation depends on the mod version. If your seed code does not match the community's, ensure you have updated to the latest mod release.

### How do I report a bug?
Open an issue on this repository or create a post on [r/RedungeonGame](https://reddit.com/r/RedungeonGame). Please describe what happened, your device model, and steps to reproduce the issue.

To capture diagnostic logs using ADB:
```shell
adb logcat -c
adb logcat | grep -iE "monodroid|AndroidRuntime|redungeon"
```
Then, run the game and trigger the bug.

## Contributing

Suggestions and feature requests are welcome! Share your ideas on r/RedungeonGame or open an issue on GitHub.

Want to make your own mod? Clone this repository, make your changes, build it, and install the signed APK:

```shell
cd src
make install && make launch
```

Please test thoroughly to ensure stability.

## Support

Being active in the subreddit, sharing your daily scores, and reporting bugs is the best way to support the project!

If you would like to leave a financial tip, you can send Monero (XMR) to the address below:

```text
88zCV1WTwSoAXq6pgqyeNUEDnf3jHuAdtDxmNobbXQRXFEXr7JsaGgV9Hd1FiTKfHwU7KUaLK6Gs7hiDLsrBJ2BT89YZPCf
```

## License

*Redungeon* was developed by Eneminds and published by Nitrome. I claim no ownership over the original game assets or intellectual property. This project is a non-commercial, fan-made modification. This repository and its releases will be removed immediately upon request by the copyright holders.