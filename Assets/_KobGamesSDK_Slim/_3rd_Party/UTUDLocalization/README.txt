This is a simple localization tool for Tracking Usage Description field in XCode.
It based mostly on Apploving MAX's realization, but you can add any language localization.

You can add your own custom localization to UTUDLocalizationSettings.asset. Please, DON'T change file name!
UTUDLocalizationSettings.asset already contains localization for 19 languages (listed at the end).

To use it you need:
1) Enable Apploving Consent Flow (AppLoving -> Integration manager -> Enable Consent Flow)

2) DISABLE Apploving Localization (AppLoving -> Integration manager -> (TURN OFF) Localize User Tracking Usage Discription
NOTE: If you don't disable AppLoving localization it will overwrite our custom localization for matching languages 
(e.x. for "fr" and "de"). This is happening because AppLoving postprocess is always the last.

3) In UTUDLocalizationSettings check Use Custom Localization option.


To add not existing language you need to know it's LANGUAGE CODE for XCode localization.
You can read about it here at "Understand the Language Identifier":
https://developer.apple.com/documentation/xcode/localization/choosing_localization_regions_and_scripts
ISO 639-1 for languages codes:
https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes
ISO 3166-1 for countries codes
https://en.wikipedia.org/wiki/ISO_3166-1
ISO 15924 for script codes:
https://en.wikipedia.org/wiki/ISO_15924

After getting Language Code add it to "Code" field and translation to "LocalizedText"
Save it.

On build process postprocess will add localization to XCode project.
You can check if everything alright by navigating at XCode project to Unity-iPhone -> Resources -> YOUR_LANGUAGE_CODE.lproj

_________________

UTUDLocalizationSettings contains localization for:
1)  ru 		- Russian
2)  zh-Hans-tw	- Taiwan Chinese in simplified script
3)  zh-Hans 	- Chinese in the simplified script
4)  fr 		- France
5)  de 		- German
6)  hi 		- Hindi for India
7)  id 		- Indonesian
8)  it 		- Italian
9)  ja 		- Japanese
10) ko 		- Korean
11) ms		- Malaysian
12) pl		- Polish
13) pt-br	- Portuguese for Brasil
14) pt		- Portuguese for EU
15) es		- Spanish for EU
16) es-419	- Spanish for Latin America
17) th 		- Thai
18) tr		- Turkish
19) vi		- Vietnamese
