# clip-ping
Displays a visual notification in the active window when the clipboard is updated.

Launching the app adds an icon in the tray. Right-click -> Exit to exit.

The app is written with .NET 9 and requires at least Windows 10 version 1607.

QoL features are not implemented yet (no way to customize the overlay, no way to launch automatically the app at startup). There are two overlays implemented, the default one is a halo on top of the window. 

![image](https://github.com/user-attachments/assets/56f44df4-d972-481d-ba22-bf8e1b62b7e9)

The other overlay is a thin border around the window.

![image](https://github.com/user-attachments/assets/ca98089e-72f4-4f8c-8d73-5c16ac7cb846)

Though it's mostly used to debug the overlay area, you can enable the border overlay by editing the code.
In `App.xaml.cs`, replace `new TopOverlay` with `new BorderOverlay`.

