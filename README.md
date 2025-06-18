# Zipped Image Server

__Description__
This is a personal solutions to my issues of always having to remote into clients' machines and install new updates to docker based applications. There are probably better ways to solve this problem but this software works exactly how I want.

## What is this?
This server is for hosting zipped docker images for client software to download updates from. It has api key authentication to allow access to certain images. This goal is to include the ZippedImageApi package and use that to download and install images, using an external script to restart the container.

This isn't meant to be super popular and widely used so documentation will be limited.
