# SAR Unity Common Functionality

The source scripts and prefabs for the common unity package.

## Building a DLL

1.  Create a new Class Library:

    >    1. Visual Studio
    >    2. New Project...  
    >    3. New Class Library
    >       - .NET Framework 3.5


1.  Add Reference to Unity library
    
    **[Windows]**<br />

    > 1. Solution Explorer  <br />
    > 2. Add [RC] <br />
    > 3. Reference...   <br />
    > 4. Browse <br />
    > 5. Browse...<br />
    > 6. "C:\Program Files\Unity\Editor\Data\Managed\UnityEngine.dll" <br />


    **[Mac]**<br />
```
    TBA
```

1.  Add git repository to source folder and refresh project

    Solution Explorer: **Show All Files**  &  **Refresh**

1.  Add new files to project

    Solution Explorer: Right Click item & **Add to Project**

1.  Build DLL

    > `Build menu` <br />
    > `Build MyClassLibrary`


## EDITING CODE:

See Drew or Daniel first!

1.  Create a Plugins folder within Assets
2.  Run the folowing command from the project's top level

    > `git submodule add git@wcl.ml.unisa.edu.au:unity-sar/common.git Assets/Plugins/common`