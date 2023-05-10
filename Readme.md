[![discord badge](https://img.shields.io/discord/716698782317805629?color=7289DA&label=discord)](https://discord.gg/uVtNr9Czng) [![Docs](https://img.shields.io/badge/-Documentary-blue)](https://docs3.synapsesl.xyz/)

# **Synapse**
## What is Synapse?
Synapse is an SCP:SL Plugin Framework and High-Level Modding API.

It was created for the purpose of making Plugin development for SCP:SL as easy as possible.

Synapse replaces a lot of logic of the base game with its own and allows plugin devs therefore to modify the game massively with ease and without compatibility issues with other Plugins

Previously it was almost impossible to create advanced logic like CustomRoles with multiple different plugins at the same time while somehow keeping it all stable and create a communication between each plugin to ensure no bugs appear, however with Synapse the plugins can just define the basic information of the custom role and Synapse will handle the entire logic for FriendlyFire, Round End Calculations and keeping track of players so that Plugins only have to focus on their actual special features of their Role and not the process of adding a new Role itself.

There are also a lot of other features like this, that tries to fix common Issues you encounter regularly while developing Plugins for SCP:SL.

## Why should I use Synapse as a Server Hoster?

We already established that there are a lot of features for developers, but what can Synapse offer to Server Hoster?

* Synapse offers a Translation System that allows individual Players to set their own Language with a simple Command
* Synapse's Database System is modular, so you can easily create an own DataBase Interface and reroute all the data storage to a custom created Database and all Plugins will use your Database
* Custom Permission System that is a lot friendlier than the vanilla one and it also allows inheritation of groups
* Useful Config for various default vanilla behaviors like a whitelist for Scp-173/096 (Tutorials can't trigger Scp-173/096 by default for moderation purposes)
* Graphical changes and new "Buttons" to the RemoteAdmin that makes the Remote Admin more useful
* The stability with many Plugins at the same time is much higher due to the fact that Synapse handles a lot of logic
* We always try our best to keep version compatibility so that even a year old plugin still runs completely fine on the newest Version and that Devs and Hosters don't have to update their server and plugins every week

## How can I install Synapse?
It depends on your machine running SCP: Sl but it's quite simple just follow the installation guide [here](https://docs3.synapsesl.xyz/hosting-guides/how-to-set-up-a-server)

## Developing a Plugin for Synapse
When you want to start coding Plugins for Synapse3 is it recommended to look at our beginner docs [here](https://docs3.synapsesl.xyz/getting-started/first-plugin) as well as joining our [Discord](https://discord.gg/uVtNr9Czng) for further questions you may have