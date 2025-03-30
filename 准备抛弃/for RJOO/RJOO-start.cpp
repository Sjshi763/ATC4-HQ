//第四次重写
//终于还是用上电脑开发了
//vs2022都用过了
//c++太难了！！！
//ok依然是开始知道一下我们这个项目的任务
//1、我们需要把文件RJOO.dll从“文件”目录复制到“暂存”
//2、把他改名为XPACK.dll
//3、我们把“XPACK.dll”复制到更目录人后删掉“暂存”里的内容
//4、我们需要知道当前文件目录
//5、我们用命令行使用Locale Emulatorv启动ATC4
//ATC4的程序名为AXA.exe，在此文件的同级目录
//Locale Emulatorv在C:\LE\LEProc.exe
#include <iostream>
#include <windows.h>
#include <filesystem>
#include <string>
//这些头文件是用来干活的，也不知道为啥要他们
using namespace std;
namespace fs = std::filesystem;
int *A = NULL;
int main() {
    // 定义源文件路径和目标文件路径
    fs::path source = "文件/RJOO.dll";
    fs::path source = "文件/RJOO.dll";
    fs::path final_destination = "XPACK.dll";

    try {
        // 复制文件
        fs::copy_file(source, destination, fs::copy_options::overwrite_existing);
        cout << "文件复制成功！" << endl;

        // 重命名文件
        fs::rename(destination, final_destination);
        cout << "文件重命名成功！" << endl;
    } catch (fs::filesystem_error& e) {
        cout << "操作失败: " << e.what() << endl;
        cout << *A << endl;
    }

    try {
        // 删除暂存目录中的文件
        fs::remove(destination);
        cout << "暂存目录中的文件删除成功！" << endl;
    } catch (fs::filesystem_error& e) {
        cout << "操作失败: " << e.what() << endl;
        cout << *A << endl;
    }
    int *B = NULL;
    // 获取文件目录
    TCHAR A[MAX_PATH];
    DWORD length = GetModuleFileName(NULL, A, MAX_PATH);
    if (length > 0) {
        basic_string<TCHAR> filePath(A, length);
        #ifdef UNICODE
            wcout << L"原来我们在 " << filePath << endl;
        #else
            cout << "原来我们在 " << filePath << endl;
        #endif
    } else {
        cout << "获取文件目录失败！" << endl;
        cout << *B << endl;
    }

    // 调用命令行启动 Locale Emulator 和 ATC4
    STARTUPINFOW si = { sizeof(si) };
    PROCESS_INFORMATION pi;
    wstring command = L"C:\\LE\\LEProc.exe -run AXA.exe";

    if (CreateProcessW(NULL, &command[0], NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi)) {
        wcout << L"ATC4 启动成功！" << endl;
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
    } else {
        wcout << L"ATC4 启动失败，错误代码: " << GetLastError() << endl;
        cout << *A << endl;
    }
    return 0;
}