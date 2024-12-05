![GitHub release (latest by date)](https://img.shields.io/github/v/release/infinitepower18/wsa-systemcontrol)
![GitHub all releases](https://img.shields.io/github/downloads/infinitepower18/WSA-SystemControl/total?label=github%20downloads)
[![.NET Build](https://github.com/infinitepower18/WSA-SystemControl/actions/workflows/dotnet.yml/badge.svg)](https://github.com/infinitepower18/WSA-SystemControl/actions/workflows/dotnet.yml)
![GitHub](https://img.shields.io/github/license/infinitepower18/wsa-systemcontrol)
# WSA System Control
A simple system tray application that allows you to monitor the WSA status as well as start/stop the subsystem. You can also launch the WSA Settings app, the Android Settings app and the Files app right from the menu.

The icon on the system tray changes depending on the WSA status.

<img width="340" alt="image" src="https://github.com/infinitepower18/WSA-SystemControl/assets/44692189/304fdbd8-ffd7-4127-96d2-23adf672724c">

**Stay updated on the latest WSA System Control updates via the [WhatsApp](https://whatsapp.com/channel/0029Va813rH1iUxXWN4sfl1Z) and [Telegram](https://t.me/WSASystemControl) channels.**

## Download
Operating System|Source
|---------|---------|
|<img src="https://upload.wikimedia.org/wikipedia/commons/e/e6/Windows_11_logo.svg" style="width: 150px;"/>|[<img src="https://get.microsoft.com/images/en-us%20dark.svg" style="width: 200px;"/>](https://apps.microsoft.com/store/detail/9PFCTFQ8V8C3?cid=ghreadme)|
|<img src="https://upload.wikimedia.org/wikipedia/commons/e/e6/Windows_11_logo.svg" style="width: 150px;"/></br><img src="https://upload.wikimedia.org/wikipedia/commons/0/05/Windows_10_Logo.svg" style="width: 150px;"/> |[<img src="https://user-images.githubusercontent.com/68516357/226141505-c93328f9-d6ae-4838-b080-85b073bfa1e0.png" style="width: 200px;"/>](https://github.com/infinitepower18/WSA-SystemControl/releases/latest)|
|<img src="https://upload.wikimedia.org/wikipedia/commons/e/e6/Windows_11_logo.svg" style="width: 150px;"/></br><img src="https://upload.wikimedia.org/wikipedia/commons/0/05/Windows_10_Logo.svg" style="width: 150px;"/> |[<img src="https://user-images.githubusercontent.com/49786146/159123331-729ae9f2-4cf9-439b-8515-16a4ef991089.png" style="width: 200px;"/>](https://winstall.app/apps/infinitepower18.WSASystemControl)|

Requires WSA running Android 13 or higher.

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/F1F1K06VY)

## Translations

Locale files are located in `WSA System Control/Resources`. The language that is used by WSA System Control is dependent on your Windows display language. If your language isn't yet supported by WSA System Control, it will fallback to English.

I am looking to integrate localization solutions such as Crowdin, but in the meantime you can make a PR with your contribution.

If you want to translate WSA System Control to a language which is not yet supported, use the `WSA System Control/Resources/Strings.resx` file as a base for the translation. Make sure that the file is named in the `Strings.langCode.resx` format e.g. `Strings.ja-JP.resx`.

Future updates to strings will involve updating all translated language files using Google Translate or some other machine translation service. As you may know, these services are not always accurate, so if you see any inaccuracies, please make a pull request!

During translation, it is a good idea to run WSA System Control with the translated language to see the end result. As long as the translated file is properly named and placed in the `WSA System Control/Resources` folder, it should automatically display in that language.

Text enclosed in `{}` brackets are variables which should be left the way they are and not be translated.

### Microsoft Store page
Along with the program itself, I also aim to provide localized MS Store pages in the languages which the program is available. In your PR, include a comment with translated versions of the strings provided on [this page](https://gist.github.com/infinitepower18/cfa1df87d6b5c1c1d520c892303a8d79).

## Disclaimer
This project is not affiliated with Microsoft or Google in any way.
