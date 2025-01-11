# Breeze source code (unity)

![Alt text](.Breeze_logo.png)

This repo contains the C# scripts used in my unity project "Breeze" which can be played at https://microcosmos22.itch.io/breeze.
The most important scripts are:

- UIManager: Handles everything, loads cameras, players, scenarios, instruments etc.
- PlaneControl/GliderControl: Player (plane) simulation Easy/Realistic mode.
- CloudFinder: Dynamic weather generation. Thermals, slope wind and more.

The inheritance structure is:

UIManager -> CloudFinder -> PlaneControl

This scripts are used in the "Assets" folder.

