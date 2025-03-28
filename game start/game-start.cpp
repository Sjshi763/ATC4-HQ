#include <graphics.h>
#include <conio.h>
#include <iostream>
#include <windows.h>
namespace master {
	int a = GetSystemMetrics(SM_CXSCREEN)/4; //这个变量int a是用户显示屏高的四分之一
	int b = 
}
int main() {
	using namespace master;
	loadimage(NULL , "/ATC4.ico"); //加载图片
	initgraph(,);
	putimage(a, 0, "/ATC4.ico"); //在屏幕上显示图片
	closegraph();
	return 0;
}