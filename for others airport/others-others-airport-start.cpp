//这一次的项目基本就结束了
//剩下的就是换变量了
//start
#include <iostream>
#include <windows.h>
#include <filesystem>
#include <string>
//这些头文件是用来干活的，也不知道为啥要他们
using namespace std;
namespace fs = std::filesystem;

int main() {
        // 定义源文件路径和目标文件路径
            fs::path source = "文件/RJAA.dll";
                fs::path destination = "暂存/XPACK.dll";
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
                        }

                            try {
                                        // 删除暂存目录中的文件
                                                fs::remove(destination);
                                                        cout << "暂存目录中的文件删除成功！" << endl;
                            } catch (fs::filesystem_error& e) {
                                        cout << "操作失败: " << e.what() << endl;
                            }

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
                                                                }
                                                                    return 0;
}"
                                                                }
                                                                }
                                            }"
                                            }
                            }
                            }
                        }
                        }
}