#include "namespace-master-nogui.cpp"
int main(int argc , char* argv[]) {
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
    << "版本号" << " : " << banbenhao << std::endl
    << "功能（在终端使用数字选择）" << std::endl
    << "1. 启动游戏" << std::endl;
    int mode;
    std::cin >> mode;
    if (mode == 1) {
        std::cout << "1.启动RJOO" << std::endl;
        std::cout << "2.其他机场" << std::endl;
        std::cin >> mode;
        if (mode == 1) {
            system("copy .\\文件\\RJOO.dll .\\ATC4\\XPACK.dll"); // 复制文件
            // 文件复制成功
            std::ifstream inputFile("ATC4-HQ.ini"); // 打开文件);
            if (!inputFile) {
                std::cerr << "文件打开失败！" << std::endl;
                return false; // 文件打开失败
            }
            // 读取文件内容
            std::string line;
            // 读取第一行（跳过）
            if (std::getline(inputFile, line)) {
                // 读取第二行
                if (std::getline(inputFile, line) && !line.empty()) {
                    //有第二行
                    std::wstring LEdizhi(line.begin(), line.end()); // 将第二行转换为wstring
                    std::wstring command = LEdizhi + L"\\LEProc.exe" + L" " + L"-run .\\ATC4\\AXA.exe";
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
                    printf("没有第二行\n");
                    //没有第二行
                    TCHAR szBuffer[MAX_PATH] = {0};
                    BROWSEINFO bi = { 0 }; // 初始化BROWSEINFO结构
                    bi.lpszTitle = _T("请选择一个文件夹:"); // 设置对话框标题
                    bi.ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE; // 设置对话框样式
                    LPITEMIDLIST idl = SHBrowseForFolder(&bi); // 显示选择文件夹对话框
                    if (idl == NULL) {
                        std::cerr << "未选择文件夹！" << std::endl;
                        return false;
                    }
                    SHGetPathFromIDList(idl, szBuffer); // 获取选择的文件夹路径
                    // 转换为 std::string
                    std::string selectedPath;
                    #ifdef UNICODE
                    std::wstring ws(szBuffer); // 转换为宽字符串
                    selectedPath = std::string(ws.begin(), ws.end()); // 转换为多字节字符串
                    #else
                    selectedPath = std::string(szBuffer); // 直接转换为字符串
                    #endif				
                    #ifdef UNICODE
                    std::wstring ws(szBuffer); // 转换为宽字符串
                    std::string path(ws.begin(), ws.end()); // 转换为多字节字符串
                    #else
                    std::string path(szBuffer); // 直接转换为字符串
                    #endif
                    updateSecondLineInFile("ATC4-HQ.ini", path, 2); // 更新第二行内容
                } 
                inputFile.close(); // 关闭文件
            }
        }
        if (mode == 2) {
            system("copy .\\文件\\RJAA.dll .\\ATC4\\XPACK.dll"); // 复制文件
            // 文件复制成功
            std::ifstream inputFile("ATC4-HQ.ini"); // 打开文件);
            if (!inputFile) {
                std::cerr << "文件打开失败！" << std::endl;
                return false; // 文件打开失败
            }
            // 读取文件内容
            std::string line;
            // 读取第一行（跳过）
            if (std::getline(inputFile, line)) {
                // 读取第二行
                if (std::getline(inputFile, line) && !line.empty()) {
                    //有第二行
                    std::wstring LEdizhi(line.begin(), line.end()); // 将第二行转换为wstring
                    std::wstring command = LEdizhi + L"\\LEProc.exe" + L" " + L"-run .\\ATC4\\AXA.exe";
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
                    printf("没有第二行\n");
                    //没有第二行
                    TCHAR szBuffer[MAX_PATH] = {0};
                    BROWSEINFO bi = { 0 }; // 初始化BROWSEINFO结构
                    bi.lpszTitle = _T("请选择一个文件夹:"); // 设置对话框标题
                    bi.ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE; // 设置对话框样式
                    LPITEMIDLIST idl = SHBrowseForFolder(&bi); // 显示选择文件夹对话框
                    if (idl == NULL) {
                        std::cerr << "未选择文件夹！" << std::endl;
                        return false;
                    }
                    SHGetPathFromIDList(idl, szBuffer); // 获取选择的文件夹路径
                    // 转换为 std::string
                    std::string selectedPath;
                    #ifdef UNICODE
                    std::wstring ws(szBuffer); // 转换为宽字符串
                    selectedPath = std::string(ws.begin(), ws.end()); // 转换为多字节字符串
                    #else
                    selectedPath = std::string(szBuffer); // 直接转换为字符串
                    #endif				
                    #ifdef UNICODE
                    std::wstring ws(szBuffer); // 转换为宽字符串
                    std::string path(ws.begin(), ws.end()); // 转换为多字节字符串
                    #else
                    std::string path(szBuffer); // 直接转换为字符串
                    #endif
                    updateSecondLineInFile("ATC4-HQ.ini", path, 2); // 更新第二行内容
                } 
                inputFile.close(); // 关闭文件
            }
        }
    }
}