# Unity3D-DDP-Client

[A lightweight DDP client for Unity3D](https://github.com/green-coder/unity3d-ddp-client). In other words, a library for Unity3D to communicate in realtime with [Meteor](https://www.meteor.com) servers.

I can be used for many things, like account registration, score board, match-making, chat, turn-based games, cooperative tools, etc ...

It may not be a fit for game interactions which require a lot of network data update per second (in which case you may want to use something like UNet instead).

> If you are developing for the UWP platform (e.g. for Mixed Reality headsets or XBox) or if you like to use the new .Net 4.6 take a look at the [dev branch](https://github.com/green-coder/unity3d-ddp-client/tree/dev)

## Feature list

* DDP-client:
  * Implementation of the [DDP-client protocol](https://github.com/meteor/meteor/blob/master/packages/ddp/DDP.md) version 1.
  * data subscription / notification.
  * method call / response.
  * Uses websocket protocoles `ws://` and `wss://` to communicate with the server.
  * Does not make assumptions about how to reconnect in case of disconnection. Let the user do it. Example provided.
* Accounts:
  * Login with username and password, logout.
  * Resume previous session by login using a token.
  * Does not make assumption about how the credentials and loaded and saved, let the user do it instead.
* Local Database:
  * Is optional.
  * Listens to the `DdpConnection` and creates collections on demand.
  * Let the user instantiate the database's collection via a callback.
  * Database collections can keep the documents as JSON objects or as custom classes, in which case it let the user provide serialization / deserialization callbacks.
  * Collections can be iterated over, and the user can register to their content's changes.

## Getting started

### Start the Meteor server

* Install [Meteor](https://www.meteor.com/install).

```
$  curl https://install.meteor.com/ | sh
```

* Launch the server:

```
$ cd example-server
$ meteor
```

### Start one of the Unity3D examples

Read the source code to see how it works and which keys to press.

The code is fairly simple and easy to read, both in the examples and in the library classes.

## Stability

The source code is freshly typed and was not fully tested yet. If you find a bug, please file an issue on Github and I will be happy to fix it asap.

The source code is fairly simple, so may you find a bug, a fix should not take very long to implement.

## Roadmap

Features to add later:

* Additional ways to login, maybe using Facebook and Google+.
* A bigger and complete example project.

## Contribute

Contributions are welcome on this project.

Pull requests will be reviewed and may be modified to fit the coding style (i.e. to keep it easy to read).

## License

This project is distributed under the MIT license.
