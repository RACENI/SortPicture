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
            Console.WriteLine("GPS기반정리는 '2'를 입력하십시오.(미구현)");
            Console.WriteLine("프로그램 종료는 '-1'를 입력하십시오.");
            try
            {
                int check = Convert.ToInt32(Console.ReadLine());

                if (check < 0)
                    return; // 프로그램 종료

                List<string> sortFilePaths = new List<string>(); // 분류된 파일 경로 리스트
                string Path = Directory.GetCurrentDirectory() + @"\working"; // 기준 경로

                if (Directory.Exists(Path))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(Path);

                    switch (check)
                    {
                        case 1:
                            sortFilePaths = sortPictureDate(dirInfo); // 날짜기반 파일 정리
                            break;
                        case 2:
                            sortFilePaths = Task.Run(() => sortPictureGPSAsync(dirInfo)).GetAwaiter().GetResult(); // GPS기반 파일 정리
                            break;
                        default:
                            return;
                    }

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
        /// 날짜기반 사진 정리 메소드
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <returns>분류된 파일들의 경로 리스트</returns>
        static private List<string> sortPictureDate(DirectoryInfo dirInfo)
        {
            List<string> doneFilePath = new List<string>(); // 완료한 파일 경로 저장 리스트
            foreach (var file in dirInfo.GetFiles())
            {
                string path = @"working/" + file.Name;

                try
                {
                    var directories = ImageMetadataReader.ReadMetadata(path);
                    var exifDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault(); // EXIF에서 날짜를 가져옴.
                    string dateTimeOriginal = exifDirectory.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal).ToString(); // 년월일만 사용할 예정이므로 형변환

                    char sp = ' ';
                    string[] spstring = dateTimeOriginal.Split(sp); // ["yyyy-mm-dd", "오전/오후", "hh:mm:ss"] 배열 생성

                    Console.WriteLine("완료한 파일 : " + path + " , 찍은 날짜 : " + spstring[0]);

                    string thisDir = Directory.GetCurrentDirectory(); // 현재 디렉터리 주소
                    if (Directory.Exists(thisDir + "\\" + spstring[0]) == false)
                        Directory.CreateDirectory(thisDir + "\\" + spstring[0]); // 날짜에 맞는 폴더가 없을 시 폴더 생성

                    File.Copy(thisDir + "\\" + path, thisDir + "\\" + spstring[0] + "\\" + file.Name, true); // 메타데이터에 해당하는 날짜에 추가
                    doneFilePath.Add(path);
                }
                catch(NullReferenceException e)
                {
                    Console.WriteLine("본 파일은 사진파일이 아니거나 날짜 메타데이터가 존재하지 않습니다.(" + path + ")");
                }
                catch(Exception e)
                {
                    Console.WriteLine("알 수 없는 오류로 처리하지 못했습니다.(" + path + ")");
                }
            }
            return doneFilePath;
        }

        /// <summary>
        /// GPS기반 사진 정리 메소드
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <returns>분류된 파일들의 경로 리스트</returns>
        static private async Task<List<string>> sortPictureGPSAsync(DirectoryInfo dirInfo)
        {
            List<string> doneFilePath = new List<string>(); // 완료한 파일 경로 저장 리스트

            foreach (var file in dirInfo.GetFiles())
            {

                string path = @"working/" + file.Name;

                try
                {
                    var directories = ImageMetadataReader.ReadMetadata(path);
                    double latitude = 0; // 서울특별시의 위도
                    double longitude = 0; // 서울특별시의 경도



                    var gpsDirectory = directories.OfType<GpsDirectory>().FirstOrDefault();

                    if (gpsDirectory != null)
                    {
                        var location = gpsDirectory.GetGeoLocation();
                        if (location != null)
                        {
                            Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}");
                            latitude = location.Latitude;
                            longitude = location.Longitude;
                        }
                    }
                    else
                    {
                        Console.WriteLine("본 파일에 위치 데이터가 없습니다.(" + path + ")");
                        continue;
                    }

                    string apiKey = ""; // Google Maps API 키 << 사용자가 입력할 수 있게끔할 예정
                    string url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latitude},{longitude}&key={apiKey}&language=ko";

                    using (HttpClient client = new HttpClient())
                    {
                        var response = await client.GetStringAsync(url);
                        var json = JObject.Parse(response);

                        // 모든 formatted_address 값을 가져오기
                        var formattedAddresses = new List<string>();
                        foreach (var result in json["results"])
                        {
                            var formattedAddress = result["formatted_address"]?.ToString();
                            if (formattedAddress != null)
                            {
                                formattedAddresses.Add(formattedAddress);
                                //"대한민국 대전광역시 유성구 문지동 636-1"
                            }
                        }
                    }

                    /*                    Console.WriteLine("완료한 파일 : " + path + " , 찍은 장소 : " + spstring[0]);

                                        string thisDir = Directory.GetCurrentDirectory(); // 현재 디렉터리 주소
                                        if (Directory.Exists(thisDir + "\\" + spstring[0]) == false)
                                            Directory.CreateDirectory(thisDir + "\\" + spstring[0]); // 

                                        File.Copy(thisDir + "\\" + path, thisDir + "\\" + spstring[0] + "\\" + file.Name, true); // 
                                        doneFilePath.Add(path);*/
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine("본 파일은 사진파일이 아닙니다.(" + path + ")");
                }
                catch (Exception e)
                {
                    Console.WriteLine("알 수 없는 오류로 처리하지 못했습니다.(" + path + ")");
                }
            }
            return null;
        }
    }
}

