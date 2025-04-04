#include <graphics.h>
#include <conio.h>
#include <iostream>
#include <windows.h>
#include <string>
namespace master {
	int a = GetSystemMetrics(SM_CXSCREEN); //这个变量int a是用户显示屏高
	int b = a /4; //这个变量int b是程序运行时的高和宽
	if (a  < 540) {
		bool jianrongmoushi = true; //如果应用宽度小于540，则设置为true
	} if else  (b > 540) {
		bool jianrongmoushi = false; //如果应用宽度大于540，则设置为false
		b = 540; //将应用高度设置为540
	} else {
		bool jianrongmoushi = false; //如果应用宽度大于540，则设置为false
	}
		
	// 检查鼠标是否在按钮区域内
    bool c (int x, int y, int btnX, int btnY, int btnWidth, int btnHeight) {
    return x >= btnX && x <= btnX + btnWidth && y >= btnY && y <= btnY + btnHeight;
	}
	bool sa (int x, int y, int btnX1, int btnY, int btnWidth, int btnHeight) {
		return x >= btnX1 && x <= btnX1 + btnWidth && y >= btnY && y <= btnY + btnHeight;
		}
	//如果鼠标在按钮区域内，返回布尔值c true，否则返回false
	int qidongyouxianniudeX = b / 3 - 50; //a和b按钮的X坐标
	int qidongyouxianniudeY = b / 3 * 2; //a和b按钮的Y坐标
	void clearButton(int x, int y, int width, int height) {
		setfillcolor(WHITE); // 设置填充颜色为白色
		solidrectangle(x, y, x + width, y + height); // 用白色填充按钮区域
	}
	void chongzhipingmu() {
		setfillcolor(WHITE); // 设置填充颜色为白色
		solidrectangle(0, 0, b, b); // 用白色填充整个屏幕
		IMAGE img; //定义一个图片对象
		const char * a91 = "ATC4.ico"; //图片路径
		loadimage(&img , a91 , b , b ,false); //加载图片
		putimage(0,0 , &img ); //在屏幕上显示图片
	}
	int qidongqitajichangdeX = b / 3 * 2 - 50; //a和b按钮的X坐标
	int qidongqitajichangdeY = b / 3 * 2; //a和b按钮的Y坐标
}
int main() {
    // if () {
	using namespace master;
	initgraph(b,b ,EX_SHOWCONSOLE); //初始化图形窗口
	SetConsoleOutputCP(936); //设置控制台输出编码为GBK
	chongzhipingmu(); //清屏
	// 按钮位置和大小
    int btnX =   qidongyouxianniudeX , btnY =  qidongyouxianniudeY , btnWidth = 100, btnHeight = 50;
	int btnX1 =   qidongqitajichangdeX , btnY1 =  qidongqitajichangdeY + 100 , btnWidth1 = 100, btnHeight1 = 50;
	// 绘制按钮
    setfillcolor(LIGHTGRAY);
    solidrectangle(btnX, btnY, btnX + btnWidth, btnY + btnHeight);
    settextstyle(20, 0, _T("仿宋"));
    outtextxy(btnX + 10, btnY + 15, _T("启动游戏"));
	while (true)
	{
		// 检查鼠标点击
		if (MouseHit()) {
			MOUSEMSG msg = GetMouseMsg();
			if (msg.uMsg == WM_LBUTTONDOWN) {
				if (c(msg.x, msg.y, btnX, btnY, btnWidth, btnHeight)) {
					// 按钮被点击
					chongzhipingmu(); //清屏
					break;
				}
			}
		}
	}
	// a绘制按钮
    setfillcolor(LIGHTGRAY);
    solidrectangle(btnX, btnY, btnX + btnWidth, btnY + btnHeight);
    settextstyle(20, 0, _T("仿宋"));
    outtextxy(btnX + 10, btnY + 15, _T("启动RJOO"));
	// b绘制按钮
    setfillcolor(LIGHTGRAY);
	btnWidth1 + 50;
    solidrectangle(btnX1, btnY, btnX1 + btnWidth1, btnY + btnHeight1);
    settextstyle(20, 0, _T("仿宋"));
    outtextxy(btnX1 + 10, btnY + 15, _T("启动其他机场"));
	while (true) {
		// 检查鼠标点击
		if (MouseHit()) {
			MOUSEMSG msg = GetMouseMsg();
			if (msg.uMsg == WM_LBUTTONDOWN) {
				if (c(msg.x, msg.y, btnX1, btnY, btnWidth, btnHeight)) {
					// b按钮被点击
					const char* world = "文件/RJAA.dll";
					const char* the = "XPACK.dll" ;
					if (CopyFileA(world, the, FALSE)) {
						// 文件复制成功
    					std::wstring command = L"C:\\LE\\LEProc.exe -run AXA.exe";
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
						// 文件复制失败
						return false;
					}
					break;
				}
			}
		}
		if (MouseHit()) {
			MOUSEMSG msg = GetMouseMsg();
			if (msg.uMsg == WM_LBUTTONDOWN) {
				if (sa(msg.x, msg.y, btnX, btnY, btnWidth, btnHeight)) {
					// a按钮被点击
					const char* world = "文件/RJOO.dll";
					const char* the = "XPACK.dll" ;
					if (CopyFileA(world, the, FALSE)) {
						// 文件复制成功
						Sleep(1000); // 等待1秒
    					std::wstring command = L"B:\\Locale-Emulator-2.5.0.1\\LEProc.exe -run AXA.exe";
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
						// 文件复制失败
						return false;
					}
					break;
				}
			}
		}
	}
	closegraph();
	return 0;
}