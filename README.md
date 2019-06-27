# K2Documentation.Samples.SmartObjects.Authoring
Sample code that demonstrates how to programmatically author and deploy SmartObjects using code.

This project contains sample code that demonstrates how to programmatically work with SmartObjects: 
* Author a SmartObject
* Deploy a SmartObject
* Create Associations between SmartObjects

## Prerequisites
The sample code has the following dependencies: 
* .NET Assemblies (assemblies are included with K2 client-side tools install and are also included in the project's References folder)
  * SourceCode.Framework.dll 
  * SourceCode.HostClientAPI.dll
  * SourceCode.SmartObjects.Authoring.dll
  * SourceCode.SmartObjects.Client.dll
  * SourceCode.SmartObjects.Management.dll

## Getting started
* Use these code snippets to learn how to perform common SmartObject tasks. In this project, three SmartObjects are created programmatically and then published. Once the SmartObjects are published, associations are created between them. 
* Note that these projects may compile, but will not actually run as-is, since they are intended as sample code only. You will need to edit the code snippets to work in your environment and with your artifacts. 
* Fetch or Pull the appropriate branch of this project for your version of K2. 
* The Master branch is considered the latest, up-to-date version of the sample project. Older versions will be branched. For example, there may be a branch called K2-Five-5.0 that is specific to K2 Five version 5.0. There may be another branch called K2-Five-5.3 that is specific to K2 Five version 5.3. Assume that the master branch is configured for the latest release version of K2 Five. 
* The Visual Studio project contains a folder called "References" where you can find the referenced .NET assemblies or other uncommon assemblies. By default, try to reference these assemblies from your own environment for consistency, but we provide the referenced assemblies as a convenience in case you are not able to locate or use the referenced assemblies in your environment. 
* The Visual Studio project contains a folder called "Resources". This folder contains additional resources that may be required to use the same code, such as K2 deployment packages, sample files, SQL scripts and so on. 
   
## License
This project is licensed under the MIT license, which can be found in LICENSE.

## Notes
 * The sample code is provided as-is without warranty.
 * These sample code projects are not supported by K2 product support. 
 * The sample code is not necessarily comprehensive for all operations, features or functionality. 
 * We only accept code contributions that are compatible with the MIT license (essentially, MIT and Public Domain).
