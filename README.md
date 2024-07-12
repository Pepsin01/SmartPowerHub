# SmartPowerHub# SmartPowerHub

## Description
SmartPowerHub is a project that aims to provide a centralized hub for managing and controlling IoT devices in a home or office environment.
It's main goal is to reduce the financial operating costs of household by providing a way to plan IoT appliance consumption according to the production of energy from renewable sources.

## Features
- Control and monitor your IoT devices remotely
- Schedule power on/off for devices
- View projected energy production and consumption
- Modular design for easy integration of new devices through *.dll* files with IoT controller implementations

## Prerequisites

### Hardware
- Raspberry Pi or other device acting as a server
- IoT devices that can be controlled remotely

### Software
- Docker
- *.dll* files with IoT controllers that fulfill the IoTControllerContract interfaces

## Installation
1. Clone the repository:
```git clone https://github.com/Pepsin01/SmartPowerHub.git```
2. Navigate to the root directory of the solution and put all the *.dll* files with IoT controllers in the *SmartPowerHub/IoTControllers* directory.
3. We use the Docker software to build this app. You can download it from [here](https://www.docker.com/products/docker-desktop).
4. Before building a docker container, you need to have docker demon running.
5. In order to build this app, navigate to the root directory of this solution and run the following command:
```docker build -f SmartPowerHub/Dockerfile -t smart-power-hub .```
6. After the build is complete, you can run the container with the following command:
```docker run -d -p <HOST_PORT>:<CONTAINER_PORT> --name smart-power-hub-container smart-power-hub```
7. Insure that the ports specified in the previous command are available on your machine.
8. You can now access the app by navigating to `http://<YOUR_HOST_IP>:<HOST_PORT>` in your browser.

## Usage
1. Open the SmartPowerHub web application in your browser
3. Add your smart power devices to the hub
4. Configure, edit and monitor your devices from the Appliance Overview page and Energy Source Management page
5. Schedule power on/off for connected devices from the Consumption Planning page

## Testing
If you wish to test this application without having any of the IoT devices and respective controllers, you can use the mock IoT controller provided in the *ZigBeeControllerMockup* directory.
To build and test this application with the mock controller, follow these steps:
1. Build the mock controller by running the following command in the root directory of the solution:
```dotnet build ZigBeeControllerMockup/ZigBeeControllerMockup.csproj```
This will create a *ZigBeeControllerMockup.dll* file and copy it to the *SmartPowerHub/IoTControllers* directory.
If you don't have the dotnet command installed, follow steps on [this page](https://learn.microsoft.com/en-us/dotnet/core/install/).
2. As in the install part, insure that the docker is available and docker demon is running.
3. Navigate to the root directory of this solution and run the following command:
```docker build -f SmartPowerHub/Dockerfile -t smart-power-hub .```
4. Finally, run the container with the following command:
```docker run -d -p 8080:8080 --name smart-power-hub-container smart-power-hub```
5. Now you can access the app by navigating to `http://localhost:8080` in your browser.

## Contributing
Contributions are welcome! If you have any ideas or improvements, feel free to submit a pull request.

## License
This project was created as bachelor thesis at the Faculty of Mathematics and Physics, Charles University in Prague.
