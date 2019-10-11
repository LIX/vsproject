// C++Learning.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include <iostream>
#include <string>
#include <vector>
using namespace std;
using std::string;
using std::vector;

struct Foo
{
	std::string bookNo;
	uint32_t units_sold = 0;
	double revenue = 0;
};

int main()
{
	vector<int> ivec;
	int i;
	while (cin>>i)
	{
		if (i != 111)
			ivec.push_back(i);
		else
			break;
	}
    return 0;
}

