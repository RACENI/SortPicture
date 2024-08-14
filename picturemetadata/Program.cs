using System;
using System.IO;
using System.Collections.Generic;
using MetadataExtractor.Formats.Exif;
using System.Linq;
using MetadataExtractor.Formats.Iptc;
using MetadataExtractor;
using Directory = System.IO.Directory;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace picturemetadata
{
    class Program
    {
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
            Console.WriteLine("날짜기반정리는 '1'을 입력하십시오.");
            Console.WriteLine("GPS기반정리는 '2'를 입력하십시오.");
            Console.WriteLine("프로그램 종료는 '-1'를 입력하십시오.");
            try
            {
                Console.Write(">");
                int check = Convert.ToInt32(Console.ReadLine());

                if (check < 0)
                    return; // 프로그램 종료

                List<string> sortFilePaths = new List<string>(); // 분류된 파일 경로 리스트
                string Path = Directory.GetCurrentDirectory() + @"\working"; // 기준 경로

                if (Directory.Exists(Path))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(Path);
                    ISortManager sortManager = GetSortClass(check);
                    sortFilePaths = sortManager.sortPictures(dirInfo); // 사진 분류 메소드

                    Console.WriteLine("총 파일 수 : " + dirInfo.GetFiles().Length);
                    Console.WriteLine("완료한 파일 수 : " + sortFilePaths.Count);
                }
                else
                {
                    Console.WriteLine("working 폴더가 없습니다.");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine("---------------------------------------------.");
                Console.WriteLine("기존 파일을 삭제하려면 '1'을 입력해주세요.");
                Console.WriteLine("프로그램 종료를 원하시면 엔터를 누르십시오.");
                Console.Write(">");
                check = Convert.ToInt32(Console.ReadLine());

                //기존 파일 삭제
                if (check == 1)
                {
                    foreach (string sortFilePath in sortFilePaths)
                    {
                        File.Delete(sortFilePath);
                    }
                    Console.WriteLine("삭제완료!");
                    Console.ReadLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("알 수 없는 오류로 처리하지 못했습니다.");
            }
        }

        /// <summary>
        /// 정리 기준 클래스 반환 메소드
        /// </summary>
        /// <param name="check"></param>
        /// <returns>정리 기준이 되는 class</returns>
        private static ISortManager GetSortClass(int check)
        {
            switch (check)
            {
                case 1:
                    return new DateManager();
                case 2:
                    return new GpsManager();
                default:
                    return null;
            }
        }
    }
}
