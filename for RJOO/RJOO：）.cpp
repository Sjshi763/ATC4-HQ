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
#pragma execution_character_set("utf-8")
//还是不知道这是干啥的
namespace master {
    int *A = NULL;
    //定义一个空指针A用来在不对劲的时候把自己干崩
}
//这是一个命名空间master是项目主人的
//后人要是要来那就再搞一个命名空间再加入吧！
int main() {
    SetConsoleOutputCP(65001);
    //这是为了输出中文 所以改cmd的编码格式为utf8
    std::wcout.imbue(std::locale("en_US.utf8"));
    //AI告诉我要初始化代码
    //这样应该可以了
    using namespace master;
    using namespace std;
    std::cout << "欢迎使用ATC4启动RJOO辅助程序" << std::endl;
    std::cout << "爷会获取你的目录，就是这个程序在哪里" << std::endl;
    std::cout << "不过你看到也来不及力" << std::endl;
    std::cout << "我们不会存储你的信息，我也没钱存" << std::endl;
    TCHAR B[MAX_PATH];
    //定义TCHAR的缓冲区 B
    DWORD length = GetModuleFileName(NULL, B ,MAX_PATH);
    //不知道原理，但是会调用这个函数到B缓冲区
    if (length > 0) {
        std::wcout << B << std::endl;
        cout << "我们做到了（爱探险的多拉bgm）" << endl;
        std::cout << "byd下一步！！" << std::endl;
    }
    else {
        std::cout << "不对啊，应该成功的啊" << std::endl ;
        std::cout << "不管了！先报错！爷要崩溃乐！" << std::endl;
        std::cout << *A << std::endl;
        //访问空指针，然后直接给我崩吧！
    }
    return 0;
}