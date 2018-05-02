# BDE
BDE enables the developers to easily select a right location of each beacon, determine the coordinates of the location and install the beacon at the coordinates would be prohibitive without the tools. BDE make use of the data provided by the Building/environment Data and Information (BeDI) repository of the building to support the design, configuration, installation and maintenance of the IPS. A part of BDE is a plugin of the widely used BIM software Autodesk Revit.

## Usage of BDE Revit AddIn

To use the plugin on your own machine with Revit installed, you are going to need to include an AddIn file in your Revit AddIns folder and also change a line in this Addin File to point to the right DLL files. All of these files can be found in the ExtractBeacons folder in this repository.

1. Copy the BDE.addin file from the project folder  (should be something like "PROJECT_ROOT_FOLDER\BDE_Revit_Addin\BDE.addin") into the Revit Addins Folder on your machine (should be something like "C:\Users\{USERNAME}\AppData\Roaming\Autodesk\Revit\Addins\2017")
2. Change the <Assembly> file path in BDE.addin to point to the location of the BDE_Revit_Addin.dll file in the Debug (should look like <Assembly>C:\..\PROJECT_ROOT_FOLDER\BDE_Revit_Addin\BDE_Revit_Addin\bin\Debug\BDE_Revit_Addin.dll</Assembly>)
3. Startup Revit.exe of external program by click "Project" -> "BDE_Revit_Addin attribute" _> "Debug" -> "Enable External Program" -> setup the path such like  "C:\Program Files\Autodesk\Revit 2017\Revit.exe". 

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Added some feature'`
4. Push to branch: `git push origin my-new-feature`
5. Submit a pull request :D


## Troubleshooting

* If Revit got a External Tool Failure - System.IO.FileLoadException
[Please follow this blog's instruction](http://thebuildingcoder.typepad.com/blog/2011/10/revit-add-in-file-load-exception.html)
