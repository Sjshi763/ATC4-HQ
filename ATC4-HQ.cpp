#include <windows.h>
#include <iostream>
#include <string>
#include <sstream>
int main() {
    SetConsoleOutputCP(CP_UTF8);   // 设置输出为UTF-8
    SetConsoleCP(CP_UTF8);         // 设置输入为UTF-8
    
    const char* pipeName = "\\\\.\\pipe\\ATC4Pipe";
    HANDLE hPipe = CreateNamedPipeA(
        pipeName,
        PIPE_ACCESS_INBOUND, 
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
        // **简化后的命令行构建部分，直接使用 std::wstring 的 + 运算符**
        // 关键：将第一个字符串字面量显式转换为 std::wstring，
        // 这样后续的 + 运算符就能正常工作。
        std::wstring msg_wstr(msg.begin(), msg.end()); 

        // 确保 minizip.exe 的路径是正确的，建议使用绝对路径或相对路径
        std::wstring minizipExePath = L".\\minizip.exe"; // 假设在程序同目录
        std::wstring zipFilePath = L".\\ATC4ALL.zip";       // 假设在程序同目录

        // 如果目标路径 msg_wstr 包含空格，需要加上双引号
        std::wstring quotedTargetPath = msg_wstr;
        if (quotedTargetPath.find(L' ') != std::wstring::npos && quotedTargetPath.front() != L'\"' && quotedTargetPath.back() != L'\"') {
            quotedTargetPath = L"\"" + quotedTargetPath + L"\"";
        }
        std::wstring command = 
        std::wstring(minizipExePath) + L" " +   // 将第一个字面量转换为 std::wstring
        L"-x" + L" " +                          // 解压选项
        zipFilePath + L" " +                    // ZIP 文件路径 (这里原来可能少了空格，已添加)
        L"-d" + L" " +                          // 目标目录选项
        quotedTargetPath;    
        STARTUPINFOW si = { sizeof(si) };
        PROCESS_INFORMATION pi;
        CreateProcessW(
            NULL,                   // 应用程序名称
            &command[0],            // 命令行
            NULL,                   // 进程安全属性
            NULL,                   // 线程安全属性
            FALSE,                  // 是否继承句柄
            0,                      // 创建标志
            NULL,                   // 环境变量
            NULL,                   // 当前目录
            &si,                    // 启动信息
            &pi                     // 进程信息
        );
    } else {
        std::cerr << "读取失败。" << std::endl;
    }

   

    CloseHandle(hPipe);
    return 0;
}