# Holo Desktop

Use AI to convert desktop content into holographic content for Looking Glass Portrait.

### Dependencies

`uDesktopDuplication`
`com.unity.barracuda`
`textmeshpro`

### Machine Learning

MiDaS 2 Small model is used for monocular depth perception.

### Creating a production build

-Clear the build folder.

-Build the Unity project

-Find the data folder, move it into an internal folder of the same name

- Run the inno compile script in Installer folder.

- The produced exe in the Installer folder is what is shared with consumers.

### Version Control

#### Pull Changes

`git pull`

#### Push Changes
`git add -A`

`git commit -m "This is my message"`

`git push origin master`
