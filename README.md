# AddressableAssetTool (AAS)

Easy for Setup Asset Address & Build.

# Setup

## Add Package

install Addressables:

[Window] > [Package Manager] > [Addressables] > [1.7.5] > [Install]

import unitypackage.(you can download from releases)

https://github.com/kou-yeung/AddressableAssetTool/releases

## Setup

[AAS] > [Setup]

this step auto setup

1.Create "AddressableAssetSettings"

2.Create "Remote" group for remote load

3.Set RemoteLoadPath to "BASE_URL/[BuildTarget]"

4.Create "AddressableAssetTool"

## How to use

1. Select "AddressableAssetTool"

2. Add "Build Rule"

・Drag&Drop a folder to "path"

・Set "assetType" : Include(include in app) Preload(Load for Remote)

・recursive : recursive file from folder

・extensions : set file extension to pack.(Separately ';') e.g. ＊.png;＊.prefab

3.Build!!

# 使用バージョン

Addressables Version.1.7.5
