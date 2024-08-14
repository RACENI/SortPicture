using MetadataExtractor.Formats.Exif;
using MetadataExtractor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Directory = System.IO.Directory;

namespace picturemetadata
{
    internal class GpsManager : ISortManager
    {

        /// <summary>
        /// GPS기반 사진 정리 메소드
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <returns>분류된 파일들의 경로 리스트</returns>
        public List<string> sortPictures(DirectoryInfo dirInfo)
        {
            List<string> doneFilePath = new List<string>(); // 완료한 파일 경로 저장 리스트
            List<string> referAddrList = new List<string>(); // 기준 지역 리스트
            string apiKey = getAPIKey(); // apiKey 가져옴

            foreach (var file in dirInfo.GetFiles())
            {
                string fileName = file.Name;
                string path = @"working/" + fileName;

                try
                {
                    var directories = ImageMetadataReader.ReadMetadata(path);
                    double latitude = 0; // 위도
                    double longitude = 0; // 경도

                    var gpsDirectory = directories.OfType<GpsDirectory>().FirstOrDefault(); // GpsDirectory에서 날짜를 가져옴.
                    if (gpsDirectory != null)
                    {
                        var location = gpsDirectory.GetGeoLocation();
                        if (location != null)
                        {
                            latitude = location.Latitude;
                            longitude = location.Longitude;
                        }
                    }
                    else
                    {
                        Console.WriteLine("본 파일에 위치 데이터가 없습니다.(" + path + ")");
                        continue;
                    }

                    string referAddr = Task.Run(async () => await getSaveAddr(apiKey, latitude, longitude, referAddrList))
                        .GetAwaiter().GetResult(); // 기준 지역 받아옴
                                                   // list를 reference형식으로 던져서 modify 가능하게 함


                    FileManager fileManager = new FileManager();
                    fileManager.saveFile(referAddr, path, fileName); // 파일 저장

                    doneFilePath.Add(path); // 완료된 파일 list에 추가
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
            return new List<string>(doneFilePath); // for value return
        }

        /// <summary>
        /// apiKey 반환 메소드
        /// </summary>
        /// <returns>apiKey</returns>
        private string getAPIKey()
        {
            FileManager fileManager = new FileManager();
            string apiKey;
            int check = -1;
            Console.WriteLine("---------------------------------------------.");
            if (fileManager.getConfigFile().googleApiKey != null)
            {
                Console.WriteLine("저장된 구글 api키를 사용하려면 '1'을 입력하세요.");
                Console.WriteLine("새로운 구글 api키를 사용하려면 '2'를 입력하세요.");
                Console.Write(">");
                check = Convert.ToInt32(Console.ReadLine());
            }

            if (check == 1)
            {
                apiKey = fileManager.getConfigFile().googleApiKey;
            }
            else
            {
                Console.WriteLine("발급받은 구글 api키를 입력하십시오.");
                Console.Write(">");
                apiKey = Console.ReadLine();


                //입력한 googleAPI Key는 config.json에 저장됨
                AppSettings appSettings = new AppSettings();
                appSettings.googleApiKey = apiKey;
                fileManager.modifyConfigFile(appSettings);
            }
            return apiKey;
        }

        /// <summary>
        /// 저장할 지역명 반환 메소드
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="referAddrList"></param>
        /// <returns>저장할 지역명</returns>
        private async Task<string> getSaveAddr(string apiKey, double latitude, double longitude, List<string> referAddrList)
        {
            string url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latitude},{longitude}&key={apiKey}&language=ko";

            List<string> choiceAddrs = new List<string>(); // 기준 지역 선택을 위한 리스트

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);
                var json = JObject.Parse(response);

                if (json["error_message"]?.ToString() != null)
                {
                    Console.WriteLine("api키를 정확히 입력하십시오.");
                    Console.ReadLine();
                    Environment.Exit(0);
                }

                // 구글에서 제공한 모든 formatted_address 값을 가져오기
                foreach (var result in json["results"])
                {
                    var formattedAddress = result["formatted_address"]?.ToString(); // 예시 데이터 ("대한민국 대전광역시 유성구 문지동 636-1")
                    if (formattedAddress != null)
                    {
                        string[] addrs = formattedAddress.Split(' ');

                        // 해당 사진의 메타데이터에 기준 지역의 존재유무 검사
                        // 배열의 마지막 요소가 더 작은 단위의 도시임
                        for (int i = addrs.Length - 1; i >= 0; i--)
                        {
                            string addr = addrs[i];

                            if (referAddrList.Contains(addr)) // 존재시 해당하는 지역 return
                            {
                                return addr;
                            }
                            else // 미존재시 기준 지역 선택 리스트에 추가
                            {
                                if (!choiceAddrs.Contains(addr)) // 단, 기준 지역 선택 리스트에 존재하지 않을 때 추가
                                    choiceAddrs.Add(addr);
                            }
                        }
                    }
                }
                return choiceAddr(new List<string>(choiceAddrs), referAddrList); // 선택한 지역명으로 반환
            }
        }

        /// <summary>
        /// 기준 지역명 선택 메소드
        /// </summary>
        /// <param name="choiceAddrs"></param>
        /// <param name="referAddrList"></param>
        /// <returns>선택한 기준 지역명 반환</returns>
        private string choiceAddr(List<string> choiceAddrs, List<string> referAddrList)
        {
            do
            {
                Console.WriteLine("---------------------------------------------.");
                Console.WriteLine("저장할 지역명을 입력해주십시오.(철자를 정확히 맞추셔야 합니다.)");

                //지역 선택 유도
                foreach (string choiceAddr in choiceAddrs)
                {
                    Console.WriteLine($">>{choiceAddr}");
                }
                Console.WriteLine("---------------------------------------------.");
                Console.Write(">");
                string saveAddr = Console.ReadLine();

                if (choiceAddrs.Contains(saveAddr))
                {
                    referAddrList.Add(saveAddr); // 기준 지역 list에 추가
                    return saveAddr; // 제대로 입력했을 시 지역명 반환
                }
            } while (true);
        }
    }
}
