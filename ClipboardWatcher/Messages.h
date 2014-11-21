#include "stdafx.h"

enum class MsgSeverity{
	Error = 0,
	Warning = 1,
	Info = 2,
	AppData = 3,
	Debug=4,
};

class Messages{
public:
	void WriteData(std::string data, std::string format, ...);
	void WriteData(std::string data);
	void WriteMsg(MsgSeverity msgType, std::string format, va_list args);
	void WriteInfo(std::string format, ...);
	void WriteError(const std::string errFunction, std::string format, ...);
	void WriteError(const std::string errFunction);
	void WriteDebug(std::string format, ...);
private:
	void WriteOut(const std::string msgType, const std::string msg);
};

