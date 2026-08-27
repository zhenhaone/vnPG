using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using ExcelDataReader;

public class ExcelReader : MonoBehaviour
{
    public struct ExcelData
    {
        public string speakerName;
        public string speakingContent;
        public string avatarImageFileName;
        public string vocalAudioFileName;
        public string backgroundImageFileName;
        public string backgroundMusicFileName;
        public string CoordinateX1;
        public string CoordinateX2;
        public string CoordinateX3;
        public string charactor1Action;
        public string charactor2Action;
        public string charactor3Action;
        public string charactor1ImageFileName;
        public string charactor2ImageFileName;
        public string charactor3ImageFileName;
        public string lastBackgroundImage;
        public string lastBackgroundMusic;
        public string lastCoordinate1;
        public string lastCoordinate2;
        public string lastCoordinate3;
    }

    public static List<ExcelData> ReadExcel(string filePath)
    {
        List<ExcelData> excelData = new List<ExcelData>();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using (var stream =File.Open(filePath,FileMode.Open,FileAccess.Read))
        {
            using (var reader=ExcelReaderFactory.CreateReader(stream))
            {
                do
                {
                    while (reader.Read())
                    {
                        ExcelData data = new ExcelData();
                        data.speakerName = reader.IsDBNull(0)?string.Empty:reader.GetValue(0)?.ToString();
                        data.speakingContent = reader.IsDBNull(1) ? string.Empty : reader.GetValue(1)?.ToString();
                        data.avatarImageFileName = reader.IsDBNull(2) ? string.Empty : reader.GetValue(2)?.ToString();
                        data.vocalAudioFileName = reader.IsDBNull(3) ? string.Empty : reader.GetValue(3)?.ToString();
                        data.backgroundImageFileName = reader.IsDBNull(4) ? string.Empty : reader.GetValue(4)?.ToString();
                        data.backgroundMusicFileName= reader.IsDBNull(5) ? string.Empty : reader.GetValue(5)?.ToString();
                        data.charactor1Action= reader.IsDBNull(6) ? string.Empty : reader.GetValue(6)?.ToString();
                        data.CoordinateX1= reader.IsDBNull(7) ? string.Empty : reader.GetValue(7)?.ToString();
                        data.charactor1ImageFileName = reader.IsDBNull(8) ? string.Empty : reader.GetValue(8)?.ToString();
                        data.charactor2Action= reader.IsDBNull(9) ? string.Empty : reader.GetValue(9)?.ToString();
                        data.CoordinateX2 = reader.IsDBNull(10) ? string.Empty : reader.GetValue(10)?.ToString();
                        data.charactor2ImageFileName= reader.IsDBNull(11) ? string.Empty : reader.GetValue(11)?.ToString();
                        data.lastBackgroundImage = reader.IsDBNull(12) ? string.Empty : reader.GetValue(12)?.ToString();
                        data.lastBackgroundMusic = reader.IsDBNull(13) ? string.Empty : reader.GetValue(13)?.ToString();
                        data.lastCoordinate1 = reader.IsDBNull(14) ? string.Empty : reader.GetValue(14)?.ToString();
                        data.lastCoordinate2 = reader.IsDBNull(15) ? string.Empty : reader.GetValue(15)?.ToString();
                        data.charactor3ImageFileName = reader.IsDBNull(16) ? string.Empty : reader.GetValue(16)?.ToString();
                        data.charactor3Action = reader.IsDBNull(17) ? string.Empty : reader.GetValue(17)?.ToString();
                        data.CoordinateX3 = reader.IsDBNull(18) ? string.Empty : reader.GetValue(18)?.ToString();
                        data.lastCoordinate3 = reader.IsDBNull(19) ? string.Empty : reader.GetValue(19)?.ToString();
                        excelData.Add(data);
                    }
                } while (reader.NextResult());
            }
        }
        return excelData;
    }
}
 