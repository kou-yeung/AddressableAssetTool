# AddressableAssetTool

Easy for Setup Asset Address & Build.

# Setup

## Add Package

install Addressables:

[Window] > [Package Manager] > [Addressables] > [1.19.19] > [Install]

[Download](https://github.com/kou-yeung/AddressableAssetTool/releases) and import AddressableAssetTool.unitypackage

## Setup

1.[AddressableAssetTool] > [Setup]

2.Select [Assets\AddressableAssetsData\AssetGroups\Remote] and change Build & Load Paths To [Remote]

 L todo : このステップの自動化

## How to use

1.Select "AddressableAssetTool"

2.Add "Build Rule"

・Drag&Drop a folder to "path"

・Set "assetType" : Local(include in app) or Remote(Load from Remote)

・label : set the label group

・recursive : recursive file from folder

・extensions : set file extension to pack.(Separately ';') e.g. ＊.png;＊.prefab

3.Build!!

4.[Use it!!](https://gist.github.com/kou-yeung/cee45275121f4b515510de486ea1c67a)

# System requirements

Unity 2021.3.1f1

Addressables Version.1.19.19
