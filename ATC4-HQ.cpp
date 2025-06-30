#include <windows.h>
#include <iostream>
#include <string>
int main() {
    SetConsoleOutputCP(CP_UTF8);   // 设置输出为UTF-8
    SetConsoleCP(CP_UTF8);         // 设置输入为UTF-8
    
    const char* pipeName = R"(LinkWithMainPipe)";
    HANDLE hPipe = CreateNamedPipeA(
        pipeName,
        PIPE_ACCESS_DUPLEX,
        PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT,
        1,              // 最大实例数
        256, 256,       // 输出/输入缓冲区大小
        0,              // 默认超时
        NULL            // 默认安全属性
    );

    if (hPipe == INVALID_HANDLE_VALUE) {
        std::cerr << "创建管道失败，错误码: " << GetLastError() << std::endl;
        return 1;
    }

    std::cout << "等待客户端连接..." << std::endl;
    BOOL connected = ConnectNamedPipe(hPipe, NULL) ? TRUE : (GetLastError() == ERROR_PIPE_CONNECTED);
    if (!connected) {
        std::cerr << "客户端连接失败。" << std::endl;
        CloseHandle(hPipe);
        return 1;
    }

    char buffer[256] = {0};
    DWORD bytesRead = 0;
    if (ReadFile(hPipe, buffer, sizeof(buffer), &bytesRead, NULL)) {
        std::string msg(buffer, bytesRead);
        std::cout << "收到消息: " << msg << std::endl;

        // 回复客户端
        std::string reply = "收到:" + msg;
        DWORD bytesWritten = 0;
        WriteFile(hPipe, reply.c_str(), (DWORD)reply.size(), &bytesWritten, NULL);
    } else {
        std::cerr << "读取失败。" << std::endl;
    }

    

    CloseHandle(hPipe);
    return 0;
}