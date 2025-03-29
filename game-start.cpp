#include <graphics.h>
#include <conio.h>
#include <iostream>
#include <windows.h>
namespace master {
	int a = GetSystemMetrics(SM_CXSCREEN); //这个变量int a是用户显示屏高
	int b = a /4;
	// 检查鼠标是否在按钮区域内
    bool c (int x, int y, int btnX, int btnY, int btnWidth, int btnHeight) {
    return x >= btnX && x <= btnX + btnWidth && y >= btnY && y <= btnY + btnHeight;
	}
	//如果鼠标在按钮区域内，返回布尔值c true，否则返回false
}

int main() {
	using namespace master;
	SetConsoleOutputCP(CP_UTF8); //设置控制台输出编码为UTF-8
	initgraph(b,b);
	IMAGE img; //定义一个图片变量
	loadimage(&img , "ATC4.ico" , b , b ,false); //加载图片
	putimage(0,0 , &img ); //在屏幕上显示图片
	// 按钮位置和大小
    int btnX = 200, btnY = 200, btnWidth = 100, btnHeight = 50;
	// 绘制按钮
    setfillcolor(LIGHTGRAY);
    solidrectangle(btnX, btnY, btnX + btnWidth, btnY + btnHeight);
    settextstyle(20, 0, _T("仿宋"));
    outtextxy(btnX + 10, btnY + 15, _T(""));
	while ( bool x = true)
	{
		std::cout << "请点击按钮" << std::endl;
	}
	closegraph();
	return 0;
}