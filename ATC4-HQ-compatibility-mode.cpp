#include <iostream>
#include <windows.h>
namespace master {
    char 版本号 = NULL ;
}
main(int argc , char* argv[]) {
    // argc 是参数个数，argv 是参数数组  
    using namespace master;
    SetConsoleOutputCP(CP_UTF8);   // 设置输出为UTF-8
    SetConsoleCP(CP_UTF8);         // 设置输入为UTF-8
    if (argc < 1 /*前面是给到第几个参数*/+ 1 || std::atoi(argv[1]) != 114514) {
        std::cout << "参数错误" << std::endl;
        return false ;
    }
    std::cout
    << "欢迎使用ATC4-HQ" << std::endl
    << "版本号" << " : " << 版本号 << std::endl
    << "功能（在终端使用数字选择）" << std::endl
    << "1. 启动游戏" << std::endl;
    int mode;
    std::cin >> mode;
    if (mode == 1) {
        std::cout << "1.启动RJOO" << std::endl;
        std::cout << "2.其他机场" << std::endl;
        std::cin >> mode;
        if (mode == 1) {
            
        }
    }
}