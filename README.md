# ProductionPlan

## Welcome :)
Hi, I'm Adrian Vanhoeke, Junior .Net Developper at Satellit, and welcome to my Powerplant-coding-challenge solution.
You will find all my source code in this repo, and the installation and run processes explained here below.

## Local installation
If you want to install the solution on your local machine, you just need to clone this repository locally.

* Click on the `<> code` green button at the top right of this page
* Select the cloning method that suits you the best : download zip files or clone to Visual Studio

## Build & Run the api 
The api is containerized in a Windows Docker container. You can build and run it directly from this repo, or from your local copy.
### Based on your local copy
* Check that your Docker Desktop is running, and is set on _Windows containers mode_
  * If it is on _Linux containers mode_, right-click the System Tray's Docker client icon and select `switch to Windows containers`
* Open a _Windows Command Prompt_ window
* Change the directory to your local solution root folder : ` $\ENGIE.ProductionPlan-master\ `
* __Build the container running this command__ : `docker build -t productionplanapp \ProductionPlan.Api`
  * Note that _productionplanapp_ is the name of the image, you can modify it by any other name you prefer. Just keep in mind to use your custom name instead of _productionplanapp_ for further commands, and to use lower case only
* __Run the container__ :
  * in Development mode __with swagger interface__ : `docker run -it --rm -p 8888:80 -e "ASPNETCORE_ENVIRONMENT=Development" --name productionplancontainer productionplanapp`
  * in default mode __without swagger interface__ : `docker run -it --rm -p 8888:80 --name productionplancontainer productionplanapp`
### Based on this (cloud) github repo
* __Build the container running this command__ : `docker build -t productionplanapp https://github.com/AdriansGithb/ENGIE.ProductionPlan/tree/master/ProductionPlan.Api`
  * Note that _productionplanapp_ is the name of the image, you can modify it by any other name you prefer. Just keep in mind to use your custom name instead of _productionplanapp_ for further commands, and to use lower case only
* __Run the container__ :
  * in Development mode __with swagger interface__ : `docker run -it --rm -p 8888:80 -e "ASPNETCORE_ENVIRONMENT=Development" --name productionplancontainer productionplanapp`
  * in default mode __without swagger interface__ : `docker run -it --rm -p 8888:80 --name productionplancontainer productionplanapp`

## Test the api
