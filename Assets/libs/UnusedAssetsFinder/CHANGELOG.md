# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased] - YYYY-MM-DD
### Added

### Removed

### Fixed

### Changed

## [1.4.0] - 2022-07-01
### Fixed
- Deleting assets will no longer delete ALL empty folders in the project, now only empty folders are deleted if selected

## [1.3.1] - 2021-08-07
### Added
- Added more error checking when trying to delete a file
- Added more error checking when trying to calculate the file size of all found unused assets

## [1.3.0] - 2020-09-30
### Added
- Added a `package.json` so that the plugin can be treated as a package in the Package Manager
- Added `private` to methods and variables that don't explicitly define the scope
- Added new extensions to ignore by default
  - .xib - Xcode Interface Builder
  - .storyboard - iOS native splash screen

### Removed
- Removed the stopwatch that times how long a scan takes
- Removed some extension methods that were duplicates of some LINQ queries
- Removed the Import and Exporting of Rule Sets
- Removed the shortcut to remove empty folders

### Fixed
- Fixed some of the popup prompts that asked for an option having the buttons the wrong way around
- Fixed the stats of the search not showing in Unity 2019.1 and above

### Changed
- Changed all instances of "Unused Assets" to "Unused Assets Finder"
- Changed the project structure so all the code is in the editor folder
- Scanning for unused assets without providing a rule set now uses a default rule set that is created in memory and is not saved to the resources folder
- Any assets assigned in any ProjectSettings related tab are now found, but this needs the asset serialisation mode to be Force Text
- Split up `Strings.cs` so that it's more manageable
- Made the export of assets async which cleans up the code around that area and makes it faster
- The check for scenes that would be deleted is now done at the very start of a scan
- The check for special Unity folders that would possibly be found is now done at the start of the scan
- The version number is now a `Version` object and not a string

[1.2.1] - 2019-10-05
### Added
- Added more extensions to ignore by default
- .pri - Universal Windows Platform

### Removed
- Removed font file extensions from the default list of extensions to ignore as they are detected if they are being used or not

### Changed
- Made deletion of assets faster

## [1.2.0] - 2019-09-17
### Added
- Added even more extensions to ignore by default
  - .shadervarients
  - .compute - Compute shaders
  - .hlsl - Shaders
  - .tff - Font file
  - .ttf - Font file
  - .preset - Component presets
  - .asmdef - Assembly definitions
  - .spriteatlas - Sprite atlas
  - .modulemap - iOS Swift Framework
  - .jslib -  WebGL Javascript
  - .projmod - Xcode project file 
  - .rsp - C# compiler response file - Used to reference unreferenced DLL's
- Added even more places to look for assets, this includes project settings
- Added a Report Issue menu item (Tools → Unused Assets → Report Issue) that will open up a browser and direct you to the page to submit a new issue
- Even more comments on our code
- Watches for assets that have been moved from the project from outside of Unity
- Added Assembly Definitions for those who use them
- The plugin now removes its own depreciated files, so upgrading should be a lot more seamless
- Confirmed support for additive scenes e.g. Scenes loaded in scenes

### Removed
- Removed our custom define symbols, didn’t really work out like we wanted
- Removed scenes in packages from triggering the unused scenes found dialog

### Fixed
- Saved the loaded scene if it’s dirty before a scan, this ensures that all changes are saved so we know for sure what’s being used
- Limited the number of scenes listed in the unused scene dialogs, this stops the dialog becoming so tall the buttons are offscreen
- Fixed some cases after the export and deletion process the externally changed files watcher would display the dialog about files being deleted. The same thing applies to when empty folders are deleted
- Fixed edge case where only the root assets folder would be ticked and an error appear in the console when the select all button was pressed after the first scan after the project was opened or the plugin window opened
- Removed Tile Palettes used by the editor from the search, these assets are loaded indirectly so they always marked as unused, but in fact, causes issues if an existing tilemap needs to be edited

### Changed
- Organised the context menus, now things are a bit cleaner

## [1.1.2] - 2019-06-04
### Fixed
- Fixed errors in UnusedAssetsSettingsProvider.cs in 2018.1 and 2018.2.
- Fixed errors in UnusedAssets.cs caused by enabled define symbols for build modules/targets. Which block the editor scripts from compiling which would run and disable the define symbols, removing the errors. We solved the chicken and the egg problem but in Unity. Also added a menu button that should also fix the errors in case it doesn’t automatically run.
- Fixed .gitignore entry generation.

## [1.1.1] - 2019-06-03
### Fixed
- Fixed unprotected call to AssetDatabase in Strings.cs that caused errors on compiling.

## [1.1.0] - 2019-06-02
### Added
- Added new plugin preferences menu to allow the plugin editor window to be automatically docked next to a user-defined list of other editor windows if they are open.
- Added build module support to account for users not having specific build modules (e.g. Android or iOS modules) installed.
- Created an editor script that detects if a .gitignore file currently exists and if it does adds an ignore for the plugin directory and demo directory. If no existing .gitignore is found then a new one is created with the same outcome.
- Added in the plugin version number to the bottom bar of the plugin editor window.
- New extensions being ignored automatically:
  - .spriteatlas - Unity Sprite Atlas
  - .pom - Android Plugin File
  - .srcaar - Android Plugin File
  - .xml - AndroidManifest.xml
  - .json - Common file format used by plugins
  - .plist - iOS Plugin File
  
### Removed
- Tree Elements displayed in the tree view that get returned during a search query now appear in their parent and child hierarchy structure similar to when not returning search query results.
- Removed ability to skip backup exporting of files to a package before deleting - better to be safe than sorry.
- Removed the hard-coded resource path for the default ruleset.

### Fixed
- Tree Elements displayed in the tree view that get returned during a search query now appear in their parent and child hierarchy structure similar to when not returning search query results.
- Fixed multicolumn header sorting.
- Fixed some assets in the project settings not being ignored.

### Changed
- Refactored the tree view code to improve its functionality.
- Any empty folders left after the plugin cleans the project are now deleted.
- Changed default export file name from ‘Game Name Date’ to ‘Game Name Unused Assets Date’.
- Invalidated the search query if a file was deleted from the project outside of the plugin - either through the Unity Project window or through system file explorer
- Renamed editor namespace to UnusedAssets.Editor.
- Replaced ‘Things to Include’ and ‘Things to Exclude’ string lists with a single generic object list called ‘Exclude Specific Assets and Folders’ which supports, files, folders, and any other object inherited from UnityEngine.Object.
- Confirmed working with nested prefabs.

## [1.0.0] - 2018-04-12
### Added
- This is the first release! Hopefully, there aren’t any more fixing bugs.
