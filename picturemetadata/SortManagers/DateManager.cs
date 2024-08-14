using MetadataExtractor.Formats.Exif;
using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Directory = System.IO.Directory;

namespace picturemetadata
{
    internal class DateManager : ISortManager
    {
        /// <summary>
        /// 날짜기반 사진 정리 메소드
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <returns>분류된 파일들의 경로 리스트</returns>
        public List<string> sortPictures(DirectoryInfo dirInfo)
        {
            List<string> doneFilePath = new List<string>(); // 완료한 파일 경로 저장 리스트
            foreach (var file in dirInfo.GetFiles())
            {
                string fileName = file.Name;
                string path = @"working/" + fileName;

                try
                {
                    var directories = ImageMetadataReader.ReadMetadata(path);
                    var exifDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault(); // EXIF에서 날짜를 가져옴.
                    string dateTimeOriginal = exifDirectory.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal).ToString(); // 년월일만 사용할 예정이므로 형변환

                    char sp = ' ';
                    string[] dateArr = dateTimeOriginal.Split(sp); // ["yyyy-mm-dd", "오전/오후", "hh:mm:ss"] 배열 생성

                    Console.WriteLine("완료한 파일 : " + path + " , 찍은 날짜 : " + dateArr[0]);


                    FileManager fileManager = new FileManager();
                    fileManager.saveFile(dateArr[0], path, fileName); // 파일 저장

                    doneFilePath.Add(path); // 완료된 파일 list에 추가
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine("본 파일은 사진파일이 아니거나 날짜 메타데이터가 존재하지 않습니다.(" + path + ")");
                }
                catch (Exception e)
                {
                    Console.WriteLine("알 수 없는 오류로 처리하지 못했습니다.(" + path + ")");
                }
            }
            return new List<string>(doneFilePath); // for value return
        }
    }
}
