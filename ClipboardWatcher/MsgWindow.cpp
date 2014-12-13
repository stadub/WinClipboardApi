
#include "stdafx.h"
#define WM_PING                         0x0420

std::map<HWND, MsgWindow*> MsgWindow::window_map = std::map<HWND, MsgWindow*>();

MsgWindow::MsgWindow(Messages* comunicator) :comunicator(comunicator){

}

LRESULT CALLBACK MsgWindow::WindProcGlobal(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam){
	auto iter = window_map.find(hWnd);
	if (iter != window_map.end()) return iter->second->WindProc(uMsg, wParam, lParam);
	else return ::DefWindowProc(hWnd, uMsg, wParam, lParam);
}
LRESULT MsgWindow::WindProc(UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	PCOPYDATASTRUCT pcData;
	LONG_PTR sender;
	DWORD sequenceNumber;
	switch (uMsg)
	{
	case WM_COPYDATA:
		pcData = (PCOPYDATASTRUCT)lParam;
		sender = (LONG_PTR)wParam;
		comunicator->PostData(MsgType_CopyData,"%p %d %p %d", sender, pcData->cbData, pcData->dwData, pcData->lpData);
		break;
	case WM_CLIPBOARDUPDATE:
		sequenceNumber=GetClipboardSequenceNumber();
		comunicator->PostData(MsgType_ClipboardUpdate, "%d", sequenceNumber);
		break;
	case WM_DESTROYCLIPBOARD:
		comunicator->PostData(MsgType_DestroyClipboard);
		break;
	case WM_RENDERFORMAT:
		comunicator->SendData(MsgType_RenderFormat, "%d", wParam);
		break;
	case WM_RENDERALLFORMATS:
		comunicator->SendData(MsgType_RenderAllFormats);
		break;

	case WM_CLOSE:
		comunicator->WriteDebug("Closing message window");
		DestroyWindow(messageWindow);
		return 0;
	case WM_TIMER:
		if (keepAlive<0)
			DestroyWindow(messageWindow);
		keepAlive --;
		break;
	case WM_PING:
		keepAlive=5;//wait 10 seconds before be killed by timer
		break;
	case WM_DESTROY:
		int resultCode = 0;
		if (keepAlive<0) 
			resultCode=WM_TIMER;
		PostQuitMessage(resultCode);
		break;
	}
	return DefWindowProc(messageWindow, uMsg, wParam, lParam);
}


void MsgWindow::InitWindowClass(const std::wstring& windClass){
	WNDCLASS windowClass = {};
	windowClass.lpfnWndProc = &MsgWindow::WindProcGlobal;

	windowClassName = windClass;

	windowClass.lpszClassName = windowClassName.c_str();

	if (!RegisterClass(&windowClass)) {
		comunicator->WriteError("RegisterClass");
	}
}

void MsgWindow::CreateMessageWindow()
{
	messageWindow = CreateWindow(windowClassName.c_str(), 0, 0, 0, 0, 0, 0, HWND_MESSAGE, 0, 0, 0);
	if (!messageWindow) {
		comunicator->WriteError("CreateWindow");
	}
	window_map[messageWindow] = this;
	comunicator->PostData(MsgType_WindowHandle, "%d", (int)messageWindow);
}

void MsgWindow::CreateWatchDogTimer()
{
	SetTimer(messageWindow, NULL, 2000, nullptr);
}

void MsgWindow::RegisterClipboardListener(){
	if (!AddClipboardFormatListener(messageWindow))
	{
		comunicator->WriteError("AddClipboardFormatListener");
	}
	comunicator->WriteDebug("RegisteredClipboardListener");
}

void MsgWindow::RemoveClipboardListener(){
	if (messageWindow == NULL)return;
	if (!RemoveClipboardFormatListener(messageWindow))
	{
		comunicator->WriteError("RemoveClipboardListener");
	}
	comunicator->WriteDebug("RemoveClipboardListener");
}

void MsgWindow::ChagngeMessageFilter(){
	CHANGEFILTERSTRUCT changefilterstruct;
	changefilterstruct.cbSize = sizeof(CHANGEFILTERSTRUCT);
#if (WINVER >= _WIN32_WINNT_WIN7)
	if (!ChangeWindowMessageFilterEx(messageWindow, WM_COPYDATA, MSGFLT_ALLOW, &changefilterstruct))
#else
#if (WINVER >= _WIN32_WINNT_VISTA)
	if (!ChangeWindowMessageFilter(WM_COPYDATA, MSGFLT_ADD))
#endif
#endif
	{
		comunicator->WriteError("ChangeWindowMessageFilterEx");;
	}
}

void MsgWindow::StartMainLoop(){
	MSG msg;

	while (GetMessage(&msg, 0, 0, 0) > 0) {
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

}

MsgWindow::~MsgWindow(){
	if (messageWindow != NULL)
		RemoveClipboardListener();
	if (!windowClassName.empty()){
		UnregisterClass(windowClassName.c_str(), NULL);
	}
}
