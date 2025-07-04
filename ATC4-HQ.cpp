#include <windows.h>
#include <iostream>
#include <string>
#include <fstream> // 用于文件日志重定向
#include <vector>  // 包含vector，防止未来扩展报错
#include <cstdlib> // 用于 atexit 函数
#include <filesystem> // 用于路径操作 (C++17)

// 全局日志文件流，确保它在所有函数中都可访问和管理
static std::ofstream logFile;

// 用于将控制台输出重定向到文件的函数
void RedirectConsoleOutputToFile(const std::string& filename) {
    // 如果日志文件已打开，先关闭它再重新打开
    if (logFile.is_open()) {
        logFile.close();
    }
    // 以追加模式打开日志文件。如果文件不存在，则创建它。
    logFile.open(filename, std::ios_base::app);

    // 检查文件是否成功打开
    if (logFile.is_open()) {
        // 将标准输出 (cout) 和标准错误 (cerr) 重定向到文件缓冲区
        std::cout.rdbuf(logFile.rdbuf());
        std::cerr.rdbuf(logFile.rdbuf());
        // 打印确认消息到日志文件并立即刷新
        std::cout << "--- 控制台输出已重定向到 " << filename << " ---" << std::endl;
        std::cout.flush(); // 确保此消息立即写入文件
    } else {
        // 如果文件无法打开，则向原始 cerr (控制台) 打印错误
        // 如果重定向失败，此消息可能不会被记录
        std::cerr << "错误：无法打开日志文件 " << filename << std::endl;
    }
}

// 程序退出时自动调用的函数
// 这确保了任何缓冲的输出都被写入文件，并且文件被关闭
void CloseLogFileOnExit() {
    if (logFile.is_open()) {
        logFile.flush(); // 将任何剩余的缓冲输出刷新到文件
        logFile.close(); // 关闭日志文件
    }
}

