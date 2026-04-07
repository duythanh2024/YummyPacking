using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class Utilities
{
    public static string policy_url = "https://www.brightplay.games/2026/02/privacy-policy.html";
    public static string term_url = "https://www.brightplay.games/2026/02/terms-conditions.html";
public static bool CheckNetWork()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
            return false;

        return true;

    }
public static async Task<bool> IsInternetAvailable(int timeout = 3)
    {
        // Kiểm tra nhanh phần cứng (Wifi/Mobile data có bật không)
        if (Application.internetReachability == NetworkReachability.NotReachable)
            return false;

        try
        {
            using (var request = UnityWebRequest.Get("https://google.com"))
            {
                request.timeout = timeout;
                var operation = request.SendWebRequest();

                // Chờ cho đến khi request hoàn thành
                while (!operation.isDone)
                    await Task.Yield();

                return request.result == UnityWebRequest.Result.Success;
            }
        }
        catch
        {
            return false;
        }
    }
    public static string ToKMB(this int num)
    {
        // Trường hợp số nhỏ hơn 1,000 -> Hiển thị nguyên gốc
        if (num < 1000) return num.ToString();

        // Trường hợp K (Nghìn)
        if (num < 1000000)
            return (num / 1000f).ToString("0.#") + "K";

        // Trường hợp M (Triệu)
        if (num < 1000000000)
            return (num / 1000000f).ToString("0.#") + "M";

        // Trường hợp B (Tỷ) - int tối đa khoảng 2 Tỷ nên chỉ đến đây là hết
        return (num / 1000000000f).ToString("0.#") + "B";
    }
}
