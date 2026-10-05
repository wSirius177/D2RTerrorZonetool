using System;

namespace D2RTerrorZone.Services
{
    public class CountdownService
    {
        /// <summary>
        /// 获取下一个整点（:00）或半点（:30）的时间边界
        /// </summary>
        public DateTimeOffset GetNextRefreshTime(DateTimeOffset now)
        {
            // 如果分钟 < 30，下一个边界是 :30
            if (now.Minute < 30)
            {
                return new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, 30, 0, now.Offset);
            }
            // 如果分钟 >= 30，下一个边界是下个整点
            else
            {
                var nextHour = now.AddHours(1);
                return new DateTimeOffset(nextHour.Year, nextHour.Month, nextHour.Day, nextHour.Hour, 0, 0, now.Offset);
            }
        }

        /// <summary>
        /// 获取距离下一次刷新的剩余时间戳 TimeSpan
        /// </summary>
        public TimeSpan GetRemainingTime(DateTimeOffset nextRefreshTime, DateTimeOffset now)
        {
            var remaining = nextRefreshTime - now;
            // 避免出现负数导致 UI 显示 -00:01 等
            return remaining.Ticks < 0 ? TimeSpan.Zero : remaining;
        }
    }
}
