# ProductionPlan

## Welcome :)
Hi, I'm Adrian Vanhoeke, Junior .Net Developper at Satellit, and welcome to my Powerplant-coding-challenge solution.
You will find all my source code in this repo, and the installation and run processes explained here below.

## Cloud or Local cloned source code
You can build and run this API directly from this git repo, but you also have the possibility to clone the solution on your local machine. In this case, you just need to clone this repository locally.

* Click on the `<> code` green button at the top right of this page
* Select the cloning method that suits you the best : _download zip files_ or _clone to Visual Studio_

## Build & Run the API 
The API is containerized in a Windows Docker container. You can build and run it directly from this repo, or from your local copy.
### Based on your local copy
* Verify that your Docker Desktop is running, and is set on _Windows containers mode_
  * If it is on _Linux containers mode_, right-click the System Tray's Docker client icon and select `switch to Windows containers`
* Open a _Windows Command Prompt_ window
* Change the directory to your local solution root folder : ` $\ENGIE.ProductionPlan-master\ `
* __Build the container running this command__ : `docker build -t productionplanapp .`
  > Note that _productionplanapp_ is the name of the image, you can modify it by any other name you prefer. Just keep in mind to use your custom name instead of _productionplanapp_ for further commands, and to use lower case only
* __Run the container__ :
  * __with swagger interface__ _(Development mode)_ : `docker run --rm -dp 8888:80 -e "ASPNETCORE_ENVIRONMENT=Development" --name productionplancontainer productionplanapp`
  * __without swagger interface__ : `docker run --rm -dp 8888:80 --name productionplancontainer productionplanapp`
### Based on this (cloud) github repo
* __Build the container running this command__ : `docker build -t productionplanapp https://github.com/AdriansGithb/ENGIE.ProductionPlan.git`
  > Note that _productionplanapp_ is the name of the image, you can modify it by any other name you prefer. Just keep in mind to use your custom name instead of _productionplanapp_ for further commands, and to use lower case only
* __Run the container__ :
  * __with swagger interface__ _(Development mode)_ : `docker run --rm -dp 8888:80 -e "ASPNETCORE_ENVIRONMENT=Development" --name productionplancontainer productionplanapp`
    > Note that _productionplancontainer_ is the name of the container, you can modify it by any other name you prefer. Just keep in mind to use lower case only
  * __without swagger interface__ : `docker run --rm -dp 8888:80 --name productionplancontainer productionplanapp`
    > Note that _productionplancontainer_ is the name of the container, you can modify it by any other name you prefer. Just keep in mind to use lower case only

## Test the API
Once the +API is running, you can test it :
* __with swagger interface__ _(if enabled like explained above)_ :
  * open your browser and navigate to `http://localhost:8888/swagger/index.html`
  * try to POST any payload object you want
* __without swagger interface__ : 
  * open _Postman_ (or any other API testing app)
  * try to POST any payload object you want to this address `http://localhost:8888/api/ProductionPlan`
