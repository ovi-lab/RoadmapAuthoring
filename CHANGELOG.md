# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.7] - 2023-11-22
### Fixed
- Fix for nested `UGUIInputAdapter`s masking one another (see https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/issues/561 for more info)
- Fix issue with new PlaceableObject needing to go in and out of modify state to be modifiable as soon as spawned.
  - Updating the modify flag at the end of the frame.

### [0.1.6] - 2023-11-21
### Fixed
- Nested XR interactable conflict with ObjectManipulator on PlaceableObject 
- Active config not being registered correctly in-editor

### Changed
- `PlaceableObject.onClickedCallback` use updated in `PlaceableManager`.
- Appropriate methods in `PlaceableObject` made internal.

### Added
- `PlaceableObject.onPlaceableInstantiated`
- `PlaceableObject.onClickedCallback`
- `PlaceableObjectEventArgs` and `PlaceableObjectEvent` to use with the above events.

## [0.1.5] - 2023-11-07
### Added
- Event that is called when Geospatial API is configured and setup

### Changed
- In AR scene, the hamburger menu is hidden until geospatial API gets configured.
- In AR scene, debug messages shown till geospatial API gets configured.
- RoadmaptSetup: try catch logs better messages on scene management

### Fixed
- RoadmaptSetup: handle error when scene setup is reset

## [0.1.4] - 2023-11-06
### Added
- Editor callback when the package version changes
  - Currently regenerates the build scenes

## Fixed
- Rotation of item in VirtualizedDynamicScrollRectList

### Changed
- RemoteDataSynchronizationEditor uses DisplayDialog.
- Updated visual of VR scroll list buttons.
- Updated messages and use of dialogs on all platforms.
- Updated the Build Setup UI to show the version.

## [0.1.3] - 2023-10-30
### Fixed
- Handle null in debug message
- Jar force resovle made override option in build setup

## [0.1.2] - 2023-10-30
### Added
- Option to reset the generated scenes

### Fixed
- VR_Scene template incorrectly configured
- Jar force resovle not called on correct pipeline

## [0.1.1] - 2023-10-29
### Added
- Documentation website (gh-pages)

### Changed
- Build uses scene generated from scene template
- Group ID can be set from build setup window

## [0.1.0] - 2023-10-24
### Added
- Working implementation of road map application
  - Allow synchronization with AR and VR applications
