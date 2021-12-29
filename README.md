# Doombox
Disclaimer: This project was made for fun and for familiarizing myself with ansible and nginx, I do not consider this to be a recommended solution.

This project offers a way to introduce artificial network delays in service communication with the use of reverse proxies.

## Prerequisites
- This project needs an Azure subscription to run the ansible scripts against.
- You need to have pre-configured an Azure Container Registry. This registry needs to contain the Docker image that you can build using the proxy folder.
- You need to have an Azure file storage. This file storage will be used to access the default and location files, found in the proxy folder.
  This was done to allow publishing the Docker image while allowing custom default and location templates (i.e. different ports, protocols etc),
  however removing the steps requiring the file storage and instead just copying the template files during the Dockerfile execution should suffice.

## Getting started

- Build the project
- Bootstrap the project using `./Doombox bootstrap --serviceconfigpath ./ansible/vars.yml`
- Teardown the project using `./Doombox teardown --serviceconfigpath ./ansible/vars.yml`
