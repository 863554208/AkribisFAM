//#include <stdio.h>
//#include <stdbool.h>

//struct station
//{
//    int id;
//    int capacity;
//    int stat; //top, bottom
//    int isDone; //done
//    int delta;
//};

//enum IO_INDEX
//{
//    gongzhan1jinliao = 0,
//    gongzhan1dingqi,

//    gongzhan2jinliao,
//    gonzhan2dingqi,

//    gongzhan3jinliao,
//    gonzhan3dingqi,

//    gongzhan4jinliao,
//    gonzhan4dingqi,

//    gongzhantotal,
//};

//struct manager
//{
//    struct station st[4];
//    int IO[gongzhantotal];
//};

//struct manager m;

//void move(int speed)
//{

//}

//int timer()
//{
//    int a = 5;
//    return a;
//}

//int saoma_jiguang = 0;

///***************qiuxinguo *****************/
//int sendMessage(const char* str)
//{
//    ///TODO
//    int saoma;
//    return saoma;
//}

//void wait(int delta, int* IOarr, int size, int type)
//{
//    int timestamp = timer();

//    if (delta != 0 && IOarr != NULL && size != 0)
//    {
//        while (timer() - timestamp < delta)
//        {
//            //if(m.IO[1] == 1) break;
//            //if(m.IO[3] == 1) break;
//            //if(m.IO[3] == 1) break;
//            int judge = 0;
//            for (int i = 0; i < size; ++i)
//            {
//                judge += IOarr[i];
//            }
//        }
//    }
//    else
//    {
//        switch (type)
//        {
//            case 0: //1号工站
//                while (sendMessage("qusaomaba") == 1) ;
//                break;
//        }
//    }
//}


//int getIO(int IOID)
//{
//    return 1;
//}

//void dingqi(int type)
//{
//    switch (type)
//    {
//        case 0:
//            m.IO[gongzhan1dingqi] = true;
//    }
//}


//void step1()
//{
//step1: //进板
//    move(200);
//    int arr[3] = { m.IO[gongzhan1dingqi], m.IO[gongzhan3jinliao], m.IO[gonzhan4dingqi] };
//    int delta = 999999;
//    wait(delta, arr, 3, 0);
//    dingqi(0);
//    wait(0, 0, 0, 0);

//step2:



//}


//int main()
//{

//    int arr[3] = { m.IO[gongzhan1dingqi], m.IO[gongzhan3jinliao], m.IO[gonzhan4dingqi] };

//    int delta = 999999;



//    wait(delta, arr, 3, 0);
//    dingqi(0);
//    wait(0, 0, 0, 0);



//    printf("Hello World!\n");
//    return 0;
//}