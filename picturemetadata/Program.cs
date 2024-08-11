using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace picturemetadata
{
    class Program
    {
        static List<String> lst = new List<string>(); // 분류한 파일 경로를 담은 리스트
        static int count = 0; // 총 변경한 파일 갯수
        
        static void Main(string[] args)
        {
            Console.WriteLine("사진 정리 프로그램에 오신 것을 환영합니다.");
            Console.WriteLine("수정 및 배포를 할 때에는 출처를 밝혀주시길 바랍니다.");
            Console.WriteLine("제작 : SARACEN (saracen_dev@naver.com)");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("※사용법※");
            Console.WriteLine("1. 프로그램이 있는 폴더 안에 \"working\"폴더를 만든다");
            Console.WriteLine("2. working폴더 안에 정리할 사진파일을 넣는다.");
            Console.WriteLine("3. 준비가 완료 되었으면 프로그램을 재시작합니다.");
            Console.WriteLine("---------------------------------------------.");
            Console.WriteLine("계속 하실 거면 '1'을 입력하시고 엔터를 눌러주십시오. 그렇지 아니하면 엔터를 눌러주십시오.");
            Console.WriteLine("단, 기존 파일은 삭제됩니다.");
            try
            {
                int check = Convert.ToInt32(Console.ReadLine());

                if (check == 1)
                {
                    int total = 0;
                    string Path = @Directory.GetCurrentDirectory() + @"\working";

                    if (Directory.Exists(Path))
                    {
                        DirectoryInfo di = new DirectoryInfo(Path);

                        foreach (var item in di.GetFiles())
                        {
                            sortPicture(item.Name);
                            total++;
                        }
                        foreach (string a in lst)
                        {
                            File.Delete(a);
                        }

                        Console.WriteLine("총 파일 수 : " + total);
                        Console.WriteLine("완료한 파일 수 : " + count);

                    }
                    else
                    {
                        Console.WriteLine("working 폴더가 없습니다.");
                    }
                    Console.WriteLine("엔터를 눌러주십시오.");
                    Console.ReadLine();
                }
            }
            catch
            {

            }
            
        }

        
        //사진 정렬
        static public void sortPicture(string fileName)
        {
            string path = @"working/" + fileName;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                try
                {
                    BitmapSource img = BitmapFrame.Create(fs);
                    BitmapMetadata md = (BitmapMetadata)img.Metadata;

                    string date = md.DateTaken; // 사진 메타데이터의 date 가져오기

                    char sp = ' ';

                    string[] spstring = date.Split(sp);

                    Console.WriteLine("완료한 파일 : "+ path + " , 찍은 날짜 : "+spstring[0]);


                    if(Directory.Exists(@Directory.GetCurrentDirectory() + "\\" + spstring[0]) == false)
                    {
                        Directory.CreateDirectory(@Directory.GetCurrentDirectory() + "\\" + spstring[0]);
                    }
                    File.Copy(Directory.GetCurrentDirectory() + "\\" + path, Directory.GetCurrentDirectory() + "\\" + spstring[0] + "\\" + fileName, true);
                    lst.Add(Directory.GetCurrentDirectory() + "\\" + path); // 기존파일 삭제를 위한 리스트 담기
                    count++;
                }
                catch
                {
                    Console.WriteLine("본 파일은 사진파일이 아닙니다.("+ path + ")");
                }
            }
        }
    }
}
