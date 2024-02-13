Locale files are located in `WSA System Control/Resources`. The language that is used by WSA System Control is dependent on your Windows display language. If your language isn't yet supported by WSA System Control, it will fallback to English.

I am looking to integrate localization solutions such as Crowdin, but in the meantime you can make a PR with your contribution.

If you want to translate WSA System Control to a language which is not yet supported, use the `WSA System Control/Resources/Strings.resx` file as a base for the translation. Make sure that the file is named in the `Strings.langCode.resx` format e.g. `Strings.ja-JP.resx`.

Future updates to strings will involve updating all translated language files using Google Translate or some other machine translation service. As you may know, these services are not always accurate, so if you see any inaccuracies, please make a pull request!

During translation, it is a good idea to run WSA System Control with the translated language to see the end result. As long as the translated file is properly named and placed in the `WSA System Control/Resources` folder, it should automatically display in that language.

Text enclosed in `{}` brackets are variables which should be left the way they are and not be translated.

## Microsoft Store page
Along with the program itself, I also aim to provide localized MS Store pages in the languages which the program is available. In your PR, include a comment with translated versions of the strings provided on [this page](https://gist.github.com/infinitepower18/cfa1df87d6b5c1c1d520c892303a8d79).
