#include "stdafx.h"

enum class MsgSeverity{
	Error = 0,
	Warning = 1,
	Info = 2,
	SendData = 3,
	PostData = 4,
	Debug=5,
};

class Messages{
public:
	void SendData(std::string data, std::string format, ...);
	void SendData(std::string data);
	void PostData(std::string data, std::string format, ...);
	void PostData(std::string data);
	void WriteMsg(MsgSeverity msgType, std::string format, va_list args);
	void WriteInfo(std::string format, ...);
	void WriteError(const std::string errFunction, std::string format, ...);
	void WriteError(const std::string errFunction);
	void WriteDebug(std::string format, ...);
private:
	void WriteOut(const std::string msgType, const std::string msg);
	void ReadData(const std::string data);
};

