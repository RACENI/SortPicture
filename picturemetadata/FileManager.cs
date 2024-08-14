using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace picturemetadata
{
    internal class FileManager
    {
        /// <summary>
        /// referName기준으로 파일 저장
        /// </summary>
        /// <param name="referName"></param>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        public void saveFile(string referName, string path, string fileName)
        {
            string thisDir = Directory.GetCurrentDirectory(); // 현재 디렉터리 주소
            if (Directory.Exists(thisDir + "\\" + referName) == false)
                Directory.CreateDirectory(thisDir + "\\" + referName); // 

            File.Copy(thisDir + "\\" + path, thisDir + "\\" + referName + "\\" + fileName, true);

            Console.WriteLine("완료한 파일 : " + path + " , 찍은 장소 : " + referName);
        }

        /// <summary>
        /// config파일을 수정하는 메소드
        /// </summary>
        /// <param name="appSettings"></param>
        public void modifyConfigFile(AppSettings appSettings)
        {
            string filePath = "config.json";

            AppSettings settings;
            if (File.Exists(filePath))
            {
                var jsonString = File.ReadAllText(filePath);
                settings = JsonConvert.DeserializeObject<AppSettings>(jsonString);
            }
            else
            {
                // 파일이 존재하지 않는 경우 기본값으로 초기화
                settings = new AppSettings
                {
                    googleApiKey = ""

                };
            }

            if(appSettings.googleApiKey != null && appSettings.googleApiKey != "")
                settings.googleApiKey = appSettings.googleApiKey;

            // JSON으로 직렬화
            var updatedJsonString = JsonConvert.SerializeObject(settings, Formatting.Indented);

            // 파일에 저장
            File.WriteAllText(filePath, updatedJsonString);
        }

        /// <summary>
        /// config 파일에 있는 value를 가져오는 메소드
        /// </summary>
        /// <returns>AppSettings 객체가 반환</returns>
        public AppSettings getConfigFile()
        {
            string filePath = "config.json";

            if (File.Exists(filePath))
            {
                // JSON 파일 읽기
                var jsonString = File.ReadAllText(filePath);

                // JSON 문자열을 설정 객체로 변환
                var settings = JsonConvert.DeserializeObject<AppSettings>(jsonString);


                return settings;
            }
            else
            {
                return new AppSettings();
            }
        }
    }
}
