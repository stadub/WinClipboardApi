// ClipboardWatcher.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

int _tmain(int argc, _TCHAR* argv[])
{
	Messages msg;
	if (argc != 2)
	{
		std::wstring appName(argv[0]);
		std::string asciiAppName(appName.begin(), appName.end());

		std::stringstream errText;
		errText << "Incorrect command line."
			<< "Expected " << asciiAppName << " [WindowClassName]";
		msg.WriteError(errText.str());


		return 1;
	}

	std::wstring windowClassName(argv[1]);
		
	MsgWindow window(&msg);

	window.InitWindowClass(windowClassName);
	window.CreateMessageWindow();
	window.RegisterClipboardListener();
	window.CreateWatchDogTimer();
	window.ChagngeMessageFilter();
	window.StartMainLoop();

	return 0;
}




