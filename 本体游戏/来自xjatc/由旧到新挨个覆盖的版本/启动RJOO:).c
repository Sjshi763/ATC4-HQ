/*虽然但是第一次开发c c++BYD在手机我也是神人了
手机战神
好吧
在这几行我们来看看我们需要什么
我们需要在结束这个项目时
打开exe文件就可以打开ATC4然后没有bug的运行这玩意*/
#include <iostream>
#include <unistd.h>
#include <limits.h>
char A;
/*定义一个变量为A它可以存储字母和数字*/
char A = 'sb'
//这个变量我把它初始化了，他现在是sb
char B
//和前面差不多，为了存点东西所以搞了个变量，并且他现在是SB
char B = 'sb'
/*分界线*/
/*孩子
后面的代码我自己还在思考是什么
ai写的
but看起来不错
我会先学习下面的代码是什么意思
然后变成自己的东西*/
int main() {
    wchar_t filename[MAX_PATH]; // 定义一个足够大的缓冲区
        DWORD result = GetModuleFileNameW(NULL, filename, MAX_PATH); // 获取当前程序路径

            if (result == 0) {
                    std::wcerr << L"获取文件名失败！错误代码：" << GetLastError() << std::endl;
                            return 1;
                                } else if (result > MAX_PATH) {
                                        std::wcerr << L"缓冲区太小！需要 " << result << L" 个字符。" << std::endl;
                                                // 可以重新分配更大的缓冲区并再次调用 GetModuleFileNameW
                                                        wchar_t* dynamicFilename = new wchar_t[result];
                                                                GetModuleFileNameW(NULL, dynamicFilename, result);
                                                                        std::wcout << L"程序位置（动态分配）：" << dynamicFilename << std::endl;
                                                                                delete[] dynamicFilename;
                                                                                        return 1;
                                                                                            } else {
                                                                                                    std::wcout << L"程序位置：" << filename << std::endl;
                                                                                                        }

                                                                                                            return 0;
                                                                                                            }