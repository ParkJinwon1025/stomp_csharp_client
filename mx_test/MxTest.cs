// MxTest.cs
using System;
using ActUtlTypeLib;

// 진입점
MxTest.Run();

class MxTest
{
    public static void Run()
    {
        var plc = new ActUtlType();

        // ========== 설정 ==========
        plc.ActLogicalStationNumber = 1; // 논리국번
        // ==========================

        // PLC 연결
        int ret = plc.Open();
        if (ret != 0)
        {
            Console.WriteLine($"[ERROR] 연결 실패. 에러코드: {ret:X}");
            return;
        }
        Console.WriteLine("[INFO] PLC 연결 성공");

        // B1000 읽기
        int value = 0;
        ret = plc.GetDevice("B1000", out value);
        if (ret != 0)
            Console.WriteLine($"[ERROR] 읽기 실패. 에러코드: {ret:X}");
        else
            Console.WriteLine($"[INFO] B1000 값: {value}");

        // 연결 종료
        plc.Close();
        Console.WriteLine("[INFO] PLC 연결 종료");
    }
}