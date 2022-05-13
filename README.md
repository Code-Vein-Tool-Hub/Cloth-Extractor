# Cloth Extractor
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/donate?hosted_button_id=7LVCJCM9LNQ2W)  
Unreal Cloth extractor for Code Vein

## Overview  
This tool is to provide aid for recreating clothing data for unreal modders mainly Code Vein.
Cloth is extracted in 2 files, a 3D model which holds the mesh driver and a json file which logs all the Cloth Config settings used in unreal.  

3D can be exported as Dae, Smd, or Obj, but obj will not contain any weights to cloth data just the mesh driver.  
When exported as Dae or Smd the model will include dummy bones and weights for those bones to be used with your original model, there will also be "MaxDistance", "BackstopDistances", and "BackstopRadiuses" weight values, these are the mask values for their respective mask in cloth painting divded by 100, so 1.0 weight is 100 value, 0.5 = 50, ect.

### Collision Info
Cloth data made with Apex clothing embeds Collision within the cloth data, to replicate this using unreal clothing you need to make a physics asset file and assign that in the cloth config. Embedded collision is main done in Sphere's and Sphere Connections which make a psudo cylinder using 2 connected spheres.  
The easiest way to recreate these is to add the 2 spheres using the dumped info. then create a cylinder in the middle of the 2 shperes by divding the largest value from the second sphere by 2. Set the Radius to match the dumped into and then set the length to the same number used to find the middle, lastly the cylinder will mostly need to be rotated line up, usually by 90 degrees in one of the axises. This should give a cylinder that lines up within the 2 spheres, finally delete the 2 original spheres.

## Useage
`Usage: ClothExtractor [Options] [File]`
-  -h, --help/: Prints help text and exits
-  -dae/-smd/-obj: Exports the driver mesh in the given format (Default dae)

# Credits
[QueenIO](https://github.com/VelouriasMoon/QueenIO)  
[UAssetAPI](https://github.com/atenfyr/UAssetAPI)  
[IONET](https://github.com/Ploaj/IONET)  

<p align="center">Made Using QueenIO:</p>
<p align="center">
    <img src="https://github.com/VelouriasMoon/QueenIO/blob/main/Images/LogoLight.png#gh-light-mode-only" width="150"/>
    <img src="https://github.com/VelouriasMoon/QueenIO/blob/main/Images/LogoDark.png#gh-dark-mode-only" width="150"/>
</p>
