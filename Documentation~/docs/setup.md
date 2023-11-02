# Installation and Configuration of project {#setup}

## Installation

You need to make sure you have git installed and accessible system wide.

### Using template project
The template repository in [https://github.com/ovi-lab/RoadmapAuthoring-template](https://github.com/ovi-lab/RoadmapAuthoring-template) is pre configured with the packages and settings required to use this package.

<div class="image">
    <img src="https://raw.githubusercontent.com/ovi-lab/RoadmapAuthoring/master/Documentation~/figures/docs/template.png" alt="template project on github">
    <div class="caption"> Template project on github. Click on the `Use this template` to create a repo from this template. You also have to the option to download this as a zip file.</div>
</div>


### Manual installation
**NOTE**: This is only necessary if the package is being installed manually without using the template proejct.

You would need the following package installed:
- Install as a git package: https://github.com/ovi-lab/arcore-unity-extensions
- MRTK3 (all MRTK 3 packages & the Mixed Reality OpenXR plugin) installed with the [Mixed Reality Feature Tool](https://www.microsoft.com/en-us/download/details.aspx?id=102778)
- Then install the `RoadmapAuthoring` as a git package

## Configuration of project

### Project settings
 TBD
 
### Setting Keystore
- Ensure the keystore is in `UserSettings/user.keystore`. 
- In `Player Settings` > `Player` > `Android tab` > ` Publishing Settings`
  - Ensure that `Path` under `Project Keystore` is set to the keystore path above.
  - Enter the password for the keystore.
  - Select the `Alias` under `Project Key` and also enter the password for the project alias.
  
<div class="image">
    <img src="https://raw.githubusercontent.com/ovi-lab/RoadmapAuthoring/master/Documentation~/figures/docs/keystore.png" alt="Keystore settings">
    <div class="caption"> Keystore settings</div>
</div>

Note that the passwords will not persist when you re-open the project, and would need to re-enter them again. For it to persist, on the Roadmap build setup (`Roadmap` > `Build and Run`), under keystore settings enter the passwords for the keystrore and key alias. Note that you may have to redo this step if you update the package.

<div class="image">
    <img src="https://raw.githubusercontent.com/ovi-lab/RoadmapAuthoring/master/Documentation~/figures/docs/roadmap_build_settings.png" alt="Roadmap build and run window">
    <div class="caption"> Roadmap build and run window</div>
</div>

### Updating the package
You can update the pacakge to the latest version from the package manager.

<div class="image">
    <img src="https://raw.githubusercontent.com/ovi-lab/RoadmapAuthoring/master/Documentation~/figures/docs/package_manager_update.png" alt="Pacakge manager update">
    <div class="caption"> Pacakge manager & update</div>
</div>

### Setting group ID
The group ID is erquired to be configired for the application to work. It can be set in either the Roadmap build settings window or the inspector of the application config.
