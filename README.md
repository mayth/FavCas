FavCas
======
**The Twitter Client for Fav**

1.0 beta
May Akizuru - @maytheplic

- - -

FavCas is a client specializing in fav.

If you find some bugs, please tell me from "Issues" page in the project page in github.com.

Requirements
------------
 - Windows Vista or later
 - .NET Framework 4 or later

Features
--------
 - FavCas has ability to fav and retweet ONLY
 - Keyboard binding is fully customizable
 - Mouse-over Fav

Notices to build
----------------
 - There are no "Properties/Resources.resx" and "Properties/Resources.Designer.cs".
   So please create a resource file from project's property.
   The resource must include 2 items: "ConsumerKey" and "ConsumerSecret".
 - If "Enable Just My Code (managed only)" is enabled,
   Visual Studio will show exception helper window for AuthenticationFailureException in authTask (MainWindow.xaml.cs).
   This exception will be catched in following code (try-catch block that includes authTask.Wait()).
   When VS shows the helper window, just ignore the exception and continue.
   (This application will be shutdown when AuthenticationFailureException is thrown.)
