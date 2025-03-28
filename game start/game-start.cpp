#include <graphics.h>
#include <conio.h>
#include <iostream>
#include <windows.h>
namespace master {
	int a = GetSystemMetrics(SM_CXSCREEN)/4; //这个
}
int main() {
	using namespace master;
	initgraph(master::a, master::b);
	
	closegraph();
	return 0;
}