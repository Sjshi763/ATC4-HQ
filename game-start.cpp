#include <graphics.h>
#include <conio.h>
#include <iostream>
#include <windows.h>
namespace master {
	int a = GetSystemMetrics(SM_CXSCREEN); //这个变量int a是用户显示屏高
	int b = a /4;
}
int main() {
	using namespace master;
	SetConsoleOutputCP(CP_UTF8); //设置控制台输出编码为UTF-8
	initgraph(b,b,EX_SHOWCONSOLE);
	IMAGE img;
	loadimage(&img, "ATC4.ico" , b,b,false); //加载图片
	putimage(0,0 , &img); //在屏幕上显示图片
	
	closegraph();
	return 0;
}