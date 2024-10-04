# Installation
Welcome!
This is the installation guide for STELLA.
Below are the types of installation you can use: 


***
### Table of Contents:
- [Requirements and Reccomendations](https://github.com/trademark-claim/laughing-octo-garbanzo/blob/master/Installation.md#requirements-and-reccomendations)
    - [Software](https://github.com/trademark-claim/laughing-octo-garbanzo/blob/master/Installation.md#software)
    - [Hardware](https://github.com/trademark-claim/laughing-octo-garbanzo/blob/master/Installation.md#hardware)
- [Installation Methods](https://github.com/trademark-claim/laughing-octo-garbanzo/blob/master/Installation.md#installation-methods)
	- [Command Line](https://github.com/trademark-claim/laughing-octo-garbanzo/blob/master/Installation.md#command-line-1)
	- [Manual Download](https://github.com/trademark-claim/laughing-octo-garbanzo/blob/master/Installation.md#manual-download-1)
- [Installation FAQ](https://github.com/trademark-claim/laughing-octo-garbanzo/blob/master/Installation.md#installation-faq)

---

## Requirements and Reccomendations
_Below are the required and reccomended software and hardware setups for STELLA_

### Software 

#### Required Software

| Software               | Version            | Description                                                                        | Download Link                                | Notes                                            |
|------------------------|--------------------|------------------------------------------------------------------------------------|---------------------------------------------|--------------------------------------------------|
| Windows^1^             | 10 (version 1607 or later) |  Minimal Operating System                                                  | [Windows 10](https://www.microsoft.com/software-download/windows10) | Ensure latest updates are installed              |
| .NET Runtime           | 8.0                | Required to run .NET 8.0 applications                                              | [Download .NET Runtime 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) | Needed for running the application               |
| Internet Connection (SW)| -                 | For full functionality including crawling and automated downloads                  | -                                           | Essential for internet-based features            |
| DirectX                | 9                  | For handling animations and graphical operations                                   | [Download DirectX](https://www.microsoft.com/en-us/download/details.aspx?id=35) | Needed for advanced graphical operations (Comes with Windows natively but ensure existence)         |

#### Recommended Software

| Software               | Version            | Description                                                                        | Download Link                               | Notes                                            |
|------------------------|--------------------|------------------------------------------------------------------------------------|---------------------------------------------|--------------------------------------------------|
| Windows^1^             | 11                 | Targeted OS                                                                        | [Download Windows 11](https://www.microsoft.com/software-download/windows11)   | Ensure latest update for full functionality     |
| DirectX                | 12                 | Targest DirectX version with full compatibility with the application               | [Download DirectX](https://www.microsoft.com/en-us/download/details.aspx?id=35)| ___A___ DirectX version should be natively on your system already | 

#### Optional Software

| Software               | Version            | Description                                                                                         | Download Link                                          | Notes                                            |
|------------------------|--------------------|-----------------------------------------------------------------------------------------------------|--------------------------------------------------------|--------------------------------------------------|
| ILSpy                  | Any                | Useful for decompiling the application to see source code -- required if contributing or debugging. | [Download ILSpy](https://github.com/icsharpcode/ILSpy) | Use with an IDE (See below reccomended IDE)      |
| Visual Studio          | 2022.16 or above   | Pair with the above decompiler to view and edit source code                                         | [Download VS2022](https://c2rsetup.officeapps.live.com/c2r/downloadVS.aspx?sku=community&channel=Release&version=VS2022&source=VSLandingPage&cid=2030:4e04693253734cf4a7f886a30271d77c) | Use with decompiler (See above decompiler), This is just the reccomended IDE, but any with .NET support works. |
| SuperF4                | 1.4                | Forcibly terminates active processes with `Control` `Alt` `F4`, where the normal `Alt` `F4` just asks the program to quit. Useful for debugging or just generally if STELLA crashes as it may overlay the screen with an unclosable, unminimizable blocking window. | [Download SuperF4](https://objects.githubusercontent.com/github-production-release-asset-2e65be/15859512/a2874280-31f6-11e9-8949-8dbd13a7a68f?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=releaseassetproduction%2F20240514%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20240514T081612Z&X-Amz-Expires=300&X-Amz-Signature=805ab1bdf1accc79f93f5798e3186208048a90b57fcbe8ab119971dcfb176df4&X-Amz-SignedHeaders=host&actor_id=110950992&key_id=0&repo_id=15859512&response-content-disposition=attachment%3B%20filename%3DSuperF4-1.4.exe&response-content-type=application%2Foctet-stream) | Highly reccomend |
| 

#### Required Libraries
_Included in Published Application, Included for troubleshooting purposes_

| Library                | Version                | Description                                                                        | Notes                                            |
|------------------------|------------------------|------------------------------------------------------------------------------------|--------------------------------------------------|
| Newtonsoft.Json        | 13.0.3                 | For JSON serialization and deserialization                                         | Useful                                           |
| AspectInjector         | 2.8.3-pre-3            | For Aspect Injection used for logging and exception handling                       | Quite Useful                                     |
| ini-parser             | 2.5.2                  | For ini serialization and deserialization                                          | Useful                                           |
| NAudio                 | 2.2.1                  | For interfacing natively with system audio (HW & SW)                               | Useful, Dont need excessive PInvoke              |
| SevenZipExtractor      | 1.0.17                 | For Extracting .7z files                                                           | Useful                                           |
| SharpCompress          | 0.36.0                 | For Extracting .zip files                                                          | Useful                                           |
| System.Management      | 9.0.0-preview.3.24172.9| For getting detailed information on the host system                                | Useful                                           |
| System.Speech          | 9.0.0-preview.3.24172.9| For the STT functionality                                                          | EXTREMELY Useful                                 |
| System                 | 9.0.0-preview.3.24172.9| Natively included library for 99% of functionality.                                | Obscenely Useful                                 |

### Hardware
| Component              | Minimum            | Recommended                                                                        | Notes                                            |
|------------------------|--------------------|------------------------------------------------------------------------------------|--------------------------------------------------|
| Processor              | 1 GHz or faster    | 2 GHz or faster                                                                    | Dual-core or better                              |
| RAM                    | 1 GB^2^            | 4 GB                                                                               | More RAM improves performance                    |
| Disk Space             | 200mb^3^           | 1 GB free space^3^                                                                 | For installation and application data            |
| Graphics               | DirectX 9 or later | DirectX 11 or later                                                                | For better graphical performance                 |
| Internet Connection    | Required for full functionality | Broadband internet connection recommended | Essential for web-based features                  |


###### Footnotes:
^1^ -- Windows is the currently supported operating system, but Stella is aimed to be run on both Linux and MacOS in the near future, stay tuned!

^2^ -- This is for bare minimum functionality, for use of graphical operations please have 4GB or more for a smooth experience.

^3^ -- If used for alot of screenshotting, video saving and logging, then change the minimum and maximum to 10GB and 50GB respectively, scaled to how much you'll be using high-storage functionalities (Far higher range as videos are dynamic and may range from a few mb (short videos) to a few GB (longer videos))

---

### Installation Methods
#### [Command Line](https://github.com/trademark-claim/laughing-octo-garbanzo/blob/master/Installation.md#command-line-1)
_This uses the terminal / powershell / the command line interface and a sequence of win commands to install the compiled STELLA to a chosen directory._
#### [Manual Download](https://github.com/trademark-claim/laughing-octo-garbanzo/blob/master/Installation.md#manual-download-1)
_Manually Download STELLA through Google Drive to your downloads folder._
*** *** ***
### Command Line
1. Open your PowerShell, either through the:
    1. 	<details>
			<summary>Terminal</summary>
			<ol>
			  <li>Open Terminal (below)</li>
			  <li>Hold down the Left <code>Control</code> button</li>
			  <li>Hold down the Left <code>Shift</code> button</li>
			  <li>Press the <code>1</code> button</li>
			  <li>Release the <code>Control</code> and <code>Shift</code> buttons</li>
              <img src="Images/terminal.png" alt="terminal">
			</ol>
      </details>
	
   ii. <details>
			<summary>Powershell</summary>
			<ol>
			  <li>Open Powershell (below)</li>
              <img src="Images/ps.png" alt="powershell">
			</ol>
      </details>
	
	Your Screen should look like this:
	![pss](Images/pss.png)
	Note: You may have something other than `Default`, this is normal.
2. Copy and paste the code ([Code](https://github.com/trademark-claim/laughing-octo-garbanzo/blob/master/Installation.md#code)) into the prompt:
	<details>
        <summary>Expand: Copy and Paste Steps</summary>
        <ol>
            <li>
                <details>
                    <summary>Copy it</summary>
                    <ol>
                        <li>Select all the below code. 
                            It should look like this (color may differ)
                            <img src="Images/code1.png" alt="Code Selection">
                        </li>
                        <li>Hold the <code>Control</code> button</li>
                        <li>Press the <code>C</code> button</li>
                        <li>Release the <code>Control</code> button</li>
                    </ol>
                </details>
            </li>
            <li>
                <details>
                    <summary>Navigate to the folder you want the installation to be in</summary>
                    <ol>
                        <li>By default, this will install to the folder you see after the <code>PS</code>. So, a folder like <code>C:\Users\your_user_name</code>.</li>
                        <li>
                            <details>
                                <summary>You can change the folder by:</summary>
                                <ol>
                                    <li>Open file explorer
                                        <ol>
                                            <li>Either:
                                                <ol>
                                                    <li>Open this App: <img src="Images/fe.png" alt="File Explorer App"></li>
                                                </ol>
                                            </li>
                                            <li>OR
                                                <ol>
                                                    <li>Hold down the <code>Windows</code> button</li>
                                                    <li>Press the <code>E</code> button</li>
                                                    <li>Release the <code>Windows</code> button </li>
                                                </ol>
                                            </li>
                                        </ol>
                                    </li>
                                    <li>Navigate to the folder you'd like to install STELLA in</li>
                                    <li>
                                        <details>
			                                <summary>Click on the Navigation bar and copy the folder path</summary>
			                                <ol>
                                              <li>Click on this (below)</li>
                                             <img src="Images/navbar.png" alt="Notice">
			                                  <li>Press and Hold down the <code>Control</code> button</li>
			                                  <li>Press down the <code>C</code> button</li>
			                                  <li>Release the <code>Control</code> button</li>
			                                </ol>
                                      </details>
                                    </li> 
                                    <li> Switch back to the prompt window </li>
                                    <li> Input: 
                                            <code>cd the_path_to_your_folder</code>
                                        <p>(i.e) <code>cd C:/Users/Default/Downloads</code></p>
                                </ol>
                            </details>
                        </li>
                    </ol>
                </details>
            </li>
            <li>Switch to the prompt window</li>
            <li>
                <details>
			        <summary>Paste the Code</summary>
			        <ol>
			          <li>Press and Hold down the <code>Control</code> button</li>
			          <li>Press down the <code>V</code> button</li>
			          <li>Release the <code>Control</code> button</li>
			        </ol>
              </details>
            </li>
            <li>A notice should appear looking like the below:
            <img src="Images/notice.png" alt="Notice">
            </li>
            <li>Press <code>Paste anyway</code></li>
        </ol>
    </details>

3. Press the `Enter` button

4. Wait for the operation to complete, It should write about 70,000,000 - 90,000,000 bytes before exiting (should look similar ot the below image)

![I](Images/bar1.png)

(It'll be complete when the blue bar disappears, See below image)

![I](Images/jj1.png)

5.  Locate the zipfile in your chosen directory called `stella.zip`

6. 
    <details>
        <summary>Extract it to same folder</summary>
        <ol>
            <li>Select the file by clicking on it</li>
            <img src="Images/zip.png" alt="Zip file">
            <li>Click 'Extract All'</li>
            <img src="Images/ex.png" alt="Extract">
            <li>In the Wizard that opens, remove the <code>stella</code> from the folder path
            <img src="Images/ns.png" alt="No Stella">
            <li>Press the <code>Enter</code> button
        <ol>
    </details>

7. There should now be a bunch of miscellaneous files, including one named `cat.exe`, (see below image), this is the application and is what you should run. (See [This link](https://www.lifewire.com/how-to-add-shortcut-to-desktop-windows-10-4767486) on making shortcuts).

![Stellaexe](Images/stellaexe.png)
#### Now you're done! Stella is installed and you can use it by double clicking on `cat.exe`.

---
### Code
```
$url = "https://drive.usercontent.google.com/download?id=1qa_TaBkjumHaJJE1L-pe6FsDrkJO-S0o&export=download&authuser=0&confirm=t&uuid=8b8e4ca4-91b6-43f9-af9f-b75b1e6ebe56&at=APZUnTU0lIWJXrKJYDedOlY0D4U9%3A1719434822671"
$output = ".\stella.zip"

Invoke-WebRequest -Uri $url -OutFile $output
```
---

### Manual Download
1. Click [here](https://drive.google.com/file/d/1NCT7Woaee1r-MhdZWZtw594M0t22gkXY/view?usp=sharing)
2. Click `Download`
![DownloadImage](Images/dl.png) or 

![I](Images/d1.png)

3. Wait for it to download
4. <details>
    <summary>Go to your downloads</summary>
    <ol>
        <li>Open file explorer
            <ol>
                <li>Either:
                    <ol>
                        <li>Open this App: <img src="Images/fe.png" alt="File Explorer App"></li>
                    </ol>
                </li>
                OR
                    <ol>
                        <li>Hold down the <code>Windows</code> button</li>
                        <li>Press the <code>E</code> button</li>
                        <li>Release the <code>Windows</code> button</li>
                    </ol>
                </li>
            </ol>
        </li>
        <li>Click the Downloads folder button</li>
        <img src="Images/dlf.png" alt="downloads folder">
    </ol>
  </details>

5. <details>
        <summary>Extract it to the folder you want the installation in</summary>
        <ol>
            <li>Select the file by clicking on it</li>
            <img src="Images/zip.png" alt="Zip file">
            <li>Click 'Extract All'</li>
            <img src="Images/ex.png" alt="Extract">
            <li>In the Wizard that opens, change the path to where you want it to extract to</li>
            <img src="Images/cf.png" alt="Change folder image">
            <li>Press the <code>Enter</code> button
        <ol>
    </details>

6. Wait for it to extract

7. There should now be a bunch of miscellaneous files, including one named `cat.exe`, (see below image), this is the application and is what you should run. (See [This link](https://www.lifewire.com/how-to-add-shortcut-to-desktop-windows-10-4767486) on making shortcuts).

![Stellaexe](Images/stellaexe.png)
#### Now you're done! Stella is installed and you can use it by double clicking on `cat.exe`.

---

### Installation FAQ
#### Missing Files or Dependancies
- Turn off your antivirus 
- Redownload again (See [Installation Methods](https://github.com/trademark-claim/laughing-octo-garbanzo/blob/master/Installation.md#installation-methods))
- Ensure Zip extraction completes correctly
- Check all DLLs and ensure they match the ones listed in [Required Libraries](https://github.com/trademark-claim/laughing-octo-garbanzo/blob/master/Installation.md#required-libraries)
- If all else fails, download and compile from source from [here](https://google.com). (See .NET source compiling [here](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/app-development/building-a-wpf-application-wpf?view=netframeworkdesktop-4.8)).

#### How do I compile from source?
- Ensure you're invited to the repo
- Go [here](https://github.com/trademark-claim/Kitty)
- Press the `<Code>` button (image below)
![Code Button](Images/codebutton.png)
- Press the `DownloadZip` button (image below)
![Download Zip Button](Images/downloadzip.png)
- You now have the source code!
- See [here](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/app-development/building-a-wpf-application-wpf?view=netframeworkdesktop-4.8) for source compiling for .NET

#### Screensizing incorrect
_This shouldn't happen as STELLA is inherently designed to work on all DPIs (adjusting for any screen)__
- Try STELLA on another moniter to see if the problem persists
- Change DPI scaling for the affected screen(s) following this guide [here](https://winaero.com/how-to-change-dpi-display-scaling-in-windows-11/#:~:text=Change%20DPI%20in%20Windows%2011%20using%20Settings%201,will%20instantly%20apply%20the%20new%20DPI%20scaling%20value.).
- Download the latest version of STELLA

#### Shortcuts not responding
STELLA uses hooks, low level methods that interface with the functional layer of your computer. As such, there's a timer for these hooks that terminates them if they don't respond fast enough, causing the unresponsiveness. 
This will be especially true if you're putting your computer to sleep continuously.
You can try:
- Restart the program
- Restart computer
- Extend the hook timeout from registry (See [here](https://stackoverflow.com/questions/2655278/what-can-cause-windows-to-unhook-a-low-level-global-keyboard-hook) for a discussion on it)

#### .NET Runtime missing
- You can download the runtime [here](https://dotnet.microsoft.com/download/dotnet/8.0), make sure you get version `8.0`

#### How do I Update?
- Just delete the folder you downloaded and run STELLA from, download the latest version, and install it in the same location
- There is also an update module on the way, in which it'll be done from within the app -- stay tuned~

##### Note
STELLA Generates detailed log files, which can be found at `C:\ProgramData\Kitty\Cat\NYANPASU\Logs`.