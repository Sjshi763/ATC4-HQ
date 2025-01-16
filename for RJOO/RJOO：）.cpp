//他妈的第一次开发在手机上也是神力
//第三次重来了
//努力
//首先来让我们熟悉要求
//首先这个程序是用来帮助一款叫ATC4的游戏更好游玩而生
//并且默认它在游戏更目录
//我们的要求是
//1.找到自己的目录
//2.复制备用目录的RJOO.dll到缓存目录
//3.改名为XPACK.dll
//4.复制到根目录
#include <iostream>
//个人没看过这个文件不知道干啥的，后人如果知道请补一下谢谢
#include <windows.h>
//这个头文件是为了输出中文字符
#include <stdio.h>
//这个和前面一样不知道
namespace master {
    int A = 0;
}
//这是一个命名空间master是项目主人的
//后人要是要来那就再搞一个命名空间再加入吧！
int main() {
    SetConsoleOutputCP(65001);
    //这是为了输出中文 所以改cmd的编码格式为utf8
    using namespace master;
    std::cout << "欢迎使用ATC4启动RJOO辅助程序" << std::endl;
    std::cout << "爷会获取你的目录，就是这个程序在哪里" << std::endl;
    std::cout << "不过你看到也来不及力" << std::endl;
    std::cout << "我们不会存储你的信息，我也没钱存" << std::endl;
    TCHAR B[MAX_PATH];
    //定义TCHAR的缓冲区 B
    DWORD length = GetModuleFileName(NULL, B ,MAX_PATH);
    //不知道原理，但是会调用这个函数到B缓冲区
    if (length > 0) {
        std::wcout << L"我去，也是成功获取地址力"<< B << std::endl;
    }
    else {
        std::cerr << L"不对啊，应该成功的啊" << std::endl;
    }
    return 0;
}