int main() {
    // 注册 CloseLogFileOnExit 函数，以便在程序终止时调用
    // 这对于确保所有日志都被写入至关重要，即使程序意外退出。
    atexit(CloseLogFileOnExit);

    // 获取当前可执行文件的完整路径
    wchar_t path[MAX_PATH];
    GetModuleFileNameW(NULL, path, MAX_PATH);
    // 使用 C++17 filesystem 库来获取可执行文件所在的目录
    std::filesystem::path exePath(path);
    std::filesystem::path exeDirectory = exePath.parent_path();
    
    // 构建日志文件的完整路径
    std::string logFileName = "pipe_server_log.txt";
    std::filesystem::path logFilePath = exeDirectory / logFileName;
    
    // 将宽字符路径转换为多字节路径，以便传递给 RedirectConsoleOutputToFile
    // 注意：这里假设日志文件名是ASCII兼容的，如果包含非ASCII字符，需要更复杂的转换
    std::string logFilePathStr = logFilePath.string();

    // 将控制台输出重定向到指定的日志文件
    RedirectConsoleOutputToFile(logFilePathStr);

    // 在每次程序启动时，如果日志文件非空，则添加一个空行，以便区分不同的运行日志
    // logFile.tellp() 返回当前写入位置，如果大于0，表示文件已有内容
    if (logFile.is_open() && logFile.tellp() > 0) {
        logFile << std::endl;
    }

    // 设置控制台代码页以支持 UTF-8
    SetConsoleOutputCP(CP_UTF8);
    SetConsoleCP(CP_UTF8);

    std::cout << "--- 程序启动 ---" << std::endl; // 记录程序启动
    std::cout << "日志文件路径: " << logFilePathStr << std::endl; // 记录日志文件实际路径

    const char* pipeName = "\\\\.\\pipe\\ATC4Pipe";
    std::cout << "尝试创建命名管道: " << pipeName << std::endl;

    // 创建命名管道
    HANDLE hPipe = CreateNamedPipeA(
        pipeName,
        PIPE_ACCESS_INBOUND, // 管道方向：入站 (客户端写入，服务器读取)
        PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT, // 字节类型管道，字节读取模式，阻塞模式
        1, // 最大实例数 (只有一个服务器实例)
        256, 256, // 输出缓冲区大小，输入缓冲区大小
        0, // 默认超时
        NULL // 默认安全属性
    );

    // 检查管道创建是否失败
    if (hPipe == INVALID_HANDLE_VALUE) {
        std::cerr << "错误：创建管道失败，错误码: " << GetLastError() << std::endl;
        return 1; // 返回错误码退出
    }
    std::cout << "管道创建成功。句柄: " << hPipe << std::endl;

    std::cout << "等待客户端连接..." << std::endl;
    // 等待客户端连接
    BOOL connected = ConnectNamedPipe(hPipe, NULL) ? TRUE : (GetLastError() == ERROR_PIPE_CONNECTED);
    // 检查客户端连接是否失败
    if (!connected) {
        std::cerr << "错误：客户端连接失败，错误码: " << GetLastError() << std::endl;
        CloseHandle(hPipe); // 关闭管道句柄
        return 1; // 返回错误码退出
    }
    std::cout << "客户端已连接！" << std::endl;

    char buffer[256] = {0}; // 用于存储从管道读取的数据的缓冲区
    DWORD bytesRead = 0;   // 实际读取的字节数

    std::cout << "尝试从管道读取数据..." << std::endl;
    // 从管道读取数据
    if (ReadFile(hPipe, buffer, sizeof(buffer), &bytesRead, NULL)) {
        std::string msg(buffer, bytesRead); // 将缓冲区转换为字符串
        std::cout << "成功从管道读取到 " << bytesRead << " 字节。" << std::endl;
        std::cout << "收到消息: \"" << msg << "\"" << std::endl;

        // 将收到的消息转换为宽字符串 (用于 CreateProcessW)
        std::wstring msg_wstr(msg.begin(), msg.end());
        std::cout << "消息转换为宽字符串成功。" << std::endl;

        // 定义 minizip.exe 和 zip 文件的路径
        // 使用程序所在目录构建绝对路径，避免找不到文件
        std::filesystem::path minizipExePath = exeDirectory / L"minizip.exe";
        std::filesystem::path zipFilePath = exeDirectory / L"ATC4ALL.zip";

        // 将宽字符路径转换为多字节路径以便日志输出
        std::cout << "minizip.exe 路径设置为: " << minizipExePath.string() << std::endl;
        std::cout << "ZIP 文件路径设置为: " << zipFilePath.string() << std::endl;

        // --- 新增：检查 minizip.exe 是否存在 ---
        if (!std::filesystem::exists(minizipExePath)) {
            std::cerr << "错误：minizip.exe 不存在于路径: " << minizipExePath.string() << std::endl;
            CloseHandle(hPipe);
            return 1;
        }
        // --- 新增：检查 ATC4ALL.zip 是否存在 ---
        if (!std::filesystem::exists(zipFilePath)) {
            std::cerr << "错误：ATC4ALL.zip 不存在于路径: " << zipFilePath.string() << std::endl;
            CloseHandle(hPipe);
            return 1;
        }

        // 处理目标路径中的空格，如果需要则添加引号
        std::wstring quotedTargetPath = msg_wstr;
        if (quotedTargetPath.find(L' ') != std::wstring::npos && quotedTargetPath.front() != L'\"' && quotedTargetPath.back() != L'\"') {
            quotedTargetPath = L"\"" + quotedTargetPath + L"\"";
            std::cout << "目标路径包含空格，已添加引号。引用路径: \"" << std::string(quotedTargetPath.begin(), quotedTargetPath.end()) << "\"" << std::endl;
        } else {
            std::cout << "目标路径不包含空格或已加引号。路径: \"" << std::string(quotedTargetPath.begin(), quotedTargetPath.end()) << "\"" << std::endl;
        }

        // --- 新增：检查并创建目标目录 ---
        std::filesystem::path targetDirPath(quotedTargetPath.begin(), quotedTargetPath.end()); // 将宽字符串转换为路径对象
        // 移除路径末尾可能存在的引号，以便正确判断目录
        if (targetDirPath.string().front() == '\"' && targetDirPath.string().back() == '\"') {
            targetDirPath = targetDirPath.string().substr(1, targetDirPath.string().length() - 2);
        }
        
        if (!std::filesystem::exists(targetDirPath)) {
            std::cout << "目标目录不存在，尝试创建: " << targetDirPath.string() << std::endl;
            std::error_code ec; // 用于捕获错误
            if (std::filesystem::create_directories(targetDirPath, ec)) {
                std::cout << "目标目录创建成功。" << std::endl;
            } else {
                std::cerr << "错误：无法创建目标目录 " << targetDirPath.string() << "，错误信息: " << ec.message() << std::endl;
                CloseHandle(hPipe);
                return 1;
            }
        } else if (!std::filesystem::is_directory(targetDirPath)) {
            std::cerr << "错误：目标路径 " << targetDirPath.string() << " 存在但不是一个目录。" << std::endl;
            CloseHandle(hPipe);
            return 1;
        }


        // 构建 minizip.exe 的命令行
        std::wstring commandLine =
            minizipExePath.wstring() + L" " +   // minizip.exe 路径（使用绝对路径的宽字符串）
            L"-x" + L" " +                           // -x 用于解压
            zipFilePath.wstring() + L" " +                     // ZIP 文件路径（使用绝对路径的宽字符串）
            L"-d" + L" " +                           // -d 用于目标目录
            quotedTargetPath;                        // 目标路径 (如果需要则加引号)
        
        // 将宽字符串命令行转换为多字节字符串以便日志输出
        std::cout << "构建的完整命令行字符串（用于 CreateProcessW）:" << std::endl;
        std::cout << std::string(commandLine.begin(), commandLine.end()) << std::endl;

        STARTUPINFOW si = { sizeof(si) }; // 初始化 STARTUPINFO 结构体
        PROCESS_INFORMATION pi;           // 初始化 PROCESS_INFORMATION 结构体

        // 创建 NUL 设备的句柄，用于重定向标准输入、输出和错误
        // 这可以防止子进程 (minizip.exe) 显示任何控制台窗口或输出。
        HANDLE hNul = CreateFileW(L"NUL", GENERIC_WRITE | GENERIC_READ, FILE_SHARE_WRITE | FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
        if (hNul == INVALID_HANDLE_VALUE) {
            std::cerr << "错误：无法打开 NUL 设备句柄，错误码: " << GetLastError() << std::endl;
            // 如果 NUL 重定向失败，则回退到隐藏窗口模式
            si.dwFlags = STARTF_USESHOWWINDOW;
            si.wShowWindow = SW_HIDE;
            std::cout << "回退到 STARTF_USESHOWWINDOW/SW_HIDE 模式。" << std::endl;
        } else {
            // 配置 STARTUPINFO 以使用自定义标准句柄
            si.dwFlags = STARTF_USESTDHANDLES; // 明确指出要使用自定义的标准句柄
            si.hStdInput = hNul; // 将标准输入重定向到 NUL
            si.hStdOutput = hNul; // 将标准输出重定向到 NUL
            si.hStdError = hNul;  // 将标准错误重定向到 NUL
            std::cout << "已配置标准输入、输出和错误重定向到 NUL。" << std::endl;
        }

        std::cout << "尝试启动解压进程..." << std::endl;
        // 创建子进程 (minizip.exe)
        BOOL success = CreateProcessW(
            NULL,               // lpApplicationName (使用 lpCommandLine 来指定完整路径 + 参数)
            &commandLine[0],    // lpCommandLine (此字符串会被原地修改，因此传递非 const)
            NULL,               // lpProcessAttributes
            NULL,               // lpThreadAttributes
            TRUE,               // bInheritHandles: **非常重要：必须为 TRUE，才能继承重定向的句柄**
            CREATE_NO_WINDOW,   // dwCreationFlags: 防止子进程出现新的控制台窗口
            NULL,               // lpEnvironment
            NULL,               // lpCurrentDirectory
            &si,                // lpStartupInfo (包含重定向信息)
            &pi                 // lpProcessInformation (接收进程信息)
        );

        // 检查进程创建是否成功
        if (success) {
            std::cout << "成功启动解压进程！进程ID: " << pi.dwProcessId << std::endl;
            CloseHandle(pi.hProcess); // 关闭进程句柄
            CloseHandle(pi.hThread);  // 关闭线程句柄
        } else {
            std::cerr << "错误：启动解压进程失败，错误码: " << GetLastError() << std::endl;
            // 额外打印当前工作目录，有时有助于调试文件找不到的问题
            wchar_t currentDir[MAX_PATH];
            GetCurrentDirectoryW(MAX_PATH, currentDir);
            std::wcout << L"当前工作目录: " << currentDir << std::endl;
        }

        // **重要：使用完后关闭 NUL 句柄**
        if (hNul != INVALID_HANDLE_VALUE) {
            CloseHandle(hNul);
            std::cout << "NUL 句柄已关闭。" << std::endl;
        }

    } else {
        std::cerr << "错误：从管道读取失败，错误码: " << GetLastError() << std::endl;
    }

    CloseHandle(hPipe); // 关闭管道句柄
    std::cout << "管道已关闭，程序退出。" << std::endl;
    return 0; // 成功退出
}