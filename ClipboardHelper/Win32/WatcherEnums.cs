

//TEnums shoud be same as in the ClipbordWatcherTypes.h file
namespace ClipboardHelper.Win32.ClipbordWatcherTypes
{
    enum MsgSeverity{
	Error = 0,
	Warning = 1,
	Info = 2,
	AppData = 3,
	Debug=4,
}
	public enum MsgType{
	WindowHandle=0,
	CopyData=1,
	ClipboardUpdate=2,
	DestroyClipboard=3,
	RenderFormat=4,
}

}